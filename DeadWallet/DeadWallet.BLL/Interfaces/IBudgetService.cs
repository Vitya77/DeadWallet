using DeadWallet.BLL.Models;
using DeadWallet.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadWallet.BLL.Interfaces
{
    public interface IBudgetService
    {
        public Task<Result<IEnumerable<Budget>>> GetOwnedBudgetsByUserIdAsync(int userId);
        public Task<Result<IEnumerable<Budget>>> GetBudgetsByUserIdAsync(int userId);
        public Task<Result> CreateBudgetAsync(Budget budget);
        public Task<Result> AddUserToBudgetAsync(int budgetId, int userId);
        public Task<Result> UpdateBudgetAsync(Budget budget, int userId);
        public Task<Result<Budget>> GetBudgetByIdAsync(int budgetId, int userId);
        public Task<Result> RemoveUsersFromBudget(int budgetId, int userId);
    }
}
