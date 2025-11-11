namespace ExpenseTracker.Database.Models
{
    public class Account
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal InitialBalace { get; set; }

        public ICollection<Transaction> Transactions { get; set; }
    }
}