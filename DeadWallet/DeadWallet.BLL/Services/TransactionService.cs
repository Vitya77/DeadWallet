using DeadWallet.BLL.Interfaces;
using DeadWallet.DAL.Interfaces;
using DeadWallet.DAL.Models;
using Microsoft.Extensions.Logging;


namespace DeadWallet.BLL.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IBudgetRepository _budgetRepository;
        private readonly ILogger<TransactionService> _logger;

        public TransactionService(
            ITransactionRepository transactionRepository,
            IBudgetRepository budgetRepository,
            ILogger<TransactionService> logger)
        {
            _transactionRepository = transactionRepository;
            _budgetRepository = budgetRepository;
            _logger = logger;
        }

        public async Task AddTransactionAsync(Transaction transaction)
        {
            _logger.LogInformation($"Adding new transaction. BudgetId: {transaction.BudgetId}, Amount: {transaction.Amount}, IsExpense: {transaction.IsExpense}");

            await _transactionRepository.AddAsync(transaction);

            var budget = await _budgetRepository.GetByIdAsync(transaction.BudgetId);
            if (budget != null)
            {
                var balanceChange = transaction.IsExpense ? -transaction.Amount : transaction.Amount;
                budget.Balance += balanceChange;
                await _budgetRepository.UpdateAsync(budget);
                _logger.LogInformation($"Updated budget {budget.Id} balance by {balanceChange}. New balance: {budget.Balance}");
            }
            else
            {
                _logger.LogWarning($"Budget with ID {transaction.BudgetId} not found when updating balance");
            }
        }

        public async Task<Transaction?> GetTransactionByIdAsync(int id)
        {
            _logger.LogDebug($"Getting transaction by ID: {id}");
            return await _transactionRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Transaction>> GetAllTransactionsAsync()
        {
            _logger.LogDebug("Getting all transactions");
            return await _transactionRepository.GetAllAsync();
        }

        public async Task UpdateTransactionAsync(Transaction transaction)
        {
            _logger.LogInformation($"Updating transaction ID: {transaction.Id}");
            await _transactionRepository.UpdateAsync(transaction);
            _logger.LogInformation($"Transaction {transaction.Id} updated successfully");
        }

        public async Task DeleteTransactionAsync(int id)
        {
            _logger.LogInformation($"Attempting to delete transaction ID: {id}");

            var transaction = await _transactionRepository.GetByIdAsync(id);
            if (transaction != null)
            {
                await _transactionRepository.DeleteAsync(id);
                _logger.LogInformation($"Transaction {id} deleted successfully");

                var budget = await _budgetRepository.GetByIdAsync(transaction.BudgetId);
                if (budget != null)
                {
                    var balanceChange = transaction.IsExpense ? transaction.Amount : -transaction.Amount;
                    budget.Balance += balanceChange;
                    await _budgetRepository.UpdateAsync(budget);
                    _logger.LogInformation($"Updated budget {budget.Id} balance by {balanceChange}. New balance: {budget.Balance}");
                }
                else
                {
                    _logger.LogWarning($"Budget with ID {transaction.BudgetId} not found when reverting transaction");
                }
            }
            else
            {
                _logger.LogWarning($"Transaction with ID {id} not found for deletion");
            }
        }

        public async Task<IEnumerable<Transaction>> GetTransactionsForBudgetAsync(int budgetId)
        {
            _logger.LogDebug($"Getting transactions for budget ID: {budgetId}");

            var allTransactions = await _transactionRepository.GetAllAsync();
            var filteredTransactions = allTransactions
                .Where(t => t.BudgetId == budgetId)
                .OrderByDescending(t => t.Id)
                .ToList();

            _logger.LogDebug($"Found {filteredTransactions.Count} transactions for budget {budgetId}");
            return filteredTransactions;
        }
    }
}