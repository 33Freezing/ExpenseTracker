using ExpenseTrackerWebApp.Database.Models;
using ExpenseTrackerWebApp.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTrackerWebApp.Database
{
    public class AppDbContext : IdentityDbContext<IdentityUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public virtual DbSet<Transaction> Transactions { get; set; }
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<Account> Accounts { get; set; }
        public virtual DbSet<Tag> Tags { get; set; }
        public virtual DbSet<TransactionTag> TransactionTags { get; set; }
        public virtual DbSet<UserPreferences> UserPreferences{get; set;}
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserPreferences>()
             .HasKey(up => up.UserId);

             modelBuilder.Entity<UserPreferences>()
                .HasOne(up => up.IdentityUser)
                .WithOne() 
                .HasForeignKey<UserPreferences>(up => up.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Account>()
                .HasOne(a => a.IdentityUser)
                .WithMany()
                .HasForeignKey(a => a.IdentityUserId)
                .IsRequired();

            modelBuilder.Entity<Category>()
                .HasOne(a => a.IdentityUser)
                .WithMany()
                .HasForeignKey(a => a.IdentityUserId)
                .IsRequired();

            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Account)
                .WithMany(a => a.Transactions)
                .HasForeignKey(t => t.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Category)
                .WithMany(a => a.Transactions)
                .HasForeignKey(t => t.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Tag>()
                .HasOne(t => t.IdentityUser)
                .WithMany()
                .HasForeignKey(t => t.IdentityUserId)
                .IsRequired();

            modelBuilder.Entity<TransactionTag>()
                .HasKey(tt => new { tt.TransactionId, tt.TagId });

            modelBuilder.Entity<TransactionTag>()
                .HasOne(tt => tt.Transaction)
                .WithMany(t => t.TransactionTags)
                .HasForeignKey(tt => tt.TransactionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TransactionTag>()
                .HasOne(tt => tt.Tag)
                .WithMany(t => t.TransactionTags)
                .HasForeignKey(tt => tt.TagId)
                .OnDelete(DeleteBehavior.Cascade);

                
            modelBuilder.Entity<Transaction>()
                .Property(t => t.Amount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Account>()
                .Property(t => t.InitialBalance)
                .HasPrecision(18, 2);


            modelBuilder.Entity<IdentityRole>().HasData(new IdentityRole 
            {
                Id = "1b1c59f2-891f-4732-a974-3b755208d0d9", 
                Name = "Administrator", 
                NormalizedName = "ADMINISTRATOR",
                ConcurrencyStamp = "a95a997e-84dd-4ef6-a759-1f36700a41f4"
            });

            modelBuilder.Entity<IdentityUser>().HasData(
            new IdentityUser
            {
                Id = "4e08d54b-16f0-47a0-afaf-afc12dbdedc8",
                UserName = "sa",
                NormalizedUserName = "SA",
                PasswordHash = "AQAAAAIAAYagAAAAEPaPOWihwWXqakYt44g4+tcyL/Re1e5Fx3AiCOMXavq7m9bkrUdn20iJc8ABi9u72A==", // Secret1!
                ConcurrencyStamp = "a95a997e-84dd-4ef6-a759-1f36700a41f4",
                SecurityStamp = "c3691bab-bed8-4bcc-91fa-62bb12e2b245"
            });
            
            modelBuilder.Entity<UserPreferences>().HasData(
            new UserPreferences
            {
                UserId = "4e08d54b-16f0-47a0-afaf-afc12dbdedc8", 
                DarkMode = false
            });
            
            modelBuilder.Entity<IdentityUserRole<string>>().HasData(
            new IdentityUserRole<string>
            {
                RoleId = "1b1c59f2-891f-4732-a974-3b755208d0d9", 
                UserId = "4e08d54b-16f0-47a0-afaf-afc12dbdedc8"
            });
        }
    }
}