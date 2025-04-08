using DeadWallet.DAL.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DeadWallet.BLL.Interfaces
{
    public interface ITagService
    {
        Task<IEnumerable<Tag>> GetAllTagsAsync();
        Task AddTagAsync(Tag tag);
        Task DeleteTagAsync(int id);
    }
}
