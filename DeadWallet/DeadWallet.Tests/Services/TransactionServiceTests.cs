using DeadWallet.BLL.Interfaces;
using DeadWallet.BLL.Services;
using DeadWallet.DAL.Interfaces;
using DeadWallet.DAL.Models;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace DeadWallet.BLL.Tests.Services
{
    public class TransactionServiceTests
    {
        private readonly Mock<ITransactionRepository> _transactionRepoMock;
        private readonly Mock<IBudgetRepository> _budgetRepoMock;
        private readonly TransactionService _transactionService;

        public TransactionServiceTests()
        {
            _transactionRepoMock = new Mock<ITransactionRepository>();
            _budgetRepoMock = new Mock<IBudgetRepository>();
            _transactionService = new TransactionService(
                _transactionRepoMock.Object,
                _budgetRepoMock.Object);
        }

        [Fact]
        public async Task AddTransactionAsync_ShouldUpdateBudgetBalance_ForExpense()
        {
            // Arrange
            var transaction = new Transaction
            {
                Id = 1,
                Amount = 100,
                IsExpense = true,
                BudgetId = 1
            };
            var budget = new Budget { Id = 1, Balance = 500 };

            _budgetRepoMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(budget);

            // Act
            await _transactionService.AddTransactionAsync(transaction);

            // Assert
            _transactionRepoMock.Verify(x => x.AddAsync(transaction), Times.Once);
            _budgetRepoMock.Verify(x => x.UpdateAsync(It.Is<Budget>(b => b.Balance == 400)), Times.Once);
        }

        [Fact]
        public async Task AddTransactionAsync_ShouldUpdateBudgetBalance_ForIncome()
        {
            // Arrange
            var transaction = new Transaction
            {
                Id = 1,
                Amount = 100,
                IsExpense = false,
                BudgetId = 1
            };
            var budget = new Budget { Id = 1, Balance = 500 };

            _budgetRepoMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(budget);

            // Act
            await _transactionService.AddTransactionAsync(transaction);

            // Assert
            _transactionRepoMock.Verify(x => x.AddAsync(transaction), Times.Once);
            _budgetRepoMock.Verify(x => x.UpdateAsync(It.Is<Budget>(b => b.Balance == 600)), Times.Once);
        }

        [Fact]
        public async Task AddTransactionAsync_ShouldNotUpdateBudget_WhenBudgetNotFound()
        {
            // Arrange
            var transaction = new Transaction { BudgetId = 1 };

            _budgetRepoMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync((Budget)null);

            // Act
            await _transactionService.AddTransactionAsync(transaction);

            // Assert
            _transactionRepoMock.Verify(x => x.AddAsync(transaction), Times.Once);
            _budgetRepoMock.Verify(x => x.UpdateAsync(It.IsAny<Budget>()), Times.Never);
        }

        [Fact]
        public async Task GetTransactionByIdAsync_ShouldReturnTransaction()
        {
            // Arrange
            var expectedTransaction = new Transaction { Id = 1 };
            _transactionRepoMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(expectedTransaction);

            // Act
            var result = await _transactionService.GetTransactionByIdAsync(1);

            // Assert
            Assert.Equal(expectedTransaction, result);
        }

        [Fact]
        public async Task GetAllTransactionsAsync_ShouldReturnAllTransactions()
        {
            // Arrange
            var transactions = new List<Transaction>
            {
                new Transaction { Id = 1 },
                new Transaction { Id = 2 }
            };
            _transactionRepoMock.Setup(x => x.GetAllAsync())
                .ReturnsAsync(transactions);

            // Act
            var result = await _transactionService.GetAllTransactionsAsync();

            // Assert
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task UpdateTransactionAsync_ShouldCallRepository()
        {
            // Arrange
            var transaction = new Transaction { Id = 1 };

            // Act
            await _transactionService.UpdateTransactionAsync(transaction);

            // Assert
            _transactionRepoMock.Verify(x => x.UpdateAsync(transaction), Times.Once);
        }

        [Fact]
        public async Task DeleteTransactionAsync_ShouldUpdateBudgetBalance_WhenTransactionExists()
        {
            // Arrange
            var transaction = new Transaction
            {
                Id = 1,
                Amount = 100,
                IsExpense = true,
                BudgetId = 1
            };
            var budget = new Budget { Id = 1, Balance = 400 };

            _transactionRepoMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(transaction);
            _budgetRepoMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(budget);

            // Act
            await _transactionService.DeleteTransactionAsync(1);

            // Assert
            _transactionRepoMock.Verify(x => x.DeleteAsync(1), Times.Once);
            _budgetRepoMock.Verify(x => x.UpdateAsync(It.Is<Budget>(b => b.Balance == 500)), Times.Once);
        }

        [Fact]
        public async Task DeleteTransactionAsync_ShouldDoNothing_WhenTransactionNotFound()
        {
            // Arrange
            _transactionRepoMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync((Transaction)null);

            // Act
            await _transactionService.DeleteTransactionAsync(1);

            // Assert
            _transactionRepoMock.Verify(x => x.DeleteAsync(It.IsAny<int>()), Times.Never);
            _budgetRepoMock.Verify(x => x.UpdateAsync(It.IsAny<Budget>()), Times.Never);
        }

        [Fact]
        public async Task GetTransactionsForBudgetAsync_ShouldReturnFilteredTransactions()
        {
            // Arrange
            var transactions = new List<Transaction>
            {
                new Transaction { Id = 1, BudgetId = 1 },
                new Transaction { Id = 2, BudgetId = 2 },
                new Transaction { Id = 3, BudgetId = 1 }
            };
            _transactionRepoMock.Setup(x => x.GetAllAsync())
                .ReturnsAsync(transactions);

            // Act
            var result = await _transactionService.GetTransactionsForBudgetAsync(1);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.All(result, t => Assert.Equal(1, t.BudgetId));
            Assert.Equal(3, result.First().Id); // Verify ordering
        }
    }
}