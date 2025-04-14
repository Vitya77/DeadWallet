using System.ComponentModel.DataAnnotations;

namespace DeadWallet.PL.Models
{
    public class EmailRecoverPasswordViewModel
    {
        [Required]
        public string Email { get; set; }
    }
}
