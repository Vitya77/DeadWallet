using DeadWallet.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadWallet.DAL.Interfaces
{
    public interface IUserRepository
    {
        Task<DeadWalletUser?> FindUserByEmailAsync(string email);
        Task CreateUserAsync(DeadWalletUser user);
        Task UpdateUserAsync(DeadWalletUser user);
    }
}
