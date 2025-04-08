using DeadWallet.DAL.Interfaces;
using DeadWallet.DAL.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DeadWallet.DAL.Repositories
{
    public class TagRepository : ITagRepository
    {
        private readonly DeadWalletContext _context;

        public TagRepository(DeadWalletContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Tag tag)
        {
            _context.Tags.Add(tag);
            await _context.SaveChangesAsync();
        }

        public async Task<Tag?> GetByIdAsync(int id) =>
            await _context.Tags.FindAsync(id);

        public async Task<IEnumerable<Tag>> GetAllAsync() =>
            await _context.Tags.ToListAsync();

        public async Task UpdateAsync(Tag tag)
        {
            _context.Tags.Update(tag);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var tag = await _context.Tags.FindAsync(id);
            if (tag != null)
            {
                _context.Tags.Remove(tag);
                await _context.SaveChangesAsync();
            }
        }
    }
}
