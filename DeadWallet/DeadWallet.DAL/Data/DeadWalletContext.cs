using DeadWallet.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace DeadWallet.DAL
{
    public class DeadWalletContext : DbContext
    {
        public DeadWalletContext (DbContextOptions<DeadWalletContext> options)
            : base(options)
        {
        }
        public DbSet<DeadWalletUser> DeadWalletUsers { get; set; }
        public DbSet<Budget> Budgets { get; set; }
        public DbSet<UserBudget> UserBudgets { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserBudget>()
                .HasKey(ub => new { ub.UserId, ub.BudgetId });

            modelBuilder.Entity<UserBudget>()
                .HasOne(ub => ub.User)
                .WithMany(u => u.UserBudgets)
                .HasForeignKey(ub => ub.UserId);

            modelBuilder.Entity<UserBudget>()
                .HasOne(ub => ub.Budget)
                .WithMany(b => b.UserBudgets)
                .HasForeignKey(ub => ub.BudgetId);

            modelBuilder.Entity<Budget>()
                .HasOne(b => b.Owner)
                .WithMany(u => u.OwnedBudgets)
                .HasForeignKey(b => b.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Budget)
                .WithMany(b => b.Transactions)
                .HasForeignKey(t => t.BudgetId);
        }
    }
}
