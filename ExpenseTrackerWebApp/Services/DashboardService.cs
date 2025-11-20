using System.Globalization;
using ExpenseTrackerWebApp.Database.Models;
using ExpenseTrackerWebApp.Dtos;
using ExpenseTrackerWebApp.Services;
using MudBlazor.Extensions;

namespace ExpenseTrackerWebApp.Services
{
    public class DashboardService
    {
        private readonly TransactionService _transactionService;
        private readonly AccountService _accountService;

        public DashboardService(TransactionService transactionService, AccountService accountService)
        {
            _transactionService = transactionService;
            _accountService = accountService;
        }

        public async Task<DashboardSummary> GetDashboardSummaryAsync(int monthsBehindToConsider = 5, int maxCategoriesToShow = 6)
        {
            var result = new DashboardSummary();
            result.Transactions = await _transactionService.GetAllAsync();
            var accounts = await _accountService.GetAllAsync();
            
            result.Balance += accounts.Sum(a => a.InitialBalance);
            result.Balance += result.Transactions.Sum(t => t.Amount);
            
            var transactionsThisMonth = result.Transactions
                .Where(t => t.Date >= DateTime.Now.StartOfMonth(CultureInfo.CurrentCulture))
                .ToList();
                
                foreach (var transaction in transactionsThisMonth)
                {
                    if (transaction.Category.Type == TransactionType.Expense)
                    {
                        result.ExpensesThisMonth += Math.Abs(transaction.Amount);
                    }
                    else if (transaction.Category.Type == TransactionType.Income)
                    {
                        result.IncomeThisMonth += transaction.Amount;
                    }
                }

            result.TopExpenseCategories = GetTopExpenseCategories(result.Transactions, monthsBehindToConsider, maxCategoriesToShow);
            result.CumulativeExpensesPerMonth = GetCumulativePerMonth(result.Transactions, TransactionType.Expense, monthsBehindToConsider);
            result.CumulativeIncomePerMonth = GetCumulativePerMonth(result.Transactions, TransactionType.Income, monthsBehindToConsider);

            return result;
        }


        private List<CategoryData> GetTopExpenseCategories(List<Transaction> transactions, int monthsBehindToConsider = 5, int maxCategoriesToShow = 6)
        {
            var result = new List<CategoryData>();
            var dateStart = DateTime.Now.AddMonths(-monthsBehindToConsider).StartOfMonth(CultureInfo.CurrentCulture);
            var topCategories = transactions
            .Where(t => t.Category.Type == TransactionType.Expense)
            .Where(t => t.Date >= dateStart)
            .GroupBy(t => t.Category)
            .Select(g => new CategoryData()
            {
                Category = g.Key.Name,
                Amount = g.Sum(t => -t.Amount)
            })
            .OrderByDescending(g => g.Amount).ToList();

            decimal total = topCategories.Sum(c => c.Amount);
            decimal topCategoriesAmount = 0;

            for (int i = 0; i < maxCategoriesToShow && i < topCategories.Count; ++i)
            {
                result.Add(topCategories[i]);
                topCategoriesAmount += topCategories[i].Amount;
            }
            if (topCategories.Count > maxCategoriesToShow && (total - topCategoriesAmount) > 0)
            {
                result.Add(new CategoryData() { Category = "Other", Amount = total - topCategoriesAmount });
            }

            return result;
        }
    
        private List<CumulativeData> GetCumulativePerMonth(List<Transaction> transactions, TransactionType type, int monthsBehindToConsider = 5)
        {
            var result = new List<CumulativeData>();

            var dateStart = DateTime.Now.AddMonths(-monthsBehindToConsider).StartOfMonth(CultureInfo.CurrentCulture);
            for(int i = 0; i <= monthsBehindToConsider; ++i)
            {
                var date = dateStart.AddMonths(i);
                result.Add(new CumulativeData()
                {
                    TimePeriod = date.ToString("MMM"),
                    Amount = 0
                });
            }

            var monthlyExpenses = transactions
            .Where(t => t.Category.Type == type)
            .Where(t => t.Date >= dateStart)
            .GroupBy(t => new { t.Date.Year, t.Date.Month })
            .Select(g => new
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                Amount = g.Sum(t => type == TransactionType.Expense ? -t.Amount : t.Amount)
            })
            .OrderBy(g => g.Year).ThenBy(g => g.Month)
            .ToList();

            foreach (var monthData in monthlyExpenses)
            {
                foreach(var data in result)
                {
                    if (data.TimePeriod == new DateTime(monthData.Year, monthData.Month, 1).ToString("MMM"))
                    {
                        data.Amount = monthData.Amount;
                        break;
                    }
                }
            }

            return result;
        }
    
    }
}