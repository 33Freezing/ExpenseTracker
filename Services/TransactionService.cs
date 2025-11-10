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
            await context.Transactions.AddAsync(transaction);
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