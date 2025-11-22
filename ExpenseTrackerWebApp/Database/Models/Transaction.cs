
namespace ExpenseTrackerWebApp.Database.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public DateTime Date { get; set; }
        public int AccountId { get; set; }
        public Account Account { get; set; }
        public int CategoryId{ get; set; }
        public Category Category{ get; set; }
        public ICollection<TransactionTag> TransactionTags { get; set; }
        public bool? IsReoccuring{get; set;}
        public ReoccuranceFrequency? ReoccuranceFrequency{get; set;}
        public DateTime? NextReoccuranceDate{get; set;}

        public Transaction()
        {
            TransactionTags = new List<TransactionTag>();
        }
    }


    public enum ReoccuranceFrequency
    {
        Daily,
        Weekly,
        Monthly,
        Yearly
    }
}