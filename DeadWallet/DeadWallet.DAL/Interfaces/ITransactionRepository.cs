using DeadWallet.DAL.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DeadWallet.DAL.Interfaces;

public interface ITransactionRepository
{
    Task AddAsync(Transaction entity);
    Task<Transaction?> GetByIdAsync(int id);
    Task<IEnumerable<Transaction>> GetAllAsync();
    Task UpdateAsync(Transaction entity);
    Task DeleteAsync(int id);
}