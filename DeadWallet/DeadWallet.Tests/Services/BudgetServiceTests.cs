using System.Collections.Generic;
using System.Threading.Tasks;
using DeadWallet.BLL.Interfaces;
using DeadWallet.BLL.Models;
using DeadWallet.BLL.Services;
using DeadWallet.DAL.Interfaces;
using DeadWallet.DAL.Models;
using Moq;
using Xunit;

public class BudgetServiceTests
{
    private readonly Mock<IBudgetRepository> _budgetRepositoryMock;
    private readonly IBudgetService _budgetService;

    public BudgetServiceTests()
    {
        _budgetRepositoryMock = new Mock<IBudgetRepository>();
        _budgetService = new BudgetService(_budgetRepositoryMock.Object);
    }

    [Fact]
    public async Task GetOwnedBudgetsByUserIdAsync_ShouldReturnSuccessResult_WithBudgets()
    {
        int userId = 1;
        var budgets = new List<Budget> { new Budget { Id = 1, Title = "Test Budget" } };
        _budgetRepositoryMock.Setup(repo => repo.GetOwnedBudgetsByUserIdAsync(userId))
                             .ReturnsAsync(budgets);

        var result = await _budgetService.GetOwnedBudgetsByUserIdAsync(userId);

        Assert.True(result.Success);
        Assert.NotNull(result.Res);
        Assert.Single(result.Res);
    }

    [Fact]
    public async Task CreateBudgetAsync_ShouldReturnSuccess_WhenBudgetIsValid()
    {
        var budget = new Budget { Id = 1, Title = "New Budget" };
        _budgetRepositoryMock.Setup(repo => repo.CreateBudgetAsync(budget))
                             .Returns(Task.CompletedTask);

        var result = await _budgetService.CreateBudgetAsync(budget);

        Assert.True(result.Success);
    }

    [Fact]
    public async Task CreateBudgetAsync_ShouldReturnFailure_WhenBudgetIsNull()
    {
        var result = await _budgetService.CreateBudgetAsync(null);

        Assert.False(result.Success);
        Assert.Equal("Budget cannot be null.", result.Message);
    }
}