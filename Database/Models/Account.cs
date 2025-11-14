using Microsoft.AspNetCore.Identity;

namespace ExpenseTracker.Database.Models
{
    public class Account
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal InitialBalance { get; set; }
        public string IdentityUserId {get; set;}
        public IdentityUser IdentityUser { get; set; }
        public ICollection<Transaction> Transactions { get; set; }
    }
}