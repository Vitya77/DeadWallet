using DeadWallet.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadWallet.DAL.Interfaces
{
    public interface IBudgetRepository
    {
        Task<Budget?> GetBudgetByIdAsync(int budgetId);
        Task<IEnumerable<Budget>> GetBudgetsByUserIdAsync(int userId);
        Task<IEnumerable<Budget>> GetOwnedBudgetsByUserIdAsync(int userId);
        Task CreateBudgetAsync(Budget budget);
    }
}
