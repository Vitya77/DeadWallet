using System.ComponentModel.DataAnnotations;

namespace DeadWallet.PL.Models
{
    public class RecoverPasswordViewModel
    {
        [Required]
        [MinLength(6)]
        public string Password { get; set; }
        [Required]
        public string OtpCode { get; set; }
    }
}
