namespace ExpenseTracker.Dtos
{
    public class AccountWithBalance
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal InitialBalance { get; set; }
        public decimal CurrentBalance { get; set; }
    }
}