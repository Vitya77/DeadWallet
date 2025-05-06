using DeadWallet.BLL.Interfaces;
using DeadWallet.BLL.Models;
using DeadWallet.BLL.Services;
using DeadWallet.DAL.Models;
using DeadWallet.PL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NuGet.Protocol;
using System.Security.Claims;
using System.Text.Json;

namespace DeadWallet.PL.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IBudgetService _budgetService;
        private readonly ILogger<DashboardController> _logger;
        private readonly UserService _userService;      

        public DashboardController(IBudgetService budgetService, UserService userService, ILogger<DashboardController> logger)
        {
            _budgetService = budgetService;
            _userService = userService;
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

            Result<IEnumerable<Budget>> ownedRes = await _budgetService.GetOwnedBudgetsByUserIdAsync(parsedUserId);
            Result<IEnumerable<Budget>> guestRes = await _budgetService.GetBudgetsByUserIdAsync(parsedUserId);

            if (ownedRes.Success && guestRes.Success)
            {
                _logger.LogInformation("Successfully got user's budgets");
                IEnumerable<Budget> ownedBudgets = ownedRes.Res;
                IEnumerable<Budget> guestBudgets = guestRes.Res;   
                var budgets = new List<IEnumerable<Budget>>([ownedBudgets, guestBudgets]);
                return View(budgets);
            }
            else 
            {
                _logger.LogError($"Error while getting user budgets: {ownedRes.Message}, { guestRes.Message }");
                return Redirect("/Home");
            }
            
        }

        public async Task<IActionResult> CreateBudget()
        {
            _logger.LogInformation("User visited budget creation form");
            var users = await _userService.GetAllUsersAsync();
            if (!users.Success) 
            {
                return View(new BudgetViewModel());
            }

            ViewBag.Users = users.Res
                .Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = $"{u.FirstName} {u.LastName}"
                })
                .ToList();

            return View(new BudgetViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> CreateBudget(BudgetViewModel model)
        {
            _logger.LogInformation(Json(model).ToJson());
            _logger.LogInformation("User submitted budget creation form");
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                _logger.LogWarning("Cannot find user id in JWT token");
                return Redirect("/Home");
            }

            if (!int.TryParse(userId, out int parsedUserId))
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
                    foreach (var selectedUserId in model.SelectedUserIds)
                    {
                        await _budgetService.AddUserToBudgetAsync(budget.Id, selectedUserId);
                    }

                    _logger.LogInformation("Successfully created budget");
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

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> SearchUsers(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Json(new List<object>());
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null || !int.TryParse(userId, out int parsedUserId))
            {
                return Unauthorized();
            }

            var result = await _userService.SearchUsers(query, parsedUserId);
            if (!result.Success)
            {
                return Json(new List<object>());
            }

            var users = result.Res.Select(u => new
            {
                u.Id,
                u.FirstName,
                u.LastName
            });

            return Json(users);
        }
    }
}
