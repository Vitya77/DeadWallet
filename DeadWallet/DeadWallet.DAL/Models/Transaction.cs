using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeadWallet.DAL.Models
{
    public class Transaction
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public bool IsExpense { get; set; }  // true - expense, false - profit

        [Required]
        public int BudgetId { get; set; }

        [ForeignKey("BudgetId")]
        public Budget Budget { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
