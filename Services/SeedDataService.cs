using ExpenseTracker.Database.Models;
using ExpenseTracker.Dtos;
using ExpenseTracker.Services;

namespace ExpenseTracker.Services
{
    public class SeedDataService
    {
        private AccountService _accountService;
        private TransactionService _transactionService;
        private CategoryService _categoryService;
        private CurrentUserService _currentUserService;
        public SeedDataService(AccountService accountService, TransactionService transactionService, CategoryService categoryService, CurrentUserService currentUserService)
        {
            _accountService = accountService;
            _transactionService = transactionService;
            _categoryService = categoryService;
            _currentUserService = currentUserService;
        }

        public async Task SeedDummyDataAsync()
        {
            var userId = _currentUserService.GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                return;
            }

            await _accountService.DeleteAllAsync();
            var bankAccount = new Account
            {
                Name = "Bank Account",
                InitialBalance = 2500m,
                IdentityUserId = userId
            };
            var cashAccount = new Account
            {
                Name = "Cash",
                InitialBalance = 300m,
                IdentityUserId = userId
            };

            await _accountService.SaveAsync(bankAccount);
            await _accountService.SaveAsync(cashAccount);
            var accounts = await _accountService.GetAllAsync();
            
            await _categoryService.DeleteAllAsync();
            await _categoryService.AssignUserDefaultCategories(userId);
            var categories = await _categoryService.GetAllAsync();

            var random = new Random();
            var transactions = new List<Transaction>();
            var today = DateTime.Today;
            
            for (int i = 0; i < 200; i++)
            {
                var daysAgo = random.Next(0, 600);
                var date = today.AddDays(-daysAgo);

                var category = categories[random.Next(categories.Count)];
                var account = accounts[random.Next(accounts.Count)];
                var amount = random.Next(0, 300);

                transactions.Add(new Transaction
                {
                    Amount = amount,
                    Date = date,
                    AccountId = account.Id,
                    Category = category,
                    CategoryId = category.Id,
                    Description = GetRandomDescription(random)
                });
            }


            foreach (var transaction in transactions)
            {
                await _transactionService.SaveAsync(transaction);
            }
        }

        private static string? GetRandomDescription(Random random)
        {
            var descriptions = new List<string?>()
            {
                "Supermarket", "Whole Foods", "Trader Joe's", "Weekly groceries",
                "Lunch", "Dinner with friends", "Coffee shop", "Fast food",
                "Clothing", "Electronics", "Home goods", "Amazon order",
                "Gas", "Uber", "Public transit", "Parking",
                "Car maintenance", "Oil change", "Car wash", "Insurance",
                "Phone bill", "Internet", "Streaming services",
                "Pharmacy", "Doctor visit", "Gym membership", "Vitamins",
                "Course fees", "Books", "Online class",
                "Movie tickets", "Concert", "Games", "Hobby supplies",
                "Pet food", "Vet visit", "Pet supplies", null 
            };

            return descriptions[random.Next(descriptions.Count)];
        }
    }
}