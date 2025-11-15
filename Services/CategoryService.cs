using Database;
using ExpenseTracker.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Services
{
    public class CategoryService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly CurrentUserService _currentUserService;
        
        public CategoryService(IDbContextFactory<AppDbContext> contextFactory, CurrentUserService currentUserService)
        {
            _contextFactory = contextFactory;
            _currentUserService = currentUserService;
        }

        public IQueryable<Category> GetCategoriesQuery(AppDbContext context)
        {
            return context.Categories.Where(t => t.IdentityUserId == _currentUserService.GetUserId());
        }
        public async Task<List<Category>> GetAllAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await GetCategoriesQuery(context).OrderBy(t => t.Id).ToListAsync();
        }
        public async Task<List<Category>> GetAllWithTransactionsAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await GetCategoriesQuery(context).Include(c => c.Transactions).OrderBy(t => t.Id).ToListAsync();
        }

        public async Task<Category?> GetAsync(int categoryId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await GetCategoriesQuery(context).SingleAsync(c => c.Id == categoryId);
        }

        public async Task DeleteAsync(int categoryId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            await context.Transactions.Where(t => t.CategoryId == categoryId).ExecuteDeleteAsync();
            await GetCategoriesQuery(context).Where(c => c.Id == categoryId).ExecuteDeleteAsync();
        }

        
        public async Task SaveAsync(Category category)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            if (category.Id == 0)
            {
                await context.Categories.AddAsync(category);
            }
            else
            {
                var oldCategory = await context.Categories.SingleAsync(t => t.Id == category.Id);
                if (oldCategory != null)
                {
                    context.Entry(oldCategory).CurrentValues.SetValues(category);
                }
            }
            await context.SaveChangesAsync();
        }


        public static List<Category> GetDefaultCategories()
        {
            return [
                new() { Id = 1, Name = "Groceries", Type=TransactionType.Expense},
                new() { Id = 2, Name = "Eating Out", Type=TransactionType.Expense},
                new() { Id = 3, Name = "Shopping", Type=TransactionType.Expense},
                new() { Id = 4, Name = "Transportation", Type=TransactionType.Expense},
                new() { Id = 5, Name = "Vehicle", Type=TransactionType.Expense},
                new() { Id = 6, Name = "Communication", Type=TransactionType.Expense},
                new() { Id = 7, Name = "Health and Wellness", Type=TransactionType.Expense},
                new() { Id = 8, Name = "Education", Type=TransactionType.Expense},
                new() { Id = 9, Name = "Entertainment", Type=TransactionType.Expense},
                new() { Id = 10, Name = "Pets", Type=TransactionType.Expense},
                new() { Id = 11, Name = "Salary", Type=TransactionType.Income},
            ];
        }

        public async Task AssignUserDefaultCategories(string userId)
        {
            var defaultCategories = GetDefaultCategories();
            foreach(var category in defaultCategories)
            {
                category.Id = 0;
                category.IdentityUserId = userId;
                await SaveAsync(category);
            }
        }

    }
}