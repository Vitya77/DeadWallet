using DeadWallet.BLL.Interfaces;
using DeadWallet.DAL.Models;
using DeadWallet.PL.Models;
using Microsoft.AspNetCore.Mvc;

namespace DeadWallet.PL.Controllers
{
    public class TransactionController : Controller
    {
        private readonly ITransactionService _transactionService;
        private readonly ILogger<TransactionController> _logger;

        public TransactionController(
            ITransactionService transactionService,
            ILogger<TransactionController> logger)
        {
            _transactionService = transactionService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> AddTransaction(TransactionViewModel model)
        {
            if (string.IsNullOrEmpty(model.Description))
            {
                ModelState.Remove("Description");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Transaction model validation failed");

                foreach (var entry in ModelState)
                {
                    foreach (var error in entry.Value.Errors)
                    {
                        _logger.LogWarning($"Validation error - {entry.Key}: {error.ErrorMessage}");
                    }
                }

                TempData["ErrorMessage"] = "Please fill all required fields correctly.";
                return RedirectToAction("Index", "Home");
            }

            var transaction = new Transaction
            {
                Amount = model.Amount,
                Description = model.Description ?? string.Empty,
                IsExpense = model.IsExpense,
                BudgetId = model.BudgetId
            };

            var result = await _transactionService.AddTransactionAsync(transaction);

            if (result.Success)
            {
                _logger.LogInformation($"Transaction added successfully. Type: {(model.IsExpense ? "Expense" : "Income")}, Amount: {model.Amount:C}");
            }
            else
            {
                _logger.LogError($"Failed to add transaction: {result.Message}");
                TempData["ErrorMessage"] = result.Message;
            }

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> BudgetTransactions(int budgetId)
        {
            var allTransactionsResult = await _transactionService.GetAllTransactionsAsync();
            if (!allTransactionsResult.Success)
            {
                _logger.LogError($"Failed to get transactions: {allTransactionsResult.Message}");
                return NotFound();
            }

            var transactions = allTransactionsResult.Res
                .Where(t => t.BudgetId == budgetId)
                .OrderByDescending(t => t.CreatedAt)
                .ToList();

            string budgetName = transactions.FirstOrDefault()?.Budget?.Title ?? "unknown budget";
            ViewBag.BudgetName = budgetName;
            return View(transactions);
        }
    }
}