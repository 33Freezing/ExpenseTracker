using Database;
using ExpenseTracker.Database.Models;
using ExpenseTracker.Dtos;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Services
{
    public class AccountService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        
        public AccountService(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }
        public async Task<List<AccountWithBalance>> GetAllAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Accounts.Include(a => a.Transactions)
            .Select(a => new AccountWithBalance()
            {
                Id = a.Id,
                Name = a.Name,
                InitialBalance = a.InitialBalace,
                CurrentBalance = a.InitialBalace + a.Transactions.Sum(t => t.Amount),
            }).ToListAsync();
        }

        public async Task SaveAsync(Account account)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
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
            var account = await context.Accounts.SingleAsync(a => a.Id == accountId);
            context.Accounts.Remove(account);
            await context.SaveChangesAsync();
        }
    }
}