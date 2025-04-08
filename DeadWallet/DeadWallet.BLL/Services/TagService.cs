using DeadWallet.BLL.Interfaces;
using DeadWallet.DAL.Interfaces;
using DeadWallet.DAL.Models;
using DeadWallet.DAL.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DeadWallet.BLL.Services
{
    public class TagService : ITagService
    {
        private readonly ITagRepository _tagRepository;

        public TagService(ITagRepository tagRepository)
        {
            _tagRepository = tagRepository;
        }

        public async Task<IEnumerable<Tag>> GetAllTagsAsync()
        {
            return await _tagRepository.GetAllAsync();
        }

        public async Task AddTagAsync(Tag tag)
        {
            await _tagRepository.AddAsync(tag);
        }

        public async Task DeleteTagAsync(int id)
        {
            await _tagRepository.DeleteAsync(id);
        }
    }
}
