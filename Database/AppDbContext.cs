using ExpenseTracker.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Database
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Account> Accounts { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Transaction>()
                .Property(t => t.Amount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Account>()
                .Property(t => t.InitialBalace)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Category>().HasData(
                new Category() { Id = 1, Name = "Groceries", Type=TransactionType.Expense},
                new Category() { Id = 2, Name = "Eating Out", Type=TransactionType.Expense},
                new Category() { Id = 3, Name = "Shopping", Type=TransactionType.Expense},
                new Category() { Id = 4, Name = "Transportation", Type=TransactionType.Expense},
                new Category() { Id = 5, Name = "Vehicle", Type=TransactionType.Expense},
                new Category() { Id = 6, Name = "Communication", Type=TransactionType.Expense},
                new Category() { Id = 7, Name = "Health and Wellness", Type=TransactionType.Expense},
                new Category() { Id = 8, Name = "Education", Type=TransactionType.Expense},
                new Category() { Id = 9, Name = "Entertainment", Type=TransactionType.Expense},
                new Category() { Id = 10, Name = "Pets", Type=TransactionType.Expense},
                new Category() { Id = 11, Name = "Salary", Type=TransactionType.Income}
            );
        }
    }
}