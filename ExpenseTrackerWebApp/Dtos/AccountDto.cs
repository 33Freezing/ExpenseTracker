using ExpenseTrackerWebApp.Database.Models;

namespace ExpenseTrackerWebApp.Dtos
{
    public class AccountDto
    {
        public int? Id {get; set;}
        public string? Name{get; set;}
        public decimal? InitialBalance{get; set;}
        public string? UserId{get; set;}
        public string? Icon{get; set;}
        public string? Color{get; set;}
    }
}