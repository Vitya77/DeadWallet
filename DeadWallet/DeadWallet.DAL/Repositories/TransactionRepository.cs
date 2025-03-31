using DeadWallet.DAL.Interfaces;
using DeadWallet.DAL.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DeadWallet.DAL.Repositories
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly DeadWalletContext _context;

        public TransactionRepository(DeadWalletContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Transaction entity)
        {
            await _context.Transactions.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<Transaction?> GetByIdAsync(int id)
        {
            return await _context.Transactions.FindAsync(id);
        }

        public async Task<IEnumerable<Transaction>> GetAllAsync()
        {
            return await _context.Transactions.ToListAsync();
        }

        public async Task UpdateAsync(Transaction entity)
        {
            _context.Transactions.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.Transactions.FindAsync(id);
            if (entity != null)
            {
                _context.Transactions.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}