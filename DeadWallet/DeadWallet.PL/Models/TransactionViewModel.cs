using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeadWallet.PL.Models
{
    public class TransactionViewModel
    {
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public string Description { get; set; } = string.Empty;

        public bool IsExpense { get; set; }

        [Required]
        public int BudgetId { get; set; }
    }
}