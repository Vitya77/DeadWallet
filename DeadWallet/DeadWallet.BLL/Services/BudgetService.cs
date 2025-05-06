using DeadWallet.BLL.Interfaces;
using DeadWallet.BLL.Models;
using DeadWallet.DAL.Interfaces;
using DeadWallet.DAL.Models;
using DeadWallet.DAL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadWallet.BLL.Services
{
    public class BudgetService: IBudgetService
    {
        private readonly IBudgetRepository _budgetRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUserBudgetRepository _userBudgetRepository;

        public BudgetService(IBudgetRepository budgetRepository, IUserRepository userRepository, IUserBudgetRepository userBudgetRepository)
        {
            _budgetRepository = budgetRepository;
            _userRepository = userRepository;
            _userBudgetRepository = userBudgetRepository;
        }

        public async Task<Result<IEnumerable<Budget>>> GetOwnedBudgetsByUserIdAsync(int userId)
        {
            return new Result<IEnumerable<Budget>> 
            {
                Success = true,
                Res = await _budgetRepository.GetOwnedBudgetsByUserIdAsync(userId)
            };
        }

        public async Task<Result<IEnumerable<Budget>>> GetBudgetsByUserIdAsync(int userId)
        {
            return new Result<IEnumerable<Budget>>
            {
                Success = true,
                Res = await _budgetRepository.GetBudgetsByUserIdAsync(userId)
            };
        }

        public async Task<Result> CreateBudgetAsync(Budget budget)
        {
            if (budget == null)
            {
                return new Result
                {
                    Success = false,
                    Message = "Budget cannot be null."
                };
            }

            await _budgetRepository.CreateBudgetAsync(budget);
            return new Result
            {
                Success = true
            };
        }

        public async Task<Result> AddUserToBudgetAsync(int budgetId, int userId)
        {
            var budget = await _budgetRepository.GetBudgetByIdAsync(budgetId);
            if (budget == null)
            {
                return new Result { Success = false, Message = "Budget not found" };
            }

            var user = await _userRepository.FindUserByIdAsync(userId);
            if (user == null)
            {
                return new Result { Success = false, Message = "User not found" };
            }

            var userBudget = new UserBudget
            {
                BudgetId = budgetId,
                UserId = userId
            };

            await _userBudgetRepository.AddUserBudgetAsync(userBudget);
            return new Result { Success = true };
        }

    }
}
