using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeadWallet.DAL.Models
{
    public class Budget
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Balance { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public int OwnerId { get; set; }
        public DeadWalletUser Owner { get; set; } = null!;

        public ICollection<UserBudget> UserBudgets { get; set; } = new List<UserBudget>();
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}
