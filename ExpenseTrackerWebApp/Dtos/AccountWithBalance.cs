using ExpenseTrackerWebApp.Database.Models;

namespace ExpenseTrackerWebApp.Dtos
{
    public class AccountWithBalance
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public decimal InitialBalance { get; set; }
        public decimal CurrentBalance { get; set; }
        public string? UserId{get; set;}
        public string? Icon { get; set; }
        public string? Color { get; set; }

        public Account ToAccount(){
            return new Account
            {
                Id = this.Id,
                Name = this.Name ?? string.Empty,
                InitialBalance = this.InitialBalance,
                IdentityUserId = this.UserId ?? string.Empty,
                Icon = this.Icon,
                Color = this.Color
            };
        }
    }
}