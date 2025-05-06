using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DeadWallet.PL.Models
{
    public class BudgetViewModel
    {
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Balance { get; set; }
        [Required]
        public string Title { get; set; }
        public List<int> SelectedUserIds { get; set; } = new List<int>();
    }
}
