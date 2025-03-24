using DeadWallet.DAL.Interfaces;
using DeadWallet.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace DeadWallet.DAL.Repositories
{
    public class BudgetRepository : IBudgetRepository
    {
        private readonly DeadWalletContext _context;

        public BudgetRepository(DeadWalletContext context)
        {
            _context = context;
        }

        public async Task<Budget?> GetBudgetByIdAsync(int budgetId)
        {
            return await _context.Budgets
                .Include(b => b.Owner)
                .Include(b => b.UserBudgets)
                    .ThenInclude(ub => ub.User)
                .FirstOrDefaultAsync(b => b.Id == budgetId);
        }

        public async Task<IEnumerable<Budget>> GetBudgetsByUserIdAsync(int userId)
        {
            return await _context.Budgets
                .Where(b => b.UserBudgets.Any(ub => ub.UserId == userId))
                .Include(b => b.Owner)
                .ToListAsync();
        }

        public async Task<IEnumerable<Budget>> GetOwnedBudgetsByUserIdAsync(int userId)
        {
            return await _context.Budgets
                .Where(b => b.OwnerId == userId)
                .Include(b => b.UserBudgets)
                .ToListAsync();
        }

        public async Task CreateBudgetAsync(Budget budget)
        {
            await _context.Budgets.AddAsync(budget);
            await _context.SaveChangesAsync();
        }
    }
}
