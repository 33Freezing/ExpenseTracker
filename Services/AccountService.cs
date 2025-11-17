using ExpenseTracker.Database;
using ExpenseTracker.Database.Models;
using ExpenseTracker.Dtos;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Services
{
    public class AccountService
    {
        private readonly AppDbContext _context;
        private readonly CurrentUserService _currentUserService;
        
        public AccountService(AppDbContext context, CurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        private IQueryable<Account> GetAccountsQuery(AppDbContext context)
        {
            return context.Accounts.Where(a => a.IdentityUserId == _currentUserService.GetUserId());
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
            }).ToListAsync();
        }

        public async Task<List<Account>> GetAllAsync()
        {
            return await GetAccountsQuery(_context).ToListAsync();
        }

        public async Task<Account> GetAsync(int accountId)
        {
            return await GetAccountsQuery(_context).SingleAsync(a => a.Id == accountId);
        }
        public async Task<AccountWithBalance> GetWithBalanceAsync(int accountId)
        {
            return await GetAccountsQuery(_context)
            .Include(a => a.Transactions)
            .Select(a => new AccountWithBalance()
            {
                Id = a.Id,
                Name = a.Name,
                InitialBalance = a.InitialBalance,
                CurrentBalance = a.InitialBalance + a.Transactions.Sum(t => t.Amount),
            }).SingleAsync(a => a.Id == accountId);
        }

        public async Task SaveAsync(AccountDto accountDto)
        {
            if(string.IsNullOrWhiteSpace(accountDto.Name) ||
                string.IsNullOrWhiteSpace(accountDto.UserId) ||
                accountDto.InitialBalance == null)
            {
                return;
            }

            var account = new Account
            {
                Id = accountDto.Id ?? 0,
                Name = accountDto.Name,
                InitialBalance = (decimal)accountDto.InitialBalance,
                IdentityUserId = accountDto.UserId
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
        public async Task AssignUserDefaultCategories(string userId)
        {
            var defaultAcounts = new List<Account>()
            {
                new()
                {
                    Name = "Cash",
                    InitialBalance = 0,
                },
                new()
                {
                    Name = "Bank",
                    InitialBalance = 0
                }
            };

            foreach(var account in defaultAcounts)
            {
                account.Id = 0;
                account.IdentityUserId = userId;
                await SaveAsync(account);
            }
        }
    }
}