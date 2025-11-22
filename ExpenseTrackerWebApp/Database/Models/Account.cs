using Microsoft.AspNetCore.Identity;

namespace ExpenseTrackerWebApp.Database.Models
{
    public class Account : IEquatable<Account>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal InitialBalance { get; set; }
        public string? Icon {get; set;}
        public string? Color {get; set;}
        public string IdentityUserId {get; set;}
        public IdentityUser IdentityUser { get; set; }
        public ICollection<Transaction> Transactions { get; set; }

        public bool Equals(Account? other)
        {if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
            return Id == other.Id;
        }

        public override bool Equals(object? obj) => obj is Account account && Equals(account);

        public override int GetHashCode() => Id.GetHashCode();
        public override string ToString() => Name;
    }
}