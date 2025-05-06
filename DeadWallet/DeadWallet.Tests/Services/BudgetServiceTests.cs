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
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IUserBudgetRepository> _userBudgetRepositoryMock;
    private readonly IBudgetService _budgetService;

    public BudgetServiceTests()
    {
        _budgetRepositoryMock = new Mock<IBudgetRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _userBudgetRepositoryMock = new Mock<IUserBudgetRepository>();
        _budgetService = new BudgetService(_budgetRepositoryMock.Object, _userRepositoryMock.Object, _userBudgetRepositoryMock.Object);
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

    [Fact]
    public async Task GetBudgetsByUserIdAsync_ShouldReturnSuccessResult_WithBudgets()
    {
        int userId = 1;
        var budgets = new List<Budget> { new Budget { Id = 1, Title = "Shared Budget" } };
        _budgetRepositoryMock.Setup(repo => repo.GetBudgetsByUserIdAsync(userId))
                             .ReturnsAsync(budgets);

        var result = await _budgetService.GetBudgetsByUserIdAsync(userId);

        Assert.True(result.Success);
        Assert.NotNull(result.Res);
        Assert.Single(result.Res);
        Assert.Equal("Shared Budget", result.Res.First().Title);
    }

    [Fact]
    public async Task AddUserToBudgetAsync_ShouldReturnSuccess_WhenBudgetAndUserExist()
    {
        int userId = 1;
        int budgetId = 2;

        _budgetRepositoryMock.Setup(repo => repo.GetBudgetByIdAsync(budgetId))
                             .ReturnsAsync(new Budget { Id = budgetId });
        _userRepositoryMock.Setup(repo => repo.FindUserByIdAsync(userId))
                           .ReturnsAsync(new DeadWalletUser { Id = userId, FirstName = "", LastName = "", Email = "", Password = "", Username = "" });
        _userBudgetRepositoryMock.Setup(repo => repo.AddUserBudgetAsync(It.IsAny<UserBudget>()))
                                 .Returns(Task.CompletedTask);

        var result = await _budgetService.AddUserToBudgetAsync(budgetId, userId);

        Assert.True(result.Success);
    }

    [Fact]
    public async Task AddUserToBudgetAsync_ShouldReturnFailure_WhenBudgetNotFound()
    {
        int userId = 1;
        int budgetId = 2;

        _budgetRepositoryMock.Setup(repo => repo.GetBudgetByIdAsync(budgetId))
                             .ReturnsAsync((Budget)null);

        var result = await _budgetService.AddUserToBudgetAsync(budgetId, userId);

        Assert.False(result.Success);
        Assert.Equal("Budget not found", result.Message);
    }

    [Fact]
    public async Task AddUserToBudgetAsync_ShouldReturnFailure_WhenUserNotFound()
    {
        int userId = 1;
        int budgetId = 2;

        _budgetRepositoryMock.Setup(repo => repo.GetBudgetByIdAsync(budgetId))
                             .ReturnsAsync(new Budget { Id = budgetId });
        _userRepositoryMock.Setup(repo => repo.FindUserByIdAsync(userId))
                           .ReturnsAsync((DeadWalletUser)null);

        var result = await _budgetService.AddUserToBudgetAsync(budgetId, userId);

        Assert.False(result.Success);
        Assert.Equal("User not found", result.Message);
    }

}