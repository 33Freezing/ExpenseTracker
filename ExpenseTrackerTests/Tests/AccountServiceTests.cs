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
using Xunit.Sdk;

namespace ExpenseTrackerTests.Tests;

public class AccountServiceTests : IDisposable
{
    private readonly DbConnection _connection;
    private readonly DbContextOptions<AppDbContext> _contextOptions;

    private readonly string _aliceId;   // has no accounts 
    private readonly string _bobId;     // has 1 account 
    private readonly string _charlieId; // has 2 accounts

    private readonly int _bobCashAccountId;
    private readonly int _charlieCashAccountId;
    private readonly int _charlieBankAccountId;
        
    
    public AccountServiceTests()
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
                UserName = "bob@test.com",
                NormalizedUserName = "BOB@TEST.COM",
                Email = "bob@test.com",
                NormalizedEmail = "BOB@TEST.COM",
                EmailConfirmed = true
            };
            
            var user2 = new IdentityUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = "alice@test.com",
                NormalizedUserName = "ALICE@TEST.COM",
                Email = "alice@test.com",
                NormalizedEmail = "ALICE@TEST.COM",
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
            var salary = new Category{Name = "Salary", Type=TransactionType.Income, IdentityUserId = _charlieId};

            context.Categories.AddRange(shopping, salary);
            context.SaveChanges();
            
            var charlieTransctionOne = new Transaction(){Amount=-13.3m, Date=DateTime.Now, AccountId=charlieBankAccount.Id, CategoryId = shopping.Id};
            var charlieTransactionTwo = new Transaction(){Amount=-44.3m, Date=DateTime.Now, AccountId=charlieBankAccount.Id, CategoryId = shopping.Id};
            var charlieTransactionThree = new Transaction(){Amount=-65.6m, Date=DateTime.Now, AccountId=charlieBankAccount.Id, CategoryId = shopping.Id};
            var charlieTransactionFour = new Transaction(){Amount=-144.8m, Date=DateTime.Now, AccountId=charlieBankAccount.Id, CategoryId = shopping.Id};
            
            var charlieTransactionFive = new Transaction(){Amount=100.0m, Date=DateTime.Now, AccountId=charlieBankAccount.Id, CategoryId = salary.Id};
            var charlieTransactionSix = new Transaction(){Amount=150.8m, Date=DateTime.Now, AccountId=charlieBankAccount.Id, CategoryId = salary.Id};

            
            context.Transactions.AddRange(charlieTransctionOne, charlieTransactionTwo, charlieTransactionThree, charlieTransactionFour,
            charlieTransactionFive, charlieTransactionSix);
            context.SaveChanges();

            _bobCashAccountId = bobCashAccount.Id;
            _charlieBankAccountId = charlieBankAccount.Id;
            _charlieCashAccountId = charlieCashAccount.Id;
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

    [Fact]
    public async Task GetAsync_AliceLoggedIn_ReturnsNull()
    {
        var accountService = CreateAccountService(_aliceId);
        var account = await accountService.GetAsync(_bobCashAccountId);
        Assert.Null(account);
    }

    [Fact]
    public async Task GetAsync_BobLoggedIn_ReturnsValidAccount()
    {
        var accountService = CreateAccountService(_bobId);
        var account = await accountService.GetAsync(_bobCashAccountId);

        Assert.Equal(_bobCashAccountId, account.Id);
        Assert.Equal("Bob's Cash", account.Name);
        Assert.Equal(1000.0m, account.InitialBalance);
    }


    [Fact]
    public async Task GetAsync_BobLoggedIn_InvalidId_ReturnsNull()
    {
        var accountService = CreateAccountService(_bobId);
        var account = await accountService.GetAsync(_charlieBankAccountId);
        Assert.Null(account);
    }

    [Fact]
    public async Task GetAllAsync_AliceLoggedIn_RetunsEmptyList()
    {
        var accountService = CreateAccountService(_aliceId);
        var accounts = await accountService.GetAllAsync();
        Assert.Empty(accounts);
    }


    [Fact]
    public async Task GetAllAsync_BobLoggedIn_ReturnsOneAccount()
    {
        var accountService = CreateAccountService(_bobId);
        var accounts = await accountService.GetAllAsync();

        Assert.Single(accounts);
        var account = accounts[0];
        Assert.Equal(_bobCashAccountId, account.Id);
        Assert.Equal("Bob's Cash", account.Name);
        Assert.Equal(1000.0m, account.InitialBalance);
    }


    [Fact]
    public async Task GetAllAsync_CharlieLoggedIn_ReturnsTwoAccounts()
    {
        var accountService = CreateAccountService(_charlieId);
        var accounts = await accountService.GetAllAsync();

        Assert.Equal(2, accounts.Count);

        var bankAccount = accounts.First(a => a.Name == "Charlie's Bank");
        Assert.Equal(_charlieBankAccountId, bankAccount.Id);
        Assert.Equal(1500.0m, bankAccount.InitialBalance);

        var cashAccount = accounts.First(a => a.Name == "Charlie's Cash");
        Assert.Equal(_charlieCashAccountId, cashAccount.Id);
        Assert.Equal(300.0m, cashAccount.InitialBalance);
    }


    [Fact]
    public async Task GetAllAsync_InvalidUser_ReturnsEmptyList()
    {
        var accountService = CreateAccountService("test id");

        var accounts = await accountService.GetAllAsync();
        Assert.Empty(accounts);
    }[Fact]
    public async Task GetAllWithBalanceAsync_AliceLoggedIn_ReturnsCorrectValues()
    {
        var accountService = CreateAccountService(_aliceId);
        var accounts = await accountService.GetAllWithBalanceAsync();

        Assert.Empty(accounts);
    }


    [Fact]
    public async Task GetAllWithBalanceAsync_BobLoggedIn_ReturnsCorrectValues()
    {
        var accountService = CreateAccountService(_bobId);
        var accounts = await accountService.GetAllWithBalanceAsync();

        Assert.Single(accounts);
        var account = accounts[0];
        Assert.Equal(_bobCashAccountId, account.Id);
        Assert.Equal("Bob's Cash", account.Name);
        Assert.Equal(1000.0m, account.InitialBalance);
        Assert.Equal(1000.0m, account.CurrentBalance);
    }


    [Fact]
    public async Task GetAllWithBalanceAsync_CharlieLoggedIn_ReturnsCorrectValues()
    {
        var accountService = CreateAccountService(_charlieId);
        var accounts = await accountService.GetAllWithBalanceAsync();

        Assert.Equal(2, accounts.Count);

        var bankAccount = accounts.First(a => a.Name == "Charlie's Bank");
        Assert.Equal(_charlieBankAccountId, bankAccount.Id);
        Assert.Equal(1500.0m, bankAccount.InitialBalance);
        Assert.Equal(1482.8m, bankAccount.CurrentBalance);

        var cashAccount = accounts.First(a => a.Name == "Charlie's Cash");
        Assert.Equal(_charlieCashAccountId, cashAccount.Id);
        Assert.Equal(300.0m, cashAccount.InitialBalance);
    }


    [Fact]
    public async Task SaveAsync_ValidAccounts_SavesSuccesfully()
    {
        var accountDto = new AccountDto();
        accountDto.UserId = _aliceId;
        accountDto.InitialBalance = 500.0m;
        accountDto.Name = "Alice's Cash";

        var accountService = CreateAccountService(_aliceId);
        await accountService.SaveAsync(accountDto);

        var accounts = await accountService.GetAllAsync();

        Assert.Single(accounts);
        var account = accounts[0];
        Assert.Equal("Alice's Cash", account.Name);
        Assert.Equal(500.0m, account.InitialBalance);
    }

    [Fact]
    public async Task SaveAsync_AccountMissingName_DoesNotInsert()
    {
        var accountDto = new AccountDto();
        accountDto.UserId = _aliceId;
        accountDto.InitialBalance = 500.0m;
        accountDto.Name = "";

        var accountService = CreateAccountService(_aliceId);
        await accountService.SaveAsync(accountDto);

        var accounts = await accountService.GetAllAsync();
        Assert.Empty(accounts);
    }

    [Fact]
    public async Task SaveAsync_AccountUserId_DoesNotInsert()
    {
        var accountDto = new AccountDto();
        accountDto.UserId = "";
        accountDto.InitialBalance = 500.0m;
        accountDto.Name = "Alice's Cash";

        var accountService = CreateAccountService(_aliceId);
        await accountService.SaveAsync(accountDto);

        var accounts = await accountService.GetAllAsync();
        Assert.Empty(accounts);
    }

    [Fact]
    public async Task SaveAsync_BalanceNull_DoesNotInsert()
    {
        var accountDto = new AccountDto();
        accountDto.UserId = _aliceId;
        accountDto.InitialBalance = null;
        accountDto.Name = "Alice's Cash";

        var accountService = CreateAccountService(_aliceId);
        await accountService.SaveAsync(accountDto);

        var accounts = await accountService.GetAllAsync();

        Assert.Empty(accounts);
    }


    [Fact]
    public async Task SaveAsync_UpdatingBobsInitialBalance_UpdatesSuccesfully()
    {
        var accountDto = new AccountDto();
        accountDto.Id = _bobCashAccountId;
        accountDto.UserId = _bobId;
        accountDto.InitialBalance = 0;
        accountDto.Name = "Bob's Cash";

        var accountService = CreateAccountService(_bobId);
        await accountService.SaveAsync(accountDto);

        var accounts = await accountService.GetAllAsync();

        Assert.Single(accounts);
        var account = accounts[0];
        Assert.Equal("Bob's Cash", account.Name);
        Assert.Equal(0.0m, account.InitialBalance);
    }


    [Fact]
    public async Task DeleteAsync_AliceLoggedIn_DoesNotDeleteBobsAccount()
    {
        var aliceAccountService = CreateAccountService(_aliceId);
        var bobAccountService = CreateAccountService(_bobId);

        await aliceAccountService.DeleteAsync(_bobCashAccountId);
        var aliceAccounts = await aliceAccountService.GetAllAsync();
        var bobAccounts = await bobAccountService.GetAllAsync();

        Assert.Empty(aliceAccounts);
        Assert.Single(bobAccounts);
    }


    [Fact]
    public async Task DeleteAsync_BobLoggedIn_DeletesBobAccount()
    {
        var bobAccountService = CreateAccountService(_bobId);

        await bobAccountService.DeleteAsync(_bobCashAccountId);
        var bobAccounts = await bobAccountService.GetAllAsync();

        Assert.Empty(bobAccounts);
    }


    [Fact]
    public async Task DeleteAllAsync_BobLoggedIn_DeletesBobsAccounts_OtherAccountsIntact()
    {
        var aliceAccountService = CreateAccountService(_aliceId);
        var bobAccountService = CreateAccountService(_bobId);
        var charlieAccountService = CreateAccountService(_charlieId);

        await bobAccountService.DeleteAllAsync();

        var aliceAccounts = await aliceAccountService.GetAllAsync();
        var bobAccounts = await bobAccountService.GetAllAsync();
        var charlieAccounts = await charlieAccountService.GetAllAsync();

        Assert.Empty(aliceAccounts);
        Assert.Empty(bobAccounts);
        Assert.Equal(2, charlieAccounts.Count);
    }

    [Fact]
    public async Task AssignUserDefaultAccounts_AliceLoggedIn_AddsAccounts()
    {
        var aliceAccountService = CreateAccountService(_aliceId);

        await aliceAccountService.AssignUserDefaultAccounts(_aliceId);

        var aliceAccounts = await aliceAccountService.GetAllAsync();
        
        Assert.Equal(2, aliceAccounts.Count);
        var account = aliceAccounts.First(a => a.Name == "Cash");
        Assert.Equal("Cash", account.Name);
        Assert.Equal(0, account.InitialBalance);
        
        account = aliceAccounts.First(a => a.Name == "Bank");
        Assert.Equal("Bank", account.Name);
        Assert.Equal(0, account.InitialBalance);
    }

    [Fact]
    public async Task AssignUserDefaultAccounts_CharlieLoggedIn_AddsAccounts()
    {
        var charlieAccountService = CreateAccountService(_charlieId);

        await charlieAccountService.AssignUserDefaultAccounts(_charlieId);

        var charlieAccounts = await charlieAccountService.GetAllAsync();
        
        Assert.Equal(2, charlieAccounts.Count);
        var account = charlieAccounts.First(a => a.Name == "Cash");
        Assert.Equal("Cash", account.Name);
        Assert.Equal(0, account.InitialBalance);
        
        account = charlieAccounts.First(a => a.Name == "Bank");
        Assert.Equal("Bank", account.Name);
        Assert.Equal(0, account.InitialBalance);
    }

}