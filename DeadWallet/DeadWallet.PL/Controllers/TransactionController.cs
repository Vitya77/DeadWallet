using DeadWallet.BLL.Interfaces;
using DeadWallet.DAL.Models;
using DeadWallet.PL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

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

            try
            {
                await _transactionService.AddTransactionAsync(transaction);
                _logger.LogInformation($"Transaction added successfully. Type: {(model.IsExpense ? "Expense" : "Income")}, Amount: {model.Amount:C}");
                TempData["SuccessMessage"] = $"Transaction added successfully! {(model.IsExpense ? "Expense" : "Income")}: {model.Amount:C}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add transaction");
                TempData["ErrorMessage"] = "Failed to add transaction. Please try again.";
            }

            return RedirectToAction("Index", "Home");
        }
    }
}