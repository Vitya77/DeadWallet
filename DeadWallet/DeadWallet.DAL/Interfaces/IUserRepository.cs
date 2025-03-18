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
        Task<DeadWalletUser?> FindUserByUsernameAsync(string username);
        Task CreateUserAsync(DeadWalletUser user);
    }
}
