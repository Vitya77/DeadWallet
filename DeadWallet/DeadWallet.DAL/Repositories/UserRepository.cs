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

        public async Task<DeadWalletUser?> FindUserByEmailAsync(string email)
        {
            return await _context.DeadWalletUsers.FirstOrDefaultAsync(u => u.Email == email);
        }
        public async Task<DeadWalletUser?> FindUserByIdAsync(int id)
        {
            return await _context.DeadWalletUsers.FirstOrDefaultAsync(u => u.Id == id);
        }


        public async Task CreateUserAsync(DeadWalletUser user)
        {
            await _context.DeadWalletUsers.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateUserAsync(DeadWalletUser user)
        {
            _context.DeadWalletUsers.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task<List<DeadWalletUser>> GetAllUsersAsync()
        {
            return await _context.DeadWalletUsers.ToListAsync();
        }

        public async Task DeleteUserByIdAsync(int id)
        {
            var user = await _context.DeadWalletUsers.FindAsync(id);
            if (user != null)
            {
                _context.DeadWalletUsers.Remove(user);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<DeadWalletUser>> SearchUsersAsync(string query)
        {
            return await _context.DeadWalletUsers
                .Where(user =>
                    user.Username.Contains(query) ||
                    user.Email.Contains(query) ||
                    user.FirstName.Contains(query) ||
                    user.LastName.Contains(query))
                .ToListAsync();
        }
    }
}
