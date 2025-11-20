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
using MudBlazor.Extensions;
using System.Globalization;

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

        private readonly string _daveId;
        private readonly string _eveId;

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
                var alice = new IdentityUser
                {
                    Id = Guid.NewGuid().ToString(),
                    UserName = "alice@test.com",
                    NormalizedUserName = "ALICE@TEST.COM",
                    Email = "alice@test.com",
                    NormalizedEmail = "ALICE@TEST.COM",
                    EmailConfirmed = true
                };

                var bob = new IdentityUser
                {
                    Id = Guid.NewGuid().ToString(),
                    UserName = "bob@test.com",
                    NormalizedUserName = "BOB@TEST.COM",
                    Email = "bob@test.com",
                    NormalizedEmail = "BOB@TEST.COM",
                    EmailConfirmed = true
                };


                var charlie = new IdentityUser
                {
                    Id = Guid.NewGuid().ToString(),
                    UserName = "charlie@test.com",
                    NormalizedUserName = "CHARLIE@TEST.COM",
                    Email = "charlie@test.com",
                    NormalizedEmail = "CHARLIE@TEST.COM",
                    EmailConfirmed = true
                };


                var dave = new IdentityUser
                {
                    Id = Guid.NewGuid().ToString(),
                    UserName = "dave@test.com",
                    NormalizedUserName = "DAVE@TEST.COM",
                    Email = "dave@test.com",
                    NormalizedEmail = "DAVE@TEST.COM",
                    EmailConfirmed = true
                };


                var eve = new IdentityUser
                {
                    Id = Guid.NewGuid().ToString(),
                    UserName = "eve@test.com",
                    NormalizedUserName = "EVE@TEST.COM",
                    Email = "eve@test.com",
                    NormalizedEmail = "EVE@TEST.COM",
                    EmailConfirmed = true
                };

                context.Users.AddRange(alice, bob, charlie, dave, eve);
                context.SaveChanges();

                _aliceId = alice.Id;
                _bobId = bob.Id;
                _charlieId = charlie.Id;
                _daveId = dave.Id;
                _eveId = eve.Id;

                var bobCashAccount = new Account { Name = "Bob's Cash", InitialBalance = 1000.0m, IdentityUserId = _bobId };
                var charlieBankAccount = new Account { Name = "Charlie's Bank", InitialBalance = 1500.0m, IdentityUserId = _charlieId };
                var charlieCashAccount = new Account { Name = "Charlie's Cash", InitialBalance = 300.0m, IdentityUserId = _charlieId };
                var daveCashAccount = new Account { Name = "Dave's Cash", InitialBalance = 500.0m, IdentityUserId = _daveId };
                var eveBankAccount = new Account { Name = "Eve's Bank", InitialBalance = 3000.0m, IdentityUserId = _eveId };

                context.Accounts.AddRange(bobCashAccount, charlieBankAccount, charlieCashAccount, daveCashAccount, eveBankAccount);
                context.SaveChanges();

                var charlieShopping = new Category { Name = "Shopping", Type = TransactionType.Expense, IdentityUserId = _charlieId };
                var charlieTransporation = new Category { Name = "Transporation", Type = TransactionType.Expense, IdentityUserId = _charlieId };
                var charlieSubscription = new Category { Name = "Subscriptions", Type = TransactionType.Expense, IdentityUserId = _charlieId };
                var CharlieSalary = new Category { Name = "Salary", Type = TransactionType.Income, IdentityUserId = _charlieId };

                // Dave's categories
                var daveGroceries = new Category { Name = "Groceries", Type = TransactionType.Expense, IdentityUserId = _daveId };

                // Eve's categories
                var eveSalary = new Category { Name = "Salary", Type = TransactionType.Income, IdentityUserId = _eveId };
                var eveBonus = new Category { Name = "Bonus", Type = TransactionType.Income, IdentityUserId = _eveId };


                context.Categories.AddRange(charlieShopping, charlieTransporation, charlieSubscription, CharlieSalary, daveGroceries, eveSalary, eveBonus);
                context.SaveChanges();

                var charlieTransction1 = new Transaction() { Amount = -1m, Date = DateTime.Now.StartOfMonth(CultureInfo.CurrentCulture), AccountId = charlieBankAccount.Id, CategoryId = charlieShopping.Id };
                var charlieTransaction2 = new Transaction() { Amount = 2m, Date = DateTime.Now, AccountId = charlieBankAccount.Id, CategoryId = CharlieSalary.Id };

                var charlieTransaction3 = new Transaction() { Amount = -5m, Date = DateTime.Now.AddMonths(-1), AccountId = charlieBankAccount.Id, CategoryId = charlieShopping.Id };
                var charlieTransaction4 = new Transaction() { Amount = -3m, Date = DateTime.Now.AddMonths(-1), AccountId = charlieBankAccount.Id, CategoryId = charlieTransporation.Id };
                var charlieTransaction5 = new Transaction() { Amount = -1m, Date = DateTime.Now.AddMonths(-1), AccountId = charlieBankAccount.Id, CategoryId = charlieSubscription.Id };

                var charlieTransaction6 = new Transaction() { Amount = -1m, Date = DateTime.Now.AddMonths(-2), AccountId = charlieBankAccount.Id, CategoryId = charlieShopping.Id };
                var charlieTransaction7 = new Transaction() { Amount = -1m, Date = DateTime.Now.AddMonths(-2), AccountId = charlieBankAccount.Id, CategoryId = charlieTransporation.Id };
                var charlieTransaction8 = new Transaction() { Amount = -2m, Date = DateTime.Now.AddMonths(-2), AccountId = charlieBankAccount.Id, CategoryId = charlieSubscription.Id };
                var charlieTransaction9 = new Transaction() { Amount = 5m, Date = DateTime.Now.AddMonths(-2), AccountId = charlieBankAccount.Id, CategoryId = CharlieSalary.Id };

                var _charlieTransactions = new List<Transaction>()
                {
                    charlieTransction1, charlieTransaction2, charlieTransaction3, charlieTransaction4,
                    charlieTransaction5, charlieTransaction6, charlieTransaction7,
                    charlieTransaction8, charlieTransaction9
                };
                
                // Dave's transactions (grocery-heavy expenses)
                var daveTransaction1 = new Transaction() { Amount = -5m, Date = DateTime.Now, AccountId = daveCashAccount.Id, CategoryId = daveGroceries.Id };
                var daveTransaction2 = new Transaction() { Amount = -5m, Date = DateTime.Now.AddMonths(-1), AccountId = daveCashAccount.Id, CategoryId = daveGroceries.Id };
                var daveTransaction3 = new Transaction() { Amount = -10m, Date = DateTime.Now.AddMonths(-1), AccountId = daveCashAccount.Id, CategoryId = daveGroceries.Id };
                var daveTransaction4 = new Transaction() { Amount = -5m, Date = DateTime.Now.AddMonths(-2), AccountId = daveCashAccount.Id, CategoryId = daveGroceries.Id };

                // Eve's transactions (income only)
                var eveTransaction1 = new Transaction() { Amount = 10m, Date = DateTime.Now, AccountId = eveBankAccount.Id, CategoryId = eveSalary.Id };
                var eveTransaction2 = new Transaction() { Amount = 5m, Date = DateTime.Now, AccountId = eveBankAccount.Id, CategoryId = eveBonus.Id };
                var eveTransaction3 = new Transaction() { Amount = 10m, Date = DateTime.Now.AddMonths(-1), AccountId = eveBankAccount.Id, CategoryId = eveSalary.Id };
                var eveTransaction4 = new Transaction() { Amount = 5m, Date = DateTime.Now.AddMonths(-2), AccountId = eveBankAccount.Id, CategoryId = eveSalary.Id };



                context.Transactions.AddRange(_charlieTransactions);
                context.Transactions.AddRange(daveTransaction1, daveTransaction2, daveTransaction3, daveTransaction4, eveTransaction1, eveTransaction2, eveTransaction3, eveTransaction4);

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
            foreach (var monthData in result.CumulativeExpensesPerMonth)
            {
                Assert.Equal(0.0m, monthData.Amount);
            }
            Assert.Equal(6, result.CumulativeIncomePerMonth.Count);
            foreach (var monthData in result.CumulativeIncomePerMonth)
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
            foreach (var monthData in result.CumulativeExpensesPerMonth)
            {
                Assert.Equal(0.0m, monthData.Amount);
            }
            Assert.Equal(6, result.CumulativeIncomePerMonth.Count);
            foreach (var monthData in result.CumulativeIncomePerMonth)
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

            Assert.Equal(2, result.CumulativeIncomePerMonth.Count);
            var thisMonthIncomeData = result.CumulativeIncomePerMonth.FirstOrDefault(c => c.TimePeriod == DateTime.Now.ToString("MMM"));
            Assert.Equal(2m, thisMonthIncomeData.Amount);

            var lastMonthIncomeData = result.CumulativeIncomePerMonth.FirstOrDefault(c => c.TimePeriod == DateTime.Now.AddMonths(-1).ToString("MMM"));
            Assert.Equal(0m, lastMonthIncomeData.Amount);

        }

        [Fact]
        public async Task GetDashboardSummary_CharlieLoggedIn__TwoFiveCategories_LastThreeReturnsCorrectSummary()
        {
            var dashboardService = CreateDashboardService(_charlieId);

            var result = await dashboardService.GetDashboardSummaryAsync(3, 5);
            Assert.Equal(1793m, result.Balance);
            Assert.Equal(1m, result.ExpensesThisMonth);
            Assert.Equal(2m, result.IncomeThisMonth);

            Assert.Equal(3, result.TopExpenseCategories.Count);
            var transporationCategory = result.TopExpenseCategories.FirstOrDefault(c => c.Category == "Transporation");
            Assert.Equal(4m, transporationCategory.Amount);

            var shoppingCategory = result.TopExpenseCategories.FirstOrDefault(c => c.Category == "Shopping");
            Assert.Equal(7m, shoppingCategory.Amount);

            var otherCategory = result.TopExpenseCategories.FirstOrDefault(c => c.Category == "Subscriptions");
            Assert.Equal(3m, otherCategory.Amount);


            Assert.Equal(4, result.CumulativeExpensesPerMonth.Count);
            var thisMonthExpenseData = result.CumulativeExpensesPerMonth.FirstOrDefault(c => c.TimePeriod == DateTime.Now.ToString("MMM"));
            Assert.Equal(1m, thisMonthExpenseData.Amount);

            var lastMonthExpenseData = result.CumulativeExpensesPerMonth.FirstOrDefault(c => c.TimePeriod == DateTime.Now.AddMonths(-1).ToString("MMM"));
            Assert.Equal(9m, lastMonthExpenseData.Amount);

            var twoMonthsMonthExpenseData = result.CumulativeExpensesPerMonth.FirstOrDefault(c => c.TimePeriod == DateTime.Now.AddMonths(-2).ToString("MMM"));
            Assert.Equal(4m, twoMonthsMonthExpenseData.Amount);

            var threeMonthsMonthExpenseData = result.CumulativeExpensesPerMonth.FirstOrDefault(c => c.TimePeriod == DateTime.Now.AddMonths(-3).ToString("MMM"));
            Assert.Equal(0m, threeMonthsMonthExpenseData.Amount);


            Assert.Equal(4, result.CumulativeIncomePerMonth.Count);
            var thisMonthIncomeData = result.CumulativeIncomePerMonth.FirstOrDefault(c => c.TimePeriod == DateTime.Now.ToString("MMM"));
            Assert.Equal(2m, thisMonthIncomeData.Amount);

            var lastMonthIncomeData = result.CumulativeIncomePerMonth.FirstOrDefault(c => c.TimePeriod == DateTime.Now.AddMonths(-1).ToString("MMM"));
            Assert.Equal(0m, lastMonthIncomeData.Amount);

            var twoMonthIncomeData = result.CumulativeIncomePerMonth.FirstOrDefault(c => c.TimePeriod == DateTime.Now.AddMonths(-2).ToString("MMM"));
            Assert.Equal(5m, twoMonthIncomeData.Amount);

            var threeMonthIncomeData = result.CumulativeIncomePerMonth.FirstOrDefault(c => c.TimePeriod == DateTime.Now.AddMonths(-3).ToString("MMM"));
            Assert.Equal(0m, threeMonthIncomeData.Amount);

        }

        [Fact]
        public async Task GetDashboardSummary_DaveLoggedIn_SingleCategoryDominance_ReturnsCorrectSummary()
        {
            var dashboardService = CreateDashboardService(_daveId);

            var result = await dashboardService.GetDashboardSummaryAsync(2, 3);

            Assert.Equal(475m, result.Balance);
            Assert.Equal(5m, result.ExpensesThisMonth);
            Assert.Equal(0m, result.IncomeThisMonth);

            Assert.Single(result.TopExpenseCategories);
            var groceryCategory = result.TopExpenseCategories.First();
            Assert.Equal("Groceries", groceryCategory.Category);
            Assert.Equal(25m, groceryCategory.Amount);
        }

        [Fact]
        public async Task GetDashboardSummary_IncomeOnlyUser_ReturnsZeroExpenses()
        {
            var dashboardService = CreateDashboardService(_eveId);

            var result = await dashboardService.GetDashboardSummaryAsync(3, 5);

            Assert.Equal(3030m, result.Balance);
            Assert.Equal(0m, result.ExpensesThisMonth);
            Assert.Equal(15m, result.IncomeThisMonth);
            Assert.Empty(result.TopExpenseCategories);
        }

        [Fact]
        public async Task GetDashboardSummary_MonthBoundaryTransactions_CountsCorrectly()
        {
            var dashboardService = CreateDashboardService(_charlieId);

            var result = await dashboardService.GetDashboardSummaryAsync(1, 2);

            // Verify transactions on month boundaries are categorized correctly
            Assert.Equal(1m, result.ExpensesThisMonth);
            Assert.Equal(2m, result.IncomeThisMonth);
        }

        [Fact]
        public async Task GetDashboardSummary_MaxTopCountExceedsAvailable_ReturnsOnlyAvailable()
        {
            var dashboardService = CreateDashboardService(_charlieId);

            var result = await dashboardService.GetDashboardSummaryAsync(1, 10);

            Assert.Equal(3, result.TopExpenseCategories.Count);
        }

        [Fact]
        public async Task GetDashboardSummary_ZeroMonthsBack_ReturnsOnlyCurrentMonth()
        {
            var dashboardService = CreateDashboardService(_charlieId);

            var result = await dashboardService.GetDashboardSummaryAsync(0, 2);

            Assert.Single(result.CumulativeExpensesPerMonth);
            Assert.Single(result.CumulativeIncomePerMonth);
        }

    }

}