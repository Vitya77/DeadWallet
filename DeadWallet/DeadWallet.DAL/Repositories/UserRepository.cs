using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeadWallet.DAL.Models;
using Microsoft.EntityFrameworkCore;
using DeadWallet.DAL.Interfaces;

namespace DeadWallet.DAL.Repositories
{
    public class UserRepository: IUserRepository
    {
        private readonly DeadWalletContext _context;

        public UserRepository(DeadWalletContext context)
        {
            _context = context;
        }

        public async Task<DeadWalletUser?> FindUserByUsernameAsync(string username)
        {
            return await _context.DeadWalletUsers.FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task CreateUserAsync(DeadWalletUser user)
        {
            await _context.DeadWalletUsers.AddAsync(user);
            await _context.SaveChangesAsync();
        }
    }
}
