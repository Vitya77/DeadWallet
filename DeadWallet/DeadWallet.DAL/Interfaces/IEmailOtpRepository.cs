using DeadWallet.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadWallet.DAL.Interfaces
{
    public interface IEmailOtpRepository
    {
        Task SaveOtpAsync(EmailOtp otp);
        Task<EmailOtp?> GetOtpByEmailAsync(string email);
        Task DeleteOtpAsync(string email);
    }
}
