using System.Data.Common;
using ExpenseTrackerTests.Services;
using ExpenseTrackerWebApp;
using ExpenseTrackerWebApp.Database;
using ExpenseTrackerWebApp.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using ExpenseTrackerWebApp.Database.Models;

namespace ExpenseTrackerTests.Tests
{
    public class DashboardServiceTests
    {

        private readonly DbConnection _connection;
        private readonly DbContextOptions<AppDbContext> _contextOptions;

        // has no accounts and no transactions
        private readonly string _aliceId;

        // has 1 account 
        // Bob's Cash Account with InitialBalance = 1000.0m
        // No transactions 
        private readonly string _bobId;


        // has 2 accounts
        // has two transactions this month, one income one expense
        // has five transctions last month, all expenses
        // has seven transctions two months ago, six expenses one income
        private readonly string _charlieId; 

        public DashboardServiceTests()
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
                var user1 = new IdentityUser
                {
                    Id = Guid.NewGuid().ToString(),
                    UserName = "alice@test.com",
                    NormalizedUserName = "ALICE@TEST.COM",
                    Email = "alice@test.com",
                    NormalizedEmail = "ALICE@TEST.COM",
                    EmailConfirmed = true
                };

                var user2 = new IdentityUser
                {
                    Id = Guid.NewGuid().ToString(),
                    UserName = "bob@test.com",
                    NormalizedUserName = "BOB@TEST.COM",
                    Email = "bob@test.com",
                    NormalizedEmail = "BOB@TEST.COM",
                    EmailConfirmed = true
                };


                var user3 = new IdentityUser
                {
                    Id = Guid.NewGuid().ToString(),
                    UserName = "charlie@test.com",
                    NormalizedUserName = "CHARLIE@TEST.COM",
                    Email = "charlie@test.com",
                    NormalizedEmail = "CHARLIE@TEST.COM",
                    EmailConfirmed = true
                };

                context.Users.AddRange(user1, user2, user3);
                context.SaveChanges();

                _aliceId = user1.Id;
                _bobId = user2.Id;
                _charlieId = user3.Id;

                var bobCashAccount = new Account { Name = "Bob's Cash", InitialBalance=1000.0m, IdentityUserId = _bobId };
                var charlieBankAccount = new Account { Name = "Charlie's Bank", InitialBalance = 1500.0m, IdentityUserId = _charlieId };
                var charlieCashAccount = new Account { Name = "Charlie's Cash", InitialBalance = 300.0m, IdentityUserId = _charlieId };

                context.Accounts.AddRange(bobCashAccount, charlieBankAccount, charlieCashAccount);
                context.SaveChanges();

                var shopping = new Category{Name = "Shopping", Type=TransactionType.Expense, IdentityUserId = _charlieId};
                var transporation = new Category{Name = "Transporation", Type=TransactionType.Expense, IdentityUserId = _charlieId};
                var subscriptions = new Category{Name = "Subscriptions", Type=TransactionType.Expense, IdentityUserId = _charlieId};
                var salary = new Category{Name = "Salary", Type=TransactionType.Income, IdentityUserId = _charlieId};

                context.Categories.AddRange(shopping, transporation, subscriptions, salary);
                context.SaveChanges();

                // shopping total: 7m
                // subscriptions total: 3m
                // transporation total: 4m
                // salary total: 7m
                var charlieTransction1 = new Transaction(){Amount=-1m, Date=DateTime.Now, AccountId=charlieBankAccount.Id, CategoryId = shopping.Id};
                var charlieTransaction2 = new Transaction(){Amount=2m, Date=DateTime.Now, AccountId=charlieBankAccount.Id, CategoryId = salary.Id};

                var charlieTransaction3 = new Transaction(){Amount=-5m, Date=DateTime.Now.AddMonths(-1), AccountId=charlieBankAccount.Id, CategoryId = shopping.Id};
                var charlieTransaction4 = new Transaction(){Amount=-3m, Date=DateTime.Now.AddMonths(-1), AccountId=charlieBankAccount.Id, CategoryId = transporation.Id};
                var charlieTransaction5 = new Transaction(){Amount=-1m, Date=DateTime.Now.AddMonths(-1), AccountId=charlieBankAccount.Id, CategoryId = subscriptions.Id};

                var charlieTransaction6 = new Transaction(){Amount=-1m, Date=DateTime.Now.AddMonths(-2), AccountId=charlieBankAccount.Id, CategoryId = shopping.Id};
                var charlieTransaction7 = new Transaction(){Amount=-1m, Date=DateTime.Now.AddMonths(-2), AccountId=charlieBankAccount.Id, CategoryId = transporation.Id};
                var charlieTransaction8 = new Transaction(){Amount=-2m, Date=DateTime.Now.AddMonths(-2), AccountId=charlieBankAccount.Id, CategoryId = subscriptions.Id};
                var charlieTransaction9 = new Transaction(){Amount=5m, Date=DateTime.Now.AddMonths(-2), AccountId=charlieBankAccount.Id, CategoryId = salary.Id};

                var _charlieTransactions = new List<Transaction>()
                {
                    charlieTransction1, charlieTransaction2, charlieTransaction3, charlieTransaction4,
                    charlieTransaction5, charlieTransaction6, charlieTransaction7,
                    charlieTransaction8, charlieTransaction9
                };

                context.Transactions.AddRange(_charlieTransactions);
                context.SaveChanges();
            }
        }

        
        AppDbContext CreateContext() => new AppDbContext(_contextOptions);

        public void Dispose() => _connection.Dispose();

        private AccountService CreateAccountService(string userId)
        {
            var context = CreateContext();
            var currentUserService = new TestCurrentUserService(userId);
            return new AccountService(context, currentUserService);
        }

        public TransactionService CreateTransactionService(string userId)
        {
            var context = CreateContext();
            var currentUserService = new TestCurrentUserService(userId);
            return new TransactionService(context, currentUserService);
        }

        public DashboardService CreateDashboardService(string userId)
        {
            var accountService = CreateAccountService(userId);
            var transactionService = CreateTransactionService(userId);
            return new DashboardService(transactionService, accountService);
        }
        
        [Fact]
        public async Task GetDashboardSummary_AliceLoggedIn_ReturnsEmptySummary()
        {
            var dashboardService = CreateDashboardService(_aliceId);

            var result = await dashboardService.GetDashboardSummaryAsync();

            Assert.Equal(0.0m, result.Balance);
            Assert.Equal(0.0m, result.ExpensesThisMonth);
            Assert.Equal(0.0m, result.IncomeThisMonth);
            Assert.Empty(result.TopExpenseCategories);
            Assert.Equal(6, result.CumulativeExpensesPerMonth.Count);
            foreach(var monthData in result.CumulativeExpensesPerMonth)
            {
                Assert.Equal(0.0m, monthData.Amount);
            }
            Assert.Equal(6, result.CumulativeIncomePerMonth.Count);
            foreach(var monthData in result.CumulativeIncomePerMonth)
            {
                Assert.Equal(0.0m, monthData.Amount);
            }
        }


        [Fact]
        public async Task GetDashboardSummary_BobLoggedIn_ReturnsSummaryWithAccountBalanceOnly()
        {
            var dashboardService = CreateDashboardService(_bobId);

            var result = await dashboardService.GetDashboardSummaryAsync();

            Assert.Equal(1000.0m, result.Balance);
            Assert.Equal(0.0m, result.ExpensesThisMonth);
            Assert.Equal(0.0m, result.IncomeThisMonth);
            Assert.Empty(result.TopExpenseCategories);
            Assert.Equal(6, result.CumulativeExpensesPerMonth.Count);
            foreach(var monthData in result.CumulativeExpensesPerMonth)
            {
                Assert.Equal(0.0m, monthData.Amount);
            }
            Assert.Equal(6, result.CumulativeIncomePerMonth.Count);
            foreach(var monthData in result.CumulativeIncomePerMonth)
            {
                Assert.Equal(0.0m, monthData.Amount);
            }
        }

        [Fact]
        public async Task GetDashboardSummary_CharlieLoggedIn__TwoTopCategories_LastMonthReturnsCorrectSummary()
        {
            var dashboardService = CreateDashboardService(_charlieId);

            var result = await dashboardService.GetDashboardSummaryAsync(1, 2);

            Assert.Equal(1793m, result.Balance);
            Assert.Equal(1m, result.ExpensesThisMonth);
            Assert.Equal(2m, result.IncomeThisMonth);

            Assert.Equal(3, result.TopExpenseCategories.Count);
            var transporationCategory = result.TopExpenseCategories.FirstOrDefault(c => c.Category == "Transporation");
            Assert.Equal(3m, transporationCategory.Amount);

            var shoppingCategory = result.TopExpenseCategories.FirstOrDefault(c => c.Category == "Shopping");
            Assert.Equal(6m, shoppingCategory.Amount);

            var otherCategory = result.TopExpenseCategories.FirstOrDefault(c => c.Category == "Other");
            Assert.Equal(1m, otherCategory.Amount);


            Assert.Equal(2, result.CumulativeExpensesPerMonth.Count);
            var thisMonthExpenseData = result.CumulativeExpensesPerMonth.FirstOrDefault(c => c.TimePeriod == DateTime.Now.ToString("MMM"));
            Assert.Equal(1m, thisMonthExpenseData.Amount);

            var lastMonthExpenseData = result.CumulativeExpensesPerMonth.FirstOrDefault(c => c.TimePeriod == DateTime.Now.AddMonths(-1).ToString("MMM"));
            Assert.Equal(9m, lastMonthExpenseData.Amount);

            Assert.Equal(1, result.CumulativeIncomePerMonth.Count);
            var thisMonthIncomeData = result.CumulativeIncomePerMonth.FirstOrDefault(c => c.TimePeriod == DateTime.Now.ToString("MMM"));
            Assert.Equal(2m, thisMonthIncomeData.Amount);

        }

    }

}