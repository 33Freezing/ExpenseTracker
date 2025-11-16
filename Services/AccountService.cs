using Database;
using ExpenseTracker.Database.Models;
using ExpenseTracker.Dtos;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Services
{
    public class AccountService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly CurrentUserService _currentUserService;
        
        public AccountService(IDbContextFactory<AppDbContext> contextFactory, CurrentUserService currentUserService)
        {
            _contextFactory = contextFactory;
            _currentUserService = currentUserService;
        }

        private IQueryable<Account> GetAccountsQuery(AppDbContext context)
        {
            return context.Accounts.Where(a => a.IdentityUserId == _currentUserService.GetUserId());
        }
        public async Task<List<AccountWithBalance>> GetAllWithBalanceAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await GetAccountsQuery(context).Include(a => a.Transactions)
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
            using var context = await _contextFactory.CreateDbContextAsync();
            return await GetAccountsQuery(context).ToListAsync();
        }

        public async Task<Account> GetAsync(int accountId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await GetAccountsQuery(context).SingleAsync(a => a.Id == accountId);
        }
        public async Task<AccountWithBalance> GetWithBalanceAsync(int accountId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await GetAccountsQuery(context)
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

            using var context = await _contextFactory.CreateDbContextAsync();
            var account = new Account
            {
                Id = accountDto.Id ?? 0,
                Name = accountDto.Name,
                InitialBalance = (decimal)accountDto.InitialBalance,
                IdentityUserId = accountDto.UserId
            };

            if (account.Id == 0)
            {
                await context.Accounts.AddAsync(account);
            }
            else
            {
                var oldAccount = await context.Accounts.SingleAsync(a => a.Id == account.Id);
                if (oldAccount != null)
                {
                    context.Entry(oldAccount).CurrentValues.SetValues(account);
                }
            }
            await context.SaveChangesAsync();

        }

        public async Task DeleteAsync(int accountId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            await GetAccountsQuery(context)
                .Where(a => a.Id == accountId)
                .ExecuteDeleteAsync();
        }
        
        public async Task DeleteAllAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            await GetAccountsQuery(context)
                .ExecuteDeleteAsync();
        }
    }
}