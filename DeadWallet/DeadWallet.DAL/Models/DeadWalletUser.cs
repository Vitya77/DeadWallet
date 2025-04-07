using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadWallet.DAL.Models
{
    public class DeadWalletUser
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public required string FirstName { get; set; }
        [Required]
        public required string LastName { get; set; }
        [Required]
        public required string Username { get; set; }
        [Required]
        public required string Password { get; set; }
        [Required]
        public required string Email { get; set; }
        public ICollection<UserBudget> UserBudgets { get; set; } = new List<UserBudget>();
        public ICollection<Budget> OwnedBudgets { get; set; } = new List<Budget>();
        public string Role { get; set; } = "User";
    }
}
