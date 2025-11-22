using System;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using ExpenseTrackerTests.Services;
using ExpenseTrackerWebApp.Database;
using ExpenseTrackerWebApp.Database.Models;
using ExpenseTrackerWebApp.Dtos;
using ExpenseTrackerWebApp.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ExpenseTrackerTests.Tests;

public class RecurringTransactionsTests : IDisposable
{
    private readonly DbConnection _connection;
    private readonly DbContextOptions<AppDbContext> _contextOptions;

    private string _userId = null!;
    private int _accountId;
    private int _categoryId;

    public RecurringTransactionsTests()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        _contextOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .EnableSensitiveDataLogging()
            .Options;

        using var context = new AppDbContext(_contextOptions);

        if (context.Database.EnsureCreated())
        {
            var user = new IdentityUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = "user@test.com",
                NormalizedUserName = "USER@TEST.COM",
                Email = "user@test.com",
                NormalizedEmail = "USER@TEST.COM",
                EmailConfirmed = true
            };

            context.Users.Add(user);
            context.SaveChanges();

            _userId = user.Id;

            // Create account
            var account = new Account 
            { 
                Name = "Test Account", 
                InitialBalance = 1000.0m, 
                IdentityUserId = _userId 
            };
            context.Accounts.Add(account);
            context.SaveChanges();
            _accountId = account.Id;

            // Create category
            var category = new Category 
            { 
                Name = "Test Category", 
                Type = TransactionType.Expense, 
                IdentityUserId = _userId 
            };
            context.Categories.Add(category);
            context.SaveChanges();
            _categoryId = category.Id;
        }
    }

    public void Dispose()
    {
        _connection?.Dispose();
    }

    private TransactionService CreateTransactionService(AppDbContext context)
    {
        var currentUserService = new TestCurrentUserService(_userId);
        var tagService = new TagService(context, currentUserService);
        return new TransactionService(context, currentUserService, 
            new AccountService(context, currentUserService),
            new CategoryService(context, currentUserService),
            tagService);
    }

    [Fact]
    public async Task SaveAsync_WithMonthlyRecurring_SetsNextOccurrenceDateToNextMonth()
    {
        using var context = new AppDbContext(_contextOptions);
        var service = CreateTransactionService(context);

        var date = new DateTime(2025, 11, 1);
        var transactionDto = new TransactionDto
        {
            Amount = 100,
            Date = date,
            Time = TimeSpan.Zero,
            AccountId = _accountId,
            CategoryId = _categoryId,
            TransactionType = TransactionType.Expense,
            Description = "Monthly subscription",
            Reoccuring = true,
            ReoccuranceFrequency = ReoccuranceFrequency.Monthly
        };

        await service.SaveAsync(transactionDto);

        var savedTransaction = await context.Transactions.FirstAsync();
        Assert.Equal(new DateTime(2025, 12, 1), savedTransaction.NextReoccuranceDate);
    }

    [Fact]
    public async Task SaveAsync_WithWeeklyRecurring_SetsNextOccurrenceDateToNextWeek()
    {
        using var context = new AppDbContext(_contextOptions);
        var service = CreateTransactionService(context);

        var date = new DateTime(2025, 11, 1); // Saturday
        var transactionDto = new TransactionDto
        {
            Amount = 50,
            Date = date,
            Time = TimeSpan.Zero,
            AccountId = _accountId,
            CategoryId = _categoryId,
            TransactionType = TransactionType.Expense,
            Description = "Weekly gym",
            Reoccuring = true,
            ReoccuranceFrequency = ReoccuranceFrequency.Weekly
        };

        await service.SaveAsync(transactionDto);

        var savedTransaction = await context.Transactions.FirstAsync();
        Assert.Equal(new DateTime(2025, 11, 8), savedTransaction.NextReoccuranceDate);
    }

    [Fact]
    public async Task SaveAsync_WithDailyRecurring_SetsNextOccurrenceDateToTomorrow()
    {
        using var context = new AppDbContext(_contextOptions);
        var service = CreateTransactionService(context);

        var date = new DateTime(2025, 11, 1);
        var transactionDto = new TransactionDto
        {
            Amount = 10,
            Date = date,
            Time = TimeSpan.Zero,
            AccountId = _accountId,
            CategoryId = _categoryId,
            TransactionType = TransactionType.Expense,
            Description = "Daily coffee",
            Reoccuring = true,
            ReoccuranceFrequency = ReoccuranceFrequency.Daily
        };

        await service.SaveAsync(transactionDto);

        var savedTransaction = await context.Transactions.FirstAsync();
        Assert.Equal(new DateTime(2025, 11, 2), savedTransaction.NextReoccuranceDate);
    }

    [Fact]
    public async Task SaveAsync_WithYearlyRecurring_SetsNextOccurrenceDateToNextYear()
    {
        using var context = new AppDbContext(_contextOptions);
        var service = CreateTransactionService(context);

        var date = new DateTime(2025, 11, 1);
        var transactionDto = new TransactionDto
        {
            Amount = 500,
            Date = date,
            Time = TimeSpan.Zero,
            AccountId = _accountId,
            CategoryId = _categoryId,
            TransactionType = TransactionType.Expense,
            Description = "Annual insurance",
            Reoccuring = true,
            ReoccuranceFrequency = ReoccuranceFrequency.Yearly
        };

        await service.SaveAsync(transactionDto);

        var savedTransaction = await context.Transactions.FirstAsync();
        Assert.Equal(new DateTime(2026, 11, 1), savedTransaction.NextReoccuranceDate);
    }

    [Fact]
    public async Task CheckForReoccuringTransactions_CreatesTransactionWhenDue()
    {
        using var context = new AppDbContext(_contextOptions);
        var service = CreateTransactionService(context);

        // Create a recurring transaction with next occurrence today
        var recurringDate = DateTime.Today.AddDays(-1); // Yesterday
        var transaction = new Transaction
        {
            Amount = -100,
            Date = recurringDate,
            AccountId = _accountId,
            CategoryId = _categoryId,
            Description = "Monthly bill",
            IsReoccuring = true,
            ReoccuranceFrequency = ReoccuranceFrequency.Monthly,
            NextReoccuranceDate = DateTime.Today // Due today
        };

        await context.Transactions.AddAsync(transaction);
        await context.SaveChangesAsync();

        // Run the check
        await service.CheckForReoccuringTransactions();

        // Should have 2 transactions: original + 1 created
        var allTransactions = await context.Transactions.OrderBy(t => t.Date).ToListAsync();
        Assert.Equal(2, allTransactions.Count);

        // New transaction should be created with today's date
        var newTransaction = allTransactions[1];
        Assert.Equal(DateTime.Today, newTransaction.Date);
        Assert.False(newTransaction.IsReoccuring);
        Assert.Equal(100, Math.Abs(newTransaction.Amount)); // Same amount
    }

    [Fact]
    public async Task CheckForReoccuringTransactions_CreatesMultipleTransactionsForGap()
    {
        using var context = new AppDbContext(_contextOptions);
        var service = CreateTransactionService(context);

        // Create a recurring transaction with next occurrence 3 months ago
        var startDate = DateTime.Today.AddMonths(-3);
        var transaction = new Transaction
        {
            Amount = -50,
            Date = startDate,
            AccountId = _accountId,
            CategoryId = _categoryId,
            Description = "Monthly subscription",
            IsReoccuring = true,
            ReoccuranceFrequency = ReoccuranceFrequency.Monthly,
            NextReoccuranceDate = startDate.AddMonths(1) // 2 months overdue
        };

        await context.Transactions.AddAsync(transaction);
        await context.SaveChangesAsync();

        // Run the check
        await service.CheckForReoccuringTransactions();

        // Should have created 3 transactions (for the 3 months since the original date up to today)
        // Original on Aug 22, then Sept 22, Oct 22, Nov 22 (if today is Nov 22 or later)
        var allTransactions = await context.Transactions.OrderBy(t => t.Date).ToListAsync();
        Assert.Equal(4, allTransactions.Count); // 1 original + 3 created

        // Verify the original transaction is still there
        var originalTransaction = allTransactions[0];
        Assert.True(originalTransaction.IsReoccuring);
        
        // Verify created transactions are not marked as recurring
        var createdTransactions = allTransactions.Skip(1).ToList();
        Assert.All(createdTransactions, t => Assert.False(t.IsReoccuring));
        
        // Verify dates are correct (next occurrence dates that have passed)
        Assert.Equal(startDate.AddMonths(1), createdTransactions[0].Date);
        Assert.Equal(startDate.AddMonths(2), createdTransactions[1].Date);
        Assert.Equal(startDate.AddMonths(3), createdTransactions[2].Date);
    }

    [Fact]
    public async Task CheckForReoccuringTransactions_UpdatesNextOccurrenceDateAfterCreation()
    {
        using var context = new AppDbContext(_contextOptions);
        var service = CreateTransactionService(context);

        var recurringDate = DateTime.Today.AddMonths(-1);
        var transaction = new Transaction
        {
            Amount = -100,
            Date = recurringDate,
            AccountId = _accountId,
            CategoryId = _categoryId,
            Description = "Monthly bill",
            IsReoccuring = true,
            ReoccuranceFrequency = ReoccuranceFrequency.Monthly,
            NextReoccuranceDate = DateTime.Today
        };

        await context.Transactions.AddAsync(transaction);
        await context.SaveChangesAsync();
        var transactionId = transaction.Id;

        // Run the check
        await service.CheckForReoccuringTransactions();

        // Reload and verify next occurrence is updated
        var updatedTransaction = await context.Transactions.FirstAsync(t => t.Id == transactionId);
        var expectedNextDate = DateTime.Today.AddMonths(1);
        Assert.Equal(expectedNextDate, updatedTransaction.NextReoccuranceDate);
    }

    [Fact]
    public async Task CheckForReoccuringTransactions_DoesNotCreateTransactionIfNotDue()
    {
        using var context = new AppDbContext(_contextOptions);
        var service = CreateTransactionService(context);

        // Create a recurring transaction with next occurrence in the future
        var futureDate = DateTime.Today.AddMonths(1);
        var transaction = new Transaction
        {
            Amount = -100,
            Date = DateTime.Today,
            AccountId = _accountId,
            CategoryId = _categoryId,
            Description = "Future bill",
            IsReoccuring = true,
            ReoccuranceFrequency = ReoccuranceFrequency.Monthly,
            NextReoccuranceDate = futureDate
        };

        await context.Transactions.AddAsync(transaction);
        await context.SaveChangesAsync();

        // Run the check
        await service.CheckForReoccuringTransactions();

        // Should still have only 1 transaction
        var allTransactions = await context.Transactions.ToListAsync();
        Assert.Single(allTransactions);
    }

    [Fact]
    public async Task CheckForReoccuringTransactions_PreservesOriginalTransactionData()
    {
        using var context = new AppDbContext(_contextOptions);
        var service = CreateTransactionService(context);

        const string description = "Test recurring with description";
        var transaction = new Transaction
        {
            Amount = -75.50m,
            Date = DateTime.Today.AddMonths(-1),
            AccountId = _accountId,
            CategoryId = _categoryId,
            Description = description,
            IsReoccuring = true,
            ReoccuranceFrequency = ReoccuranceFrequency.Monthly,
            NextReoccuranceDate = DateTime.Today
        };

        await context.Transactions.AddAsync(transaction);
        await context.SaveChangesAsync();

        await service.CheckForReoccuringTransactions();

        var createdTransaction = await context.Transactions.OrderByDescending(t => t.Id).FirstAsync();
        Assert.Equal(description, createdTransaction.Description);
        Assert.Equal(-75.50m, createdTransaction.Amount);
        Assert.Equal(_accountId, createdTransaction.AccountId);
        Assert.Equal(_categoryId, createdTransaction.CategoryId);
    }

    [Fact]
    public async Task CheckForReoccuringTransactions_WithTags_PreservesTags()
    {
        using var context = new AppDbContext(_contextOptions);
        var currentUserService = new TestCurrentUserService(_userId);
        var tagService = new TagService(context, currentUserService);
        var service = new TransactionService(context, currentUserService,
            new AccountService(context, currentUserService),
            new CategoryService(context, currentUserService),
            tagService);

        // Create a tag
        var tag = await tagService.SaveAsync("Monthly Bill", "#FF5733");

        // Create recurring transaction with tag
        var transaction = new Transaction
        {
            Amount = -100,
            Date = DateTime.Today.AddMonths(-1),
            AccountId = _accountId,
            CategoryId = _categoryId,
            Description = "Bill with tag",
            IsReoccuring = true,
            ReoccuranceFrequency = ReoccuranceFrequency.Monthly,
            NextReoccuranceDate = DateTime.Today,
            TransactionTags = new List<TransactionTag>
            {
                new TransactionTag { TagId = tag.Id }
            }
        };

        await context.Transactions.AddAsync(transaction);
        await context.SaveChangesAsync();

        await service.CheckForReoccuringTransactions();

        // Verify new transaction has the same tag
        var createdTransaction = await context.Transactions
            .Include(t => t.TransactionTags)
            .OrderByDescending(t => t.Id)
            .FirstAsync();

        Assert.Single(createdTransaction.TransactionTags);
        Assert.Equal(tag.Id, createdTransaction.TransactionTags.First().TagId);
    }

    [Fact]
    public async Task CheckForReoccuringTransactions_SkipsNonRecurringTransactions()
    {
        using var context = new AppDbContext(_contextOptions);
        var service = CreateTransactionService(context);

        // Create a non-recurring transaction
        var transaction = new Transaction
        {
            Amount = -50,
            Date = DateTime.Today.AddMonths(-3),
            AccountId = _accountId,
            CategoryId = _categoryId,
            Description = "One-time expense",
            IsReoccuring = false,
            ReoccuranceFrequency = null,
            NextReoccuranceDate = null
        };

        await context.Transactions.AddAsync(transaction);
        await context.SaveChangesAsync();

        // Run the check
        await service.CheckForReoccuringTransactions();

        // Should still have only 1 transaction (no new ones created)
        var allTransactions = await context.Transactions.ToListAsync();
        Assert.Single(allTransactions);
    }

    [Fact]
    public async Task GetAllAsync_WithReoccuringParameter_FiltersCorrectly()
    {
        using var context = new AppDbContext(_contextOptions);
        var service = CreateTransactionService(context);

        // Create recurring and non-recurring transactions
        var recurring = new Transaction
        {
            Amount = -100,
            Date = DateTime.Today,
            AccountId = _accountId,
            CategoryId = _categoryId,
            IsReoccuring = true,
            ReoccuranceFrequency = ReoccuranceFrequency.Monthly,
            NextReoccuranceDate = DateTime.Today.AddMonths(1)
        };

        var nonRecurring = new Transaction
        {
            Amount = -50,
            Date = DateTime.Today,
            AccountId = _accountId,
            CategoryId = _categoryId,
            IsReoccuring = false
        };

        await context.Transactions.AddAsync(recurring);
        await context.Transactions.AddAsync(nonRecurring);
        await context.SaveChangesAsync();

        // Get only recurring
        var recurringTransactions = await service.GetAllAsync(true);
        Assert.Single(recurringTransactions);
        Assert.True(recurringTransactions.First().IsReoccuring);

        // Get all
        var allTransactions = await service.GetAllAsync(false);
        Assert.Equal(2, allTransactions.Count);
    }
}
