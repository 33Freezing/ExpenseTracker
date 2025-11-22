using ExpenseTrackerWebApp.Database.Models;
using ExpenseTrackerWebApp.Dtos;
using ExpenseTrackerWebApp.Services;

namespace ExpenseTrackerWebApp.Services
{
    public class SeedDataService
    {
        private AccountService _accountService;
        private TransactionService _transactionService;
        private CategoryService _categoryService;
        private TagService _tagService;
        private ICurrentUserService _currentUserService;
        public SeedDataService(AccountService accountService, TransactionService transactionService, CategoryService categoryService, TagService tagService, ICurrentUserService currentUserService)
        {
            _accountService = accountService;
            _transactionService = transactionService;
            _categoryService = categoryService;
            _tagService = tagService;
            _currentUserService = currentUserService;
        }

        public async Task SeedDummyDataAsync()
        {
            var userId = _currentUserService.GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                return;
            }

            await _accountService.AssignUserDefaultAccounts(userId);
            var accounts = await _accountService.GetAllAsync();

            await _categoryService.DeleteAllAsync();
            await _categoryService.AssignUserDefaultCategories(userId);
            var categories = await _categoryService.GetAllAsync();

            // Create seed tags
            var tags = await CreateSeedTagsAsync();

            var random = new Random();
            var transactions = new List<TransactionDto>();
            var today = DateTime.Today;
            
            for (int i = 0; i < 2000; i++)
            {
                var daysAgo = random.Next(0, 1200);
                var date = today.AddDays(-daysAgo);

                var category = categories[random.Next(categories.Count)];
                var account = accounts[random.Next(accounts.Count)];
                var amount = random.Next(10, 500);

                // Assign random tags (0-3 tags per transaction for variety)
                var tagCount = random.Next(0, 4);
                var assignedTags = new List<int>();
                for (int j = 0; j < tagCount; j++)
                {
                    var tag = tags[random.Next(tags.Count)];
                    if (!assignedTags.Contains(tag.Id))
                    {
                        assignedTags.Add(tag.Id);
                    }
                }

                transactions.Add(new TransactionDto
                {
                    Amount = amount,
                    Date = date,
                    Time = new TimeSpan(random.Next(0, 24), random.Next(0, 60), 0),
                    AccountId = account.Id,
                    CategoryId = category.Id,
                    TransactionType = category.Type,
                    Description = GetRandomDescription(random),
                    TagIds = assignedTags
                });
            }

            foreach (var transaction in transactions)
            {
                await _transactionService.SaveAsync(transaction);
            }
        }

        private async Task<List<Tag>> CreateSeedTagsAsync()
        {
            var seedTags = new List<(string name, string color)>
            {
                ("Groceries", "#4CAF50"),       // Green
                ("Dining Out", "#FF9800"),      // Orange
                ("Shopping", "#E91E63"),        // Pink
                ("Utilities", "#2196F3"),       // Blue
                ("Entertainment", "#9C27B0"),   // Purple
                ("Transportation", "#00BCD4"),  // Cyan
                ("Health", "#F44336"),          // Red
                ("Work Related", "#FFC107"),    // Amber
                ("Subscription", "#795548"),    // Brown
                ("Emergency", "#607D8B")        // Blue Grey
            };

            var tags = new List<Tag>();
            foreach (var (name, color) in seedTags)
            {
                var tag = await _tagService.SaveAsync(name, color);
                tags.Add(tag);
            }

            return tags;
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