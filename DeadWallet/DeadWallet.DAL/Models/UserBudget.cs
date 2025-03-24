using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadWallet.DAL.Models
{
    public class UserBudget
    {
        [ForeignKey("DeadWalletUser")]
        public int UserId { get; set; }
        public DeadWalletUser User { get; set; } = null!;
        [ForeignKey("Budget")]
        public int BudgetId { get; set; }
        public Budget Budget { get; set; } = null!;
    }
}
