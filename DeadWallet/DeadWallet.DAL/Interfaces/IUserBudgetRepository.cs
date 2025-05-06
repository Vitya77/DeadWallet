using DeadWallet.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadWallet.DAL.Interfaces
{
    public interface IUserBudgetRepository
    {
        Task AddUserBudgetAsync(UserBudget userBudget);
    }
}
