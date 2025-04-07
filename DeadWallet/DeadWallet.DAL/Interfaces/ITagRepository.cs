using DeadWallet.DAL.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DeadWallet.DAL.Interfaces
{
    public interface ITagRepository
    {
        Task AddAsync(Tag tag);
        Task<Tag?> GetByIdAsync(int id);
        Task<IEnumerable<Tag>> GetAllAsync();
        Task UpdateAsync(Tag tag);
        Task DeleteAsync(int id);
    }
}
