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
            return await context.Transactions.OrderByDescending(t => t.Date).ToListAsync();
        }

        public async Task Save(Transaction transaction)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            if (transaction.Id == 0)
            {
                await context.Transactions.AddAsync(transaction);
            }
            else
            {
                var oldTransaction = await context.Transactions.SingleAsync(t => t.Id == transaction.Id);
                oldTransaction.Type = transaction.Type;
                oldTransaction.Amount = transaction.Amount;
                oldTransaction.Date = transaction.Date;
                oldTransaction.Description = transaction.Description;
            }
            await context.SaveChangesAsync();
        }
        
        public async Task<Transaction?> GetAsync(int transactionId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Transactions.SingleAsync(t => t.Id == transactionId);
        }
    }
}