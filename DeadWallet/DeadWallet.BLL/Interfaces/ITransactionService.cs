using DeadWallet.BLL.Models;
using DeadWallet.DAL.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DeadWallet.BLL.Interfaces;

public interface ITransactionService
{
    Task<Result> AddTransactionAsync(Transaction transaction);
    Task<Result<Transaction>> GetTransactionByIdAsync(int id);
    Task<Result<IEnumerable<Transaction>>> GetAllTransactionsAsync();
    Task<Result> UpdateTransactionAsync(Transaction transaction);
    Task<Result> DeleteTransactionAsync(int id);
    Task<Result<IEnumerable<Transaction>>> GetTransactionsForBudgetAsync(int budgetId);
}
