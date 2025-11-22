using ExpenseTrackerWebApp.Database.Models;

namespace ExpenseTrackerWebApp.Dtos
{
    public class TransactionDto
    {
        public int? Id { get; set; }
        public decimal? Amount { get; set; }
        public string? Description { get; set; }
        public DateTime? Date{get; set;}
        public TimeSpan? Time{get; set;}
        public int? AccountId { get; set; }
        public int? CategoryId {get; set;}
        public TransactionType? TransactionType{get; set;}
        public List<int> TagIds { get; set; } = new List<int>();
    }
}