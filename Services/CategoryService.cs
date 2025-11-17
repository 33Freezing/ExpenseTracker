using ExpenseTracker.Database;
using ExpenseTracker.Database.Models;
using ExpenseTracker.Dtos;
using ExpenseTracker.Migrations;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Services
{
    public class CategoryService
    {
        private readonly AppDbContext _context;
        private readonly CurrentUserService _currentUserService;
        
        public CategoryService(AppDbContext context, CurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public IQueryable<Category> GetCategoriesQuery(AppDbContext context)
        {
            return context.Categories.Where(t => t.IdentityUserId == _currentUserService.GetUserId());
        }
        public async Task<List<Category>> GetAllAsync()
        {
            return await GetCategoriesQuery(_context).OrderBy(t => t.Id).ToListAsync();
        }
        public async Task<List<Category>> GetAllWithTransactionsAsync()
        {
            return await GetCategoriesQuery(_context).Include(c => c.Transactions).OrderBy(t => t.Id).ToListAsync();
        }

        public async Task<Category?> GetAsync(int categoryId)
        {
            return await GetCategoriesQuery(_context).SingleAsync(c => c.Id == categoryId);
        }

        public async Task DeleteAsync(int categoryId)
        {
            await _context.Transactions.Where(t => t.CategoryId == categoryId).ExecuteDeleteAsync();
            await GetCategoriesQuery(_context).Where(c => c.Id == categoryId).ExecuteDeleteAsync();
        }

        public async Task DeleteAllAsync()
        {
            var categoryIds = await GetCategoriesQuery(_context).Select(c => c.Id).ToListAsync();
            foreach(var categoryId in categoryIds)
            {
                await _context.Transactions.Where(t => t.CategoryId == categoryId).ExecuteDeleteAsync();
            }
            await GetCategoriesQuery(_context).ExecuteDeleteAsync();
        }

        
        public async Task SaveAsync(CategoryDto categoryDto)
        {
            if(string.IsNullOrWhiteSpace(categoryDto.Name) ||
                string.IsNullOrWhiteSpace(categoryDto.UserId) ||
                categoryDto.Type == null)
                {
                    return;
                }
            var category = new Category
            {
                Id = categoryDto.Id ?? 0,
                Name = categoryDto.Name,
                Type = (TransactionType)categoryDto.Type,
                IdentityUserId = categoryDto.UserId
            };

            await InternalSave(category);
        }
        public async Task SaveAsync(Category category)
        {
            await InternalSave(category);
        }

        private async Task InternalSave(Category category)
        {
            if (category.Id == 0)
            {
                await _context.Categories.AddAsync(category);
            }
            else
            {
                var oldCategory = await _context.Categories.SingleAsync(t => t.Id == category.Id);
                if (oldCategory != null)
                {
                    _context.Entry(oldCategory).CurrentValues.SetValues(category);
                }
            }
            await _context.SaveChangesAsync();
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