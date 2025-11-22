using ExpenseTrackerWebApp.Database;
using ExpenseTrackerWebApp.Database.Models;
using ExpenseTrackerWebApp.Dtos;
using Microsoft.EntityFrameworkCore;
using MudBlazor;

namespace ExpenseTrackerWebApp.Services
{
    public class AccountService
    {
        private readonly AppDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        public AccountService(AppDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        private IQueryable<Account> GetAccountsQuery(AppDbContext context)
        {
            return context.Accounts.Where(a => a.IdentityUserId == _currentUserService.GetUserId());
        }
        public async Task<Account?> GetAsync(int accountId)
        {
            return await GetAccountsQuery(_context).SingleOrDefaultAsync(a => a.Id == accountId);
        }

        public async Task<List<Account>> GetAllAsync()
        {
            return await GetAccountsQuery(_context).ToListAsync();
        }

        public async Task<List<AccountWithBalance>> GetAllWithBalanceAsync()
        {
            return await GetAccountsQuery(_context).Include(a => a.Transactions)
            .Select(a => new AccountWithBalance()
            {
                Id = a.Id,
                Name = a.Name,
                InitialBalance = a.InitialBalance,
                CurrentBalance = a.InitialBalance + a.Transactions.Sum(t => t.Amount),
                Icon = a.Icon,
                Color = a.Color
            }).ToListAsync();
        }

        public async Task SaveAsync(AccountDto accountDto)
        {
            if(string.IsNullOrWhiteSpace(accountDto.Name) ||
                string.IsNullOrWhiteSpace(accountDto.UserId) ||
                accountDto.InitialBalance == null ||
                string.IsNullOrWhiteSpace(accountDto.Icon) ||
                string.IsNullOrWhiteSpace(accountDto.Color))
                {
                    return;
                }

            var account = new Account
            {
                Id = accountDto.Id ?? 0,
                Name = accountDto.Name,
                InitialBalance = (decimal)accountDto.InitialBalance,
                IdentityUserId = accountDto.UserId,
                Icon = accountDto.Icon,
                Color = accountDto.Color
            };
            await SaveInternal(account);
        }

        public async Task SaveAsync(Account account)
        {
            await SaveInternal(account);
        }

        private async Task SaveInternal(Account account)
        {
            if (account.Id == 0)
            {
                await _context.Accounts.AddAsync(account);
            }
            else
            {
                var oldAccount = await _context.Accounts.SingleAsync(a => a.Id == account.Id);
                if (oldAccount != null)
                {
                    _context.Entry(oldAccount).CurrentValues.SetValues(account);
                }
            }
            await _context.SaveChangesAsync();
            
        }

        public async Task DeleteAsync(int accountId)
        {
            await GetAccountsQuery(_context)
                .Where(a => a.Id == accountId)
                .ExecuteDeleteAsync();
        }
        
        public async Task DeleteAllAsync()
        {
            await GetAccountsQuery(_context)
                .ExecuteDeleteAsync();
        }
        public async Task AssignUserDefaultAccounts(string userId)
        {
            await DeleteAllAsync();
            
            var defaultAcounts = new List<Account>()
            {
                new()
                {
                    Name = "Cash",
                    InitialBalance = 0,
                    Icon = Icons.Material.Filled.Payments,
                    Color = "#FF5733"
                },
                new()
                {
                    Name = "Bank",
                    InitialBalance = 0,
                    Icon = Icons.Material.Filled.AccountBalance,
                    Color = "#33C1FF"
                }
            };

            foreach(var account in defaultAcounts)
            {
                account.Id = 0;
                account.IdentityUserId = userId;
                await SaveAsync(account);
            }
        }

        public static List<string> GetDefaultAccountIcons()
        {
            return new List<string>
            {
                Icons.Material.Filled.Payments,
                Icons.Material.Filled.AccountBalance,
                Icons.Material.Filled.Wallet,
                Icons.Material.Filled.CreditCard,
                Icons.Material.Filled.Savings,
                Icons.Material.Filled.AttachMoney,
                Icons.Material.Filled.StoreMallDirectory,
                Icons.Material.Filled.LocalAtm,
                Icons.Material.Filled.MonetizationOn,
                Icons.Material.Filled.PointOfSale,
                Icons.Material.Filled.ShoppingCart,
                Icons.Material.Filled.AccountBalanceWallet
            };
        }
    }
}