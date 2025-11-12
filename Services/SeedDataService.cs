using ExpenseTracker.Database.Models;
using ExpenseTracker.Services;

namespace ExpenseTracker.Services
{
    public class SeedDataService
    {
        private AccountService _accountService;
        private TransactionService _transactionService;
        private CategoryService _categoryService;
        public SeedDataService(AccountService accountService, TransactionService transactionService, CategoryService categoryService)
        {
            _accountService = accountService;
            _transactionService = transactionService;
            _categoryService = categoryService;
        }

        public async Task SeedDummyDataAsync()
        {
            var bankAccount = new Account
            {
                Name = "Bank Account",
                InitialBalance = 2500m
            };
            var cashAccount = new Account
            {
                Name = "Cash",
                InitialBalance = 300m
            };

            await _accountService.DeleteAllAsync();

            await _accountService.SaveAsync(bankAccount);
            await _accountService.SaveAsync(cashAccount);

            var random = new Random();
            var transactions = new List<Transaction>();
            var today = DateTime.Today;
            var categories = await _categoryService.GetAllAsync();

            for (int i = 0; i < 200; i++)
            {
                var daysAgo = random.Next(0, 600);
                var date = today.AddDays(-daysAgo);
                var accountId = random.Next(0, 2) == 0 ? bankAccount.Id : cashAccount.Id;

                var category = categories[random.Next(categories.Count)];
                var amount = random.Next(0, 300);

                transactions.Add(new Transaction
                {
                    Amount = amount,
                    Date = date,
                    AccountId = accountId,
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