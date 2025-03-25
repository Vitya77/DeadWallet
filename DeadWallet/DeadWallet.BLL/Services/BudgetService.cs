using DeadWallet.BLL.Interfaces;
using DeadWallet.BLL.Models;
using DeadWallet.DAL.Interfaces;
using DeadWallet.DAL.Models;
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

        public BudgetService(IBudgetRepository budgetRepository)
        {
            _budgetRepository = budgetRepository;
        }

        public async Task<Result<IEnumerable<Budget>>> GetOwnedBudgetsByUserIdAsync(int userId)
        {
            return new Result<IEnumerable<Budget>> 
            {
                Success = true,
                Res = await _budgetRepository.GetOwnedBudgetsByUserIdAsync(userId)
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
    }
}
