using System.Globalization;
using ExpenseTrackerWebApp.Database.Models;
using MudBlazor.Extensions;

namespace ExpenseTrackerWebApp.Dtos
{
    public class TransactionsDto
    {
        public List<Transaction> Transactions { get; set; }
        public List<Transaction> FilteredTransactions{get; set;}
        public List<Account> Accounts { get; set; }
        public List<Category> Categories { get; set; }
        public List<Category> FilteredCategories{get; set;}
        public List<Tag> Tags { get; set; }
        private TransactionType? typeFilter;
        public TransactionType? TypeFilter
        {
            get
            {
                return typeFilter;
            }
            set
            {
                if(value != null)
                {
                    FilteredCategories = Categories.Where(c => c.Type == value).ToList();
                    CategoriesFilter = CategoriesFilter.Where(c => c.Type == value).ToList();
                }
                typeFilter = value;
                FilterChanged();
            }
        }
        private DateFilterPreset? dateFilter;
        public DateFilterPreset? DateFilter
        {
            get
            {
                return dateFilter;
            }
            set
            {
                dateFilter = value;
                FilterChanged();
            }
        }
        private IEnumerable<Account> accountsFilter;
        public IEnumerable<Account> AccountsFilter
        {
            get
            {
                return accountsFilter;
            }
            set
            {
                accountsFilter = value;
                FilterChanged();
            }
        }
        private IEnumerable<Category> categoriesFilter;
        public IEnumerable<Category> CategoriesFilter
        {
            get
            {
                return categoriesFilter;
            }
            set
            {
                categoriesFilter = value;
                FilterChanged();
            }
        }

        private IEnumerable<Tag> tagsFilter;
        public IEnumerable<Tag> TagsFilter
        {
            get
            {
                return tagsFilter;
            }
            set
            {
                tagsFilter = value;
                FilterChanged();
            }
        }

        public TransactionsDto()
        {
            Transactions = [];
            FilteredTransactions = [];
            Accounts = [];
            Categories = [];
            FilteredCategories = [];
            Tags = [];
            TypeFilter = null;
            DateFilter = null;
            AccountsFilter = [];
            CategoriesFilter = [];
            TagsFilter = [];
        }

        private void FilterChanged()
        {
            FilteredTransactions = [.. Transactions];
            if (TypeFilter is not null)
            {
                FilteredTransactions = FilteredTransactions.Where(t => t.Category.Type == TypeFilter).ToList();
            }

            if (AccountsFilter is not null && AccountsFilter.Count() != 0)
            {
                var accountIds = AccountsFilter.Select(a => a.Id).ToList();
                FilteredTransactions = FilteredTransactions.Where(t => accountIds.Contains(t.AccountId)).ToList();
            }

            if (CategoriesFilter is not null && CategoriesFilter.Count() != 0)
            {
                var categoryIds = CategoriesFilter.Select(a => a.Id).ToList();
                FilteredTransactions = FilteredTransactions.Where(t => categoryIds.Contains(t.CategoryId)).ToList();
            }

            if (TagsFilter is not null && TagsFilter.Count() != 0)
            {
                var tagIds = TagsFilter.Select(t => t.Id).ToList();
                FilteredTransactions = FilteredTransactions.Where(t => t.TransactionTags.Any(tt => tagIds.Contains(tt.TagId))).ToList();
            }

            if (DateFilter is not null)
            {
                switch (DateFilter)
                {
                    case DateFilterPreset.ThisMonth:
                        var startOfMonth = DateTime.Now.StartOfMonth(CultureInfo.CurrentCulture);
                        FilteredTransactions = FilteredTransactions.Where(t => t.Date >= startOfMonth).ToList();
                        break;

                    case DateFilterPreset.LastMonth:
                        DateTime oneMonthAgo = DateTime.Now.AddMonths(-1);
                        DateTime startOfLastMonth = oneMonthAgo.StartOfMonth(CultureInfo.CurrentCulture);
                        DateTime endOfLastMonth = oneMonthAgo.EndOfMonth(CultureInfo.CurrentCulture);
                        FilteredTransactions = FilteredTransactions.Where(t => t.Date >= startOfLastMonth && t.Date < endOfLastMonth).ToList();
                        break;

                    case DateFilterPreset.Last3Months:
                        DateTime threeMonthsAgo = DateTime.Now.AddMonths(-3);
                        DateTime startOf3MonthsAgoMonth = threeMonthsAgo.StartOfMonth(CultureInfo.CurrentCulture);
                        FilteredTransactions = FilteredTransactions.Where(t => t.Date >= startOf3MonthsAgoMonth).ToList();
                        break;


                    case DateFilterPreset.ThisYear:
                        var startOfyear = new DateTime(DateTime.Now.Year, 1, 1);
                        FilteredTransactions = FilteredTransactions.Where(t => t.Date >= startOfyear).ToList();
                        break;

                }
            }
        }

        public bool AreAnyFiltersActive
        {
            get
            {
                return TypeFilter != null
                    || (CategoriesFilter != null && CategoriesFilter.Count() != 0)
                    || DateFilter != null
                    || (AccountsFilter != null && AccountsFilter.Count() != 0)
                    || (TagsFilter != null && TagsFilter.Count() != 0);
            }
        }

        public void ClearFilters()
        {
            TypeFilter = null;
            CategoriesFilter = new HashSet<Category>() { };
            AccountsFilter = new HashSet<Account>() { };
            DateFilter = null;
            TagsFilter = new HashSet<Tag>() { };
        }
    
        public void RemoveTransactions(int id)
        {
            var transactionsToRemove = Transactions.SingleOrDefault(t => t.Id == id);
            if(transactionsToRemove != null)
            {
                Transactions.Remove(transactionsToRemove);
            }
            
            transactionsToRemove = FilteredTransactions.SingleOrDefault(t => t.Id == id);
            if(transactionsToRemove != null)
            {
                FilteredTransactions.Remove(transactionsToRemove);
            }
        }


    }


    public enum DateFilterPreset
    {
        ThisMonth,
        LastMonth,
        Last3Months,
        ThisYear
    }
}