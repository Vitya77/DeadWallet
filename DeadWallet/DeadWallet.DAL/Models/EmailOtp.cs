using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadWallet.DAL.Models
{
    public class EmailOtp
    {
        [Key]
        public int Id { get; set; }
        public string Email { get; set; }
        public string Code { get; set; }
        public DateTime Expiration { get; set; }
    }
}
