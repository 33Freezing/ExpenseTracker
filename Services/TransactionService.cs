using ExpenseTracker.Components.Pages;
using ExpenseTracker.Database;
using ExpenseTracker.Database.Models;
using ExpenseTracker.Dtos;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Services
{
    public class TransactionService
    {
        private readonly AppDbContext _context;
        private readonly CurrentUserService _currentUserService;
        
        public TransactionService(AppDbContext context, CurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public IQueryable<Transaction> GetTransactionsQuery(AppDbContext context)
        {
            return context.Transactions
            .Include(t => t.Account)
            .Include(t => t.Category)
            .Where(t => t.Account.IdentityUserId == _currentUserService.GetUserId());
        }
        public async Task<List<Transaction>> GetAllAsync()
        {
            return await GetTransactionsQuery(_context).Include(t => t.Category).OrderByDescending(t => t.Date).ToListAsync();
        }

        public async Task SaveAsync(TransactionDto transactionDto)
        {
            if(transactionDto.Amount == null
            || transactionDto.Date == null
            || transactionDto.Time == null
            || transactionDto.AccountId == null
            || transactionDto.CategoryId == null
            || transactionDto.TransactionType == null)
            {
                return;
            }

            var transaction = new Transaction()
            {
                Id = transactionDto.Id ?? 0,
                Amount = (decimal)transactionDto.Amount,
                Description = transactionDto.Description,
                Date = (DateTime)(transactionDto.Date + transactionDto.Time),
                AccountId = (int)transactionDto.AccountId,
                CategoryId = (int)transactionDto.CategoryId
            };

            await SaveInternal(transaction, (TransactionType)transactionDto.TransactionType);
        }

        public async Task SaveAsync(Transaction transaction)
        {
            await SaveInternal(transaction, transaction.Category.Type);
        }
        
        private async Task SaveInternal(Transaction transaction, TransactionType type)
        {
            transaction.Amount = Math.Abs(transaction.Amount);
            if (type == TransactionType.Expense)
            {
                transaction.Amount = -transaction.Amount;
            }

            transaction.Category = null;
            transaction.Account = null;

            if (transaction.Id == 0)
            {
                await _context.Transactions.AddAsync(transaction);
            }
            else
            {
                var oldTransaction = await _context.Transactions.SingleAsync(t => t.Id == transaction.Id);
                if (oldTransaction != null)
                {
                    _context.Entry(oldTransaction).CurrentValues.SetValues(transaction);
                }
            }
            await _context.SaveChangesAsync();
        }
        public async Task DeleteAsync(int transactionId)
        {
            var transaction = await GetTransactionsQuery(_context).SingleAsync(t => t.Id == transactionId);
            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAllAsync()
        {
            await GetTransactionsQuery(_context).ExecuteDeleteAsync();
        }
        
        public async Task<Transaction?> GetAsync(int transactionId)
        {
            return await GetTransactionsQuery(_context).Include(t => t.Category).SingleAsync(t => t.Id == transactionId);
        }
    }
}