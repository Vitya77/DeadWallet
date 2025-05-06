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
    public class UserBudgetRepository: IUserBudgetRepository
    {
        private readonly DeadWalletContext _context;

        public UserBudgetRepository(DeadWalletContext context)
        {
            _context = context;
        }

        public async Task AddUserBudgetAsync(UserBudget userBudget)
        {
            _context.UserBudgets.Add(userBudget);
            await _context.SaveChangesAsync();
        }

    }
}
