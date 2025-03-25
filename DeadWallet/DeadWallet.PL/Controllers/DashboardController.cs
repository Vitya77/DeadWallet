using DeadWallet.BLL.Interfaces;
using DeadWallet.BLL.Models;
using DeadWallet.BLL.Services;
using DeadWallet.DAL.Models;
using DeadWallet.PL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DeadWallet.PL.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IBudgetService _budgetService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(IBudgetService budgetService, ILogger<DashboardController> logger)
        {
            _budgetService = budgetService;
            _logger = logger;
        }

        [Authorize]
        async public Task<IActionResult> Index()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                _logger.LogWarning("Cannot find user id in JWT token");
                return Redirect("/Home");
            }

            int parsedUserId;
            if (!int.TryParse(userId, out parsedUserId))
            {
                _logger.LogWarning("Cannot parse user id from JWT");
                return Redirect("/Home");
            }
            Result<IEnumerable<Budget>> res = await _budgetService.GetOwnedBudgetsByUserIdAsync(parsedUserId);
            if (res.Success)
            {
                _logger.LogInformation("Successfully got user's budgets");
                IEnumerable<Budget> budgets = res.Res;
                return View(budgets);
            }
            else 
            {
                _logger.LogError($"Error while getting user budgets: {res.Message}");
                return Redirect("/Home");
            }
            
        }

        public IActionResult CreateBudget()
        {
            _logger.LogInformation("User visited budget creation form");
            return View(new BudgetViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> CreateBudget(BudgetViewModel model)
        {
            _logger.LogInformation("User submited budget creation form");
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                _logger.LogWarning("Cannot find user id in JWT token");
                return Redirect("/Home");
            }

            int parsedUserId;
            if (!int.TryParse(userId, out parsedUserId))
            {
                _logger.LogWarning("Cannot parse user id from JWT token");
                return Redirect("/Home");
            }

            if (ModelState.IsValid)
            {
                var budget = new Budget
                {
                    Title = model.Title,
                    Balance = model.Balance,
                    OwnerId = parsedUserId
                };
                Result res = await _budgetService.CreateBudgetAsync(budget);
                if (res.Success)
                {
                    _logger.LogInformation("Succesfully created budget");
                    return Redirect("/Dashboard");
                }
                else
                {
                    _logger.LogInformation($"Error while creating budget: {res.Message}");
                    ModelState.AddModelError("", $"Error: {res.Message}");
                }
            }

            _logger.LogInformation("Submitted data is invalid");
            return View(model);
        }
    }
}
