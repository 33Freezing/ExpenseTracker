using Database;
using ExpenseTracker.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Services
{
    public class TransactionService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        
        public TransactionService(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<Transaction>> GetAllAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Transactions.Include(t => t.Category).OrderByDescending(t => t.Date).ToListAsync();
        }

        public async Task Save(Transaction transaction)
        {
            // otherwise it will try to save the category as well
            transaction.Category = null;

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
            var transaction = await context.Transactions.SingleAsync(t => t.Id == transactionId);
            context.Transactions.Remove(transaction);
            await context.SaveChangesAsync();
        }
        
        public async Task<Transaction?> GetAsync(int transactionId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Transactions.Include(t => t.Category).SingleAsync(t => t.Id == transactionId);
        }
    }
}