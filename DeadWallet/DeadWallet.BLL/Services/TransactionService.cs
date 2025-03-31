using DeadWallet.BLL.Interfaces;
using DeadWallet.BLL.Models;
using DeadWallet.DAL.Interfaces;
using DeadWallet.DAL.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public async Task<Result> AddTransactionAsync(Transaction transaction)
        {
            if (transaction == null)
            {
                return new Result { Success = false, Message = "Transaction cannot be null." };
            }

            _logger.LogInformation($"Adding new transaction. BudgetId: {transaction.BudgetId}, Amount: {transaction.Amount}, IsExpense: {transaction.IsExpense}");

            await _transactionRepository.AddAsync(transaction);

            var budget = await _budgetRepository.GetByIdAsync(transaction.BudgetId);
            if (budget == null)
            {
                return new Result { Success = false, Message = $"Budget with ID {transaction.BudgetId} not found." };
            }

            var balanceChange = transaction.IsExpense ? -transaction.Amount : transaction.Amount;
            budget.Balance += balanceChange;
            await _budgetRepository.UpdateAsync(budget);

            _logger.LogInformation($"Updated budget {budget.Id} balance by {balanceChange}. New balance: {budget.Balance}");
            return new Result { Success = true };
        }

        public async Task<Result<Transaction>> GetTransactionByIdAsync(int id)
        {
            _logger.LogDebug($"Getting transaction by ID: {id}");
            var transaction = await _transactionRepository.GetByIdAsync(id);

            return transaction != null
                ? new Result<Transaction> { Success = true, Res = transaction }
                : new Result<Transaction> { Success = false, Message = $"Transaction with ID {id} not found." };
        }

        public async Task<Result<IEnumerable<Transaction>>> GetAllTransactionsAsync()
        {
            _logger.LogDebug("Getting all transactions");
            var transactions = await _transactionRepository.GetAllAsync();
            return new Result<IEnumerable<Transaction>> { Success = true, Res = transactions };
        }

        public async Task<Result> UpdateTransactionAsync(Transaction transaction)
        {
            if (transaction == null)
            {
                return new Result { Success = false, Message = "Transaction cannot be null." };
            }

            _logger.LogInformation($"Updating transaction ID: {transaction.Id}");
            await _transactionRepository.UpdateAsync(transaction);

            return new Result { Success = true };
        }

        public async Task<Result> DeleteTransactionAsync(int id)
        {
            _logger.LogInformation($"Attempting to delete transaction ID: {id}");

            var transaction = await _transactionRepository.GetByIdAsync(id);
            if (transaction == null)
            {
                return new Result { Success = false, Message = $"Transaction with ID {id} not found." };
            }

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

            return new Result { Success = true };
        }

        public async Task<Result<IEnumerable<Transaction>>> GetTransactionsForBudgetAsync(int budgetId)
        {
            _logger.LogDebug($"Getting transactions for budget ID: {budgetId}");

            var allTransactions = await _transactionRepository.GetAllAsync();
            var filteredTransactions = allTransactions
                .Where(t => t.BudgetId == budgetId)
                .OrderByDescending(t => t.Id)
                .ToList();

            return new Result<IEnumerable<Transaction>> { Success = true, Res = filteredTransactions };
        }
    }
}