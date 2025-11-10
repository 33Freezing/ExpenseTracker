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
                var old = await GetAsync(transaction.Id);
                old.Amount = transaction.Amount;
                old.Date = transaction.Date;
                old.Description = transaction.Description;
            }
            await context.SaveChangesAsync();
        }
        
        public async Task<Transaction?> GetAsync(int transactionId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var transactions = await context.Transactions.Where(t => t.Id == transactionId).ToListAsync();
            return transactions.FirstOrDefault();
        }
    }
}