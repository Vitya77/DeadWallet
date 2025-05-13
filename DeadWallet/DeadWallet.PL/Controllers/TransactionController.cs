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
        private readonly ITagService _tagService;

        public TransactionController(
        ITransactionService transactionService,
        ITagService tagService,
        ILogger<TransactionController> logger)
        {
            _transactionService = transactionService;
            _tagService = tagService;
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
                BudgetId = model.BudgetId,
                TagId = model.TagId
            };

            var result = await _transactionService.AddTransactionAsync(transaction);

            if (result.Success)
            {
                _logger.LogInformation($"Transaction added successfully. Type: {(model.IsExpense ? "Expense" : "Income")}, Amount: {model.Amount:C}, TagId: {model.TagId}");
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

            try
            {
                var allTags = await _tagService.GetAllTagsAsync();
                ViewBag.Tags = allTags.ToDictionary(tag => tag.Id, tag => tag);

                _logger.LogInformation($"Loaded {allTags.Count()} tags.");

                foreach (var transaction in transactions)
                {
                    if (transaction.TagId.HasValue)
                    {
                        var tag = allTags.FirstOrDefault(t => t.Id == transaction.TagId.Value);
                        if (tag != null)
                        {
                            _logger.LogInformation($"Transaction ID: {transaction.Id} is tagged with: {tag.Name} (TagId: {tag.Id})");
                        }
                        else
                        {
                            _logger.LogWarning($"Transaction ID: {transaction.Id} has a TagId: {transaction.TagId.Value} but no tag found in the list.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Could not load tags for transactions view: {ex.Message}");
                ViewBag.Tags = new Dictionary<int, Tag>();
            }

            return View(transactions);
        }

        [HttpGet]
        public async Task<IActionResult> GetBudgetTotals(int budgetId)
        {
            var incomeResult = await _transactionService.GetTotalIncomeAsync(budgetId);
            var expensesResult = await _transactionService.GetTotalExpensesAsync(budgetId);

            if (!incomeResult.Success || !expensesResult.Success)
            {
                return Json(new { success = false, message = "Error getting budget totals" });
            }

            return Json(new
            {
                success = true,
                income = incomeResult.Res,
                expenses = expensesResult.Res
            });
        }
    }
}