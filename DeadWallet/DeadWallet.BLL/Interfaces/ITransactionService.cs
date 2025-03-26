using DeadWallet.DAL.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DeadWallet.BLL.Interfaces;

public interface ITransactionService
{
    Task AddTransactionAsync(Transaction transaction);
    Task<Transaction?> GetTransactionByIdAsync(int id);
    Task<IEnumerable<Transaction>> GetAllTransactionsAsync();
    Task UpdateTransactionAsync(Transaction transaction);
    Task DeleteTransactionAsync(int id);
    Task<IEnumerable<Transaction>> GetTransactionsForBudgetAsync(int budgetId); // Додано новий метод

}