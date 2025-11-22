using System;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using ExpenseTrackerTests.Services;
using ExpenseTrackerWebApp.Database;
using ExpenseTrackerWebApp.Database.Models;
using ExpenseTrackerWebApp.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ExpenseTrackerTests.Tests;

public class TagServiceTests : IDisposable
{
    private readonly DbConnection _connection;
    private readonly DbContextOptions<AppDbContext> _contextOptions;

    private readonly string _userId = null!;
    private readonly string _otherUserId = null!;

    private readonly int _transactionId;
    private readonly int _otherUserTransactionId;

    public TagServiceTests()
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

            var otherUser = new IdentityUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = "other@test.com",
                NormalizedUserName = "OTHER@TEST.COM",
                Email = "other@test.com",
                NormalizedEmail = "OTHER@TEST.COM",
                EmailConfirmed = true
            };

            context.Users.AddRange(user, otherUser);
            context.SaveChanges();

            _userId = user.Id;
            _otherUserId = otherUser.Id;

            // Create accounts for both users
            var userAccount = new Account { Name = "User Account", InitialBalance = 1000.0m, IdentityUserId = _userId };
            var otherUserAccount = new Account { Name = "Other Account", InitialBalance = 1000.0m, IdentityUserId = _otherUserId };

            context.Accounts.AddRange(userAccount, otherUserAccount);
            context.SaveChanges();

            // Create categories for both users
            var userCategory = new Category { Name = "Expense", Type = TransactionType.Expense, IdentityUserId = _userId };
            var otherUserCategory = new Category { Name = "Expense", Type = TransactionType.Expense, IdentityUserId = _otherUserId };

            context.Categories.AddRange(userCategory, otherUserCategory);
            context.SaveChanges();

            // Create transactions for both users
            var userTransaction = new Transaction
            {
                Amount = -50.0m,
                Date = DateTime.Now,
                AccountId = userAccount.Id,
                CategoryId = userCategory.Id
            };

            var otherUserTransaction = new Transaction
            {
                Amount = -75.0m,
                Date = DateTime.Now,
                AccountId = otherUserAccount.Id,
                CategoryId = otherUserCategory.Id
            };

            context.Transactions.AddRange(userTransaction, otherUserTransaction);
            context.SaveChanges();

            _transactionId = userTransaction.Id;
            _otherUserTransactionId = otherUserTransaction.Id;
        }
    }

    public void Dispose()
    {
        _connection?.Dispose();
    }

    [Fact]
    public async Task SaveAsync_ShouldCreateNewTag()
    {
        using var context = new AppDbContext(_contextOptions);
        var currentUserService = new TestCurrentUserService(_userId);
        var tagService = new TagService(context, currentUserService);

        var tag = await tagService.SaveAsync("Work", "#FF5733");

        Assert.NotNull(tag);
        Assert.Equal("Work", tag.Name);
        Assert.Equal("#FF5733", tag.Color);
        Assert.Equal(_userId, tag.IdentityUserId);
    }

    [Fact]
    public async Task SaveAsync_ShouldReturnExistingTagIfDuplicate()
    {
        using var context = new AppDbContext(_contextOptions);
        var currentUserService = new TestCurrentUserService(_userId);
        var tagService = new TagService(context, currentUserService);

        var tag1 = await tagService.SaveAsync("Work", "#FF5733");
        var tag2 = await tagService.SaveAsync("Work", "#000000"); // Different color, same name

        Assert.Equal(tag1.Id, tag2.Id);
        Assert.Equal(tag1.Name, tag2.Name);
    }

    [Fact]
    public async Task SaveAsync_ShouldUseDefaultColorIfNotProvided()
    {
        using var context = new AppDbContext(_contextOptions);
        var currentUserService = new TestCurrentUserService(_userId);
        var tagService = new TagService(context, currentUserService);

        var tag = await tagService.SaveAsync("Personal");

        Assert.Equal("#9E9E9E", tag.Color);
    }

    [Fact]
    public async Task SaveAsync_ShouldThrowExceptionIfNameIsEmpty()
    {
        using var context = new AppDbContext(_contextOptions);
        var currentUserService = new TestCurrentUserService(_userId);
        var tagService = new TagService(context, currentUserService);

        await Assert.ThrowsAsync<ArgumentException>(() => tagService.SaveAsync(""));
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnOnlyCurrentUserTags()
    {
        using var context = new AppDbContext(_contextOptions);
        var currentUserService = new TestCurrentUserService(_userId);
        var tagService = new TagService(context, currentUserService);

        // Create tags for current user
        await tagService.SaveAsync("Work");
        await tagService.SaveAsync("Personal");

        // Create tags for other user
        var otherUserService = new TestCurrentUserService(_otherUserId);
        var otherTagService = new TagService(context, otherUserService);
        await otherTagService.SaveAsync("Business");
        await otherTagService.SaveAsync("Home");

        // Get tags for current user
        var tags = await tagService.GetAllAsync();

        Assert.Equal(2, tags.Count);
        Assert.All(tags, t => Assert.Equal(_userId, t.IdentityUserId));
        Assert.Contains(tags, t => t.Name == "Work");
        Assert.Contains(tags, t => t.Name == "Personal");
    }

    [Fact]
    public async Task GetByNameAsync_ShouldReturnTagByName()
    {
        using var context = new AppDbContext(_contextOptions);
        var currentUserService = new TestCurrentUserService(_userId);
        var tagService = new TagService(context, currentUserService);

        await tagService.SaveAsync("Work", "#FF5733");
        var tag = await tagService.GetByNameAsync("Work");

        Assert.NotNull(tag);
        Assert.Equal("Work", tag.Name);
    }

    [Fact]
    public async Task GetByNameAsync_ShouldReturnNullIfNotFound()
    {
        using var context = new AppDbContext(_contextOptions);
        var currentUserService = new TestCurrentUserService(_userId);
        var tagService = new TagService(context, currentUserService);

        var tag = await tagService.GetByNameAsync("NonExistent");

        Assert.Null(tag);
    }

    [Fact]
    public async Task SetTransactionTagsAsync_ShouldAddTagsToTransaction()
    {
        using var context = new AppDbContext(_contextOptions);
        var currentUserService = new TestCurrentUserService(_userId);
        var tagService = new TagService(context, currentUserService);

        var tag1 = await tagService.SaveAsync("Work");
        var tag2 = await tagService.SaveAsync("Personal");

        await tagService.SetTransactionTagsAsync(_transactionId, new List<int> { tag1.Id, tag2.Id });

        var transaction = await context.Transactions
            .Include(t => t.TransactionTags)
            .ThenInclude(tt => tt.Tag)
            .FirstAsync(t => t.Id == _transactionId);

        Assert.Equal(2, transaction.TransactionTags.Count);
        Assert.Contains(transaction.TransactionTags, tt => tt.TagId == tag1.Id);
        Assert.Contains(transaction.TransactionTags, tt => tt.TagId == tag2.Id);
    }

    [Fact]
    public async Task SetTransactionTagsAsync_ShouldNotDuplicateExistingTag()
    {
        using var context = new AppDbContext(_contextOptions);
        var currentUserService = new TestCurrentUserService(_userId);
        var tagService = new TagService(context, currentUserService);

        var tag = await tagService.SaveAsync("Work");
        
        // Add same tag twice via SetTransactionTagsAsync (passing same ID twice)
        await tagService.SetTransactionTagsAsync(_transactionId, new List<int> { tag.Id, tag.Id });

        var transaction = await context.Transactions
            .Include(t => t.TransactionTags)
            .FirstAsync(t => t.Id == _transactionId);

        // Only one entry should exist (duplicate prevention in database via composite key)
        Assert.Single(transaction.TransactionTags);
    }

    [Fact]
    public async Task SetTransactionTagsAsync_ShouldReplaceAllTags()
    {
        using var context = new AppDbContext(_contextOptions);
        var currentUserService = new TestCurrentUserService(_userId);
        var tagService = new TagService(context, currentUserService);

        // Add initial tags
        var tag1 = await tagService.SaveAsync("Work");
        var tag2 = await tagService.SaveAsync("Personal");
        var tag3 = await tagService.SaveAsync("Urgent");

        await tagService.SetTransactionTagsAsync(_transactionId, new List<int> { tag1.Id, tag2.Id });

        // Verify initial state
        var transaction = await context.Transactions
            .Include(t => t.TransactionTags)
            .FirstAsync(t => t.Id == _transactionId);
        Assert.Equal(2, transaction.TransactionTags.Count);

        // Replace with new set
        await tagService.SetTransactionTagsAsync(_transactionId, new List<int> { tag3.Id });

        // Verify new state
        transaction = await context.Transactions
            .Include(t => t.TransactionTags)
            .FirstAsync(t => t.Id == _transactionId);
        Assert.Single(transaction.TransactionTags);
        Assert.Equal(tag3.Id, transaction.TransactionTags.First().TagId);
    }

    [Fact]
    public async Task SetTransactionTagsAsync_ShouldClearAllTagsWhenPassedEmptyList()
    {
        using var context = new AppDbContext(_contextOptions);
        var currentUserService = new TestCurrentUserService(_userId);
        var tagService = new TagService(context, currentUserService);

        var tag = await tagService.SaveAsync("Work");
        await tagService.SetTransactionTagsAsync(_transactionId, new List<int> { tag.Id });

        // Verify tag was added
        var transaction = await context.Transactions
            .Include(t => t.TransactionTags)
            .FirstAsync(t => t.Id == _transactionId);
        Assert.Single(transaction.TransactionTags);

        // Clear tags
        await tagService.SetTransactionTagsAsync(_transactionId, new List<int> { });

        // Verify tags were removed
        transaction = await context.Transactions
            .Include(t => t.TransactionTags)
            .FirstAsync(t => t.Id == _transactionId);
        Assert.Empty(transaction.TransactionTags);
    }

    [Fact]
    public async Task SetTransactionTagsAsync_ShouldHandleMultipleAdditionsAfterDelete()
    {
        using var context = new AppDbContext(_contextOptions);
        var currentUserService = new TestCurrentUserService(_userId);
        var tagService = new TagService(context, currentUserService);

        var tag1 = await tagService.SaveAsync("Work");
        var tag2 = await tagService.SaveAsync("Personal");

        // First set: add one tag
        await tagService.SetTransactionTagsAsync(_transactionId, new List<int> { tag1.Id });

        // Second set: add a different tag (this replaces the previous one)
        await tagService.SetTransactionTagsAsync(_transactionId, new List<int> { tag2.Id });

        // Third set: add multiple tags at once
        await tagService.SetTransactionTagsAsync(_transactionId, new List<int> { tag1.Id, tag2.Id });

        var transaction = await context.Transactions
            .Include(t => t.TransactionTags)
            .FirstAsync(t => t.Id == _transactionId);

        Assert.Equal(2, transaction.TransactionTags.Count);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteTag()
    {
        using var context = new AppDbContext(_contextOptions);
        var currentUserService = new TestCurrentUserService(_userId);
        var tagService = new TagService(context, currentUserService);

        var tag = await tagService.SaveAsync("Work");
        await tagService.DeleteAsync(tag.Id);

        var tags = await tagService.GetAllAsync();
        Assert.DoesNotContain(tags, t => t.Id == tag.Id);
    }

    [Fact]
    public async Task UserIsolation_ShouldNotAllowAccessToOtherUsersTags()
    {
        using var context = new AppDbContext(_contextOptions);
        var currentUserService = new TestCurrentUserService(_userId);
        var otherUserService = new TestCurrentUserService(_otherUserId);

        var userTagService = new TagService(context, currentUserService);
        var otherUserTagService = new TagService(context, otherUserService);

        var userTag = await userTagService.SaveAsync("Private");
        var otherUserTags = await otherUserTagService.GetAllAsync();

        Assert.DoesNotContain(otherUserTags, t => t.Id == userTag.Id);
    }
}
