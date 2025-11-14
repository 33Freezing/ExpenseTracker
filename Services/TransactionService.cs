using Database;
using ExpenseTracker.Components.Pages;
using ExpenseTracker.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Services
{
    public class TransactionService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly CurrentUserService _currentUserService;
        
        public TransactionService(IDbContextFactory<AppDbContext> contextFactory, CurrentUserService currentUserService)
        {
            _contextFactory = contextFactory;
            _currentUserService = currentUserService;
        }

        public IQueryable<Transaction> GetTransactionsQuery(AppDbContext context)
        {
            return context.Transactions.Include(t => t.Account).Where(t => t.Account.IdentityUserId == _currentUserService.GetUserId());
        }
        public async Task<List<Transaction>> GetAllAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await GetTransactionsQuery(context).Include(t => t.Category).OrderByDescending(t => t.Date).ToListAsync();
        }

        public async Task SaveAsync(Transaction transaction)
        {
            if (transaction.Category.Type == TransactionType.Expense)
            {
                transaction.Amount = -transaction.Amount;
            }

            
            // otherwise it will try to save the category as well
            transaction.Category = null;
            transaction.Account = null;


            using var context = await _contextFactory.CreateDbContextAsync();
            if (transaction.Id == 0)
            {
                await context.Transactions.AddAsync(transaction);
            }
            else
            {
                var oldTransaction = await context.Transactions.SingleAsync(t => t.Id == transaction.Id);
                if (oldTransaction != null)
                {
                    context.Entry(oldTransaction).CurrentValues.SetValues(transaction);
                }
            }
            await context.SaveChangesAsync();
        }
        
        public async Task DeleteAsync(int transactionId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var transaction = await GetTransactionsQuery(context).SingleAsync(t => t.Id == transactionId);
            context.Transactions.Remove(transaction);
            await context.SaveChangesAsync();
        }
        
        public async Task<Transaction?> GetAsync(int transactionId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await GetTransactionsQuery(context).Include(t => t.Category).SingleAsync(t => t.Id == transactionId);
        }
    }
}