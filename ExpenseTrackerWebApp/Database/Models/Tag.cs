using Microsoft.AspNetCore.Identity;

namespace ExpenseTrackerWebApp.Database.Models
{
    public class Tag : IEquatable<Tag>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Color { get; set; }
        public string IdentityUserId { get; set; }
        public IdentityUser IdentityUser { get; set; }
        public ICollection<TransactionTag> TransactionTags { get; set; }

        public Tag()
        {
            Name = "";
            TransactionTags = new List<TransactionTag>();
        }

        public bool Equals(Tag? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id == other.Id;
        }

        public override bool Equals(object? obj) => obj is Tag tag && Equals(tag);

        public override int GetHashCode() => Id.GetHashCode();
        public override string ToString() => Name;
    }
}
