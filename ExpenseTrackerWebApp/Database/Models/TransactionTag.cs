namespace ExpenseTrackerWebApp.Database.Models
{
    public class TransactionTag
    {
        public int TransactionId { get; set; }
        public Transaction Transaction { get; set; }

        public int TagId { get; set; }
        public Tag Tag { get; set; }
    }
}
