using ExpenseTrackerWebApp.Components.Pages;
using ExpenseTrackerWebApp.Database;
using ExpenseTrackerWebApp.Database.Models;
using ExpenseTrackerWebApp.Dtos;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTrackerWebApp.Services
{
    public class TransactionService
    {
        private readonly AppDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly AccountService _accountService;
        private readonly CategoryService _categoryService;
        private readonly TagService _tagService;
        
        public TransactionService(AppDbContext context, ICurrentUserService currentUserService,
        AccountService accountService, CategoryService categoryService, TagService tagService)
        {
            _context = context;
            _currentUserService = currentUserService;
            _accountService = accountService;
            _categoryService = categoryService;
            _tagService = tagService;
        }
        // has 2 accounts
        // has two transactions this month, one income one expense
        // has five transctions last month, all expenses
        // has seven transctions two months ago, six expenses one income

        public IQueryable<Transaction> GetTransactionsQuery(AppDbContext context)
        {
            return context.Transactions
            .Include(t => t.Account)
            .Include(t => t.Category)
            .Include(t => t.TransactionTags)
                .ThenInclude(tt => tt.Tag)
            .Where(t => t.Account.IdentityUserId == _currentUserService.GetUserId());
        }
        public async Task<List<Transaction>> GetAllAsync(bool reoccuring = false)
        {
            var result = GetTransactionsQuery(_context);
            if(reoccuring){
                result = result.Where(t => t.IsReoccuring == true);
            }

            return await result.OrderByDescending(t => t.Date).ToListAsync();
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
                CategoryId = (int)transactionDto.CategoryId,
                IsReoccuring = transactionDto.Reoccuring,
                ReoccuranceFrequency = transactionDto.ReoccuranceFrequency
            };

            if(transaction.IsReoccuring!= null && transaction.IsReoccuring == true && transaction.ReoccuranceFrequency != null){
                switch(transaction.ReoccuranceFrequency){
                    case ReoccuranceFrequency.Daily:
                        transaction.NextReoccuranceDate = transaction.Date.AddDays(1);
                        break;
                    case ReoccuranceFrequency.Weekly:
                        transaction.NextReoccuranceDate = transaction.Date.AddDays(7);
                        break;
                    case ReoccuranceFrequency.Monthly:
                        transaction.NextReoccuranceDate = transaction.Date.AddMonths(1);
                        break;
                    case ReoccuranceFrequency.Yearly:
                        transaction.NextReoccuranceDate = transaction.Date.AddYears(1);
                        break;
                }

            }

            await SaveInternal(transaction, (TransactionType)transactionDto.TransactionType);
            transactionDto.Id = transaction.Id;

            // Set tags for the transaction
            await _tagService.SetTransactionTagsAsync(transaction.Id, transactionDto.TagIds);
        }

        public async Task SaveAsync(Transaction transaction)
        {
            var tagIds = transaction.TransactionTags.Select(t => t.TagId).ToList();
            await SaveInternal(transaction, transaction.Category.Type);
            await _tagService.SetTransactionTagsAsync(transaction.Id, tagIds);
        }
        
        private async Task SaveInternal(Transaction transaction, TransactionType type)
        {
            transaction.Amount = Math.Abs(transaction.Amount);
            if (type == TransactionType.Expense)
            {
                transaction.Amount = -transaction.Amount;
            }

            transaction.Category = null!;
            transaction.Account = null!;
            transaction.TransactionTags = null!;

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
            return await GetTransactionsQuery(_context).SingleAsync(t => t.Id == transactionId);
        }

        public async Task<TransactionsDto> GetTransactionsDto(bool reoccuring = false)
        {
            var result = new TransactionsDto();
            result.Transactions = await GetAllAsync(reoccuring);
            result.FilteredTransactions = result.Transactions.ToList();
            result.Accounts = await _accountService.GetAllAsync();
            result.Categories = await _categoryService.GetAllAsync();
            result.FilteredCategories = result.Categories.ToList();
            result.Tags = await _tagService.GetAllAsync();
            return result;
        }


        public async Task CheckForReoccuringTransactions(){
            
            var reoccuringTransactions = await GetAllAsync(true);
            if(reoccuringTransactions.Count == 0){
                return;
            }

            foreach(var transaction in reoccuringTransactions){
                if(transaction.NextReoccuranceDate != null && transaction.NextReoccuranceDate <= DateTime.Today){

                    while(transaction.NextReoccuranceDate <= DateTime.Today){
                        var newTransaction = new Transaction(){
                            Amount = transaction.Amount,
                            AccountId = transaction.AccountId,
                            Account = transaction.Account,
                            CategoryId = transaction.CategoryId,
                            Category = transaction.Category,
                            Description = transaction.Description,
                            Date = (DateTime)transaction.NextReoccuranceDate,
                            IsReoccuring = false,
                            ReoccuranceFrequency = null,
                            NextReoccuranceDate = null,
                            TransactionTags = transaction.TransactionTags
                        };

                        // Calculate next reoccurance date
                        switch(transaction.ReoccuranceFrequency){
                            case ReoccuranceFrequency.Daily:
                                transaction.NextReoccuranceDate = transaction.NextReoccuranceDate!.Value.AddDays(1);
                                break;
                            case ReoccuranceFrequency.Weekly:
                                transaction.NextReoccuranceDate = transaction.NextReoccuranceDate!.Value.AddDays(7);
                                break;
                            case ReoccuranceFrequency.Monthly:
                                transaction.NextReoccuranceDate = transaction.NextReoccuranceDate!.Value.AddMonths(1);
                                break;
                            case ReoccuranceFrequency.Yearly:
                                transaction.NextReoccuranceDate = transaction.NextReoccuranceDate!.Value.AddYears(1);
                                break;
                        }

                        await SaveAsync(newTransaction);
                    }

                }
            }

        }
    }
}