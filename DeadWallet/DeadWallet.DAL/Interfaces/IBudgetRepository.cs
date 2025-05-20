using DeadWallet.DAL.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DeadWallet.DAL.Interfaces
{
    public interface IBudgetRepository
    {

        Task<Budget?> GetBudgetByIdAsync(int budgetId);
        Task<Budget?> GetByIdAsync(int id);
        Task<Budget?> GetBudgetWithTransactionsAsync(int budgetId); // Новий метод
        Task UpdateAsync(Budget budget);
        Task<IEnumerable<Budget>> GetBudgetsByUserIdAsync(int userId);
        Task<IEnumerable<Budget>> GetOwnedBudgetsByUserIdAsync(int userId);
        Task CreateBudgetAsync(Budget budget);
        Task<bool> DeleteBudgetAsync(int id);
    }
}