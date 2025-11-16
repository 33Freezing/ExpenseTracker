using ExpenseTracker.Database.Models;

namespace ExpenseTracker.Dtos
{
    public class AccountDto
    {
        public int? Id {get; set;}
        public string? Name{get; set;}
        public decimal? InitialBalance{get; set;}
        public string? UserId{get; set;}
    }
}