using DeadWallet.DAL.Interfaces;
using DeadWallet.DAL.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadWallet.DAL.Repositories
{
    public class EmailOtpRepository : IEmailOtpRepository
    {
        private readonly DeadWalletContext _context;

        public EmailOtpRepository(DeadWalletContext context)
        {
            _context = context;
        }

        public async Task SaveOtpAsync(EmailOtp otp)
        {
            var existing = await _context.EmailOtps.FirstOrDefaultAsync(o => o.Email == otp.Email);

            if (existing != null)
            {
                _context.EmailOtps.Remove(existing);
            }

            await _context.EmailOtps.AddAsync(otp);
            await _context.SaveChangesAsync();
        }

        public async Task<EmailOtp?> GetOtpByEmailAsync(string email)
        {
            return await _context.EmailOtps
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Email == email);
        }

        public async Task DeleteOtpAsync(string email)
        {
            var otp = await _context.EmailOtps.FirstOrDefaultAsync(o => o.Email == email);
            if (otp != null)
            {
                _context.EmailOtps.Remove(otp);
                await _context.SaveChangesAsync();
            }
        }
    }
}
