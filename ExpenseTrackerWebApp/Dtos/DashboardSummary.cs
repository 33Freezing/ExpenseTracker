using ExpenseTrackerWebApp.Database.Models;

namespace ExpenseTrackerWebApp.Dtos
{
    public class DashboardSummary
    {
        public string Username{get; set;}
        public decimal Balance{get; set;}
        public decimal ExpensesThisMonth{get; set;}
        public decimal IncomeThisMonth{get; set;}

        public List<CategoryData> TopExpenseCategories { get; set; }

        public List<CumulativeData> CumulativeExpensesPerMonth {get; set;}
        public List<CumulativeData> CumulativeIncomePerMonth {get; set;}

        public List<Transaction> Transactions{get; set;}

        public List<Transaction> RecentTransactions
        {
            get
            {
                return Transactions
                    .OrderByDescending(t => t.Date)
                    .Take(5)
                    .ToList();
            }
        }

        public DashboardSummary()
        {
            Balance = 0;
            ExpensesThisMonth = 0;
            IncomeThisMonth = 0;
            TopExpenseCategories = [];
            CumulativeExpensesPerMonth = [];
            CumulativeIncomePerMonth = [];
            Transactions = [];
        }
    }
    
    public class CategoryData
    {
        public string Category { get; set; }
        public decimal Amount { get; set; }
    }

    public class CumulativeData
    {
        public string TimePeriod {get; set;}
        public decimal Amount {get; set;}
    }
}