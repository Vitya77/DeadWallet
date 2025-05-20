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

    [Fact]
    public async Task UpdateBudgetAsync_ShouldReturnSuccess_WhenOwnerUpdatesBudget()
    {
        var budget = new Budget { Id = 1, Title = "Updated Budget", OwnerId = 1 };

        _budgetRepositoryMock.Setup(repo => repo.UpdateAsync(budget))
                             .Returns(Task.CompletedTask);

        var result = await _budgetService.UpdateBudgetAsync(budget, 1);

        Assert.True(result.Success);
    }

    [Fact]
    public async Task UpdateBudgetAsync_ShouldReturnFailure_WhenBudgetIsNull()
    {
        var result = await _budgetService.UpdateBudgetAsync(null, 1);

        Assert.False(result.Success);
        Assert.Equal("Budget cannot be null.", result.Message);
    }

    [Fact]
    public async Task UpdateBudgetAsync_ShouldReturnFailure_WhenUserIsNotOwner()
    {
        var budget = new Budget { Id = 1, Title = "Updated Budget", OwnerId = 2 };

        var result = await _budgetService.UpdateBudgetAsync(budget, 1);

        Assert.False(result.Success);
        Assert.Equal("Only owner of a budget can edit it.", result.Message);
    }

    [Fact]
    public async Task GetBudgetByIdAsync_ShouldReturnSuccess_WhenUserIsOwner()
    {
        var budget = new Budget { Id = 1, OwnerId = 1, UserBudgets = new List<UserBudget>() };

        _budgetRepositoryMock.Setup(repo => repo.GetBudgetByIdAsync(1))
                             .ReturnsAsync(budget);

        var result = await _budgetService.GetBudgetByIdAsync(1, 1);

        Assert.True(result.Success);
        Assert.NotNull(result.Res);
        Assert.Equal(1, result.Res.Id);
    }

    [Fact]
    public async Task GetBudgetByIdAsync_ShouldReturnSuccess_WhenUserIsSharedUser()
    {
        var budget = new Budget
        {
            Id = 1,
            OwnerId = 2,
            UserBudgets = new List<UserBudget>
        {
            new UserBudget { UserId = 1, BudgetId = 1 }
        }
        };

        _budgetRepositoryMock.Setup(repo => repo.GetBudgetByIdAsync(1))
                             .ReturnsAsync(budget);

        var result = await _budgetService.GetBudgetByIdAsync(1, 1);

        Assert.True(result.Success);
        Assert.NotNull(result.Res);
        Assert.Equal(1, result.Res.Id);
    }

    [Fact]
    public async Task GetBudgetByIdAsync_ShouldReturnFailure_WhenBudgetNotFound()
    {
        _budgetRepositoryMock.Setup(repo => repo.GetBudgetByIdAsync(1))
                             .ReturnsAsync((Budget)null);

        var result = await _budgetService.GetBudgetByIdAsync(1, 1);

        Assert.False(result.Success);
        Assert.Equal("Budget not found", result.Message);
    }

    [Fact]
    public async Task GetBudgetByIdAsync_ShouldReturnFailure_WhenUserHasNoAccess()
    {
        var budget = new Budget
        {
            Id = 1,
            OwnerId = 2,
            UserBudgets = new List<UserBudget>() // No access
        };

        _budgetRepositoryMock.Setup(repo => repo.GetBudgetByIdAsync(1))
                             .ReturnsAsync(budget);

        var result = await _budgetService.GetBudgetByIdAsync(1, 1);

        Assert.False(result.Success);
        Assert.Equal("You do not have access to this budget", result.Message);
    }

    [Fact]
    public async Task RemoveUsersFromBudget_ShouldReturnSuccess_WhenOwnerRemovesUsers()
    {
        var budget = new Budget
        {
            Id = 1,
            OwnerId = 1,
            UserBudgets = new List<UserBudget>
        {
            new UserBudget { UserId = 2, BudgetId = 1 },
            new UserBudget { UserId = 3, BudgetId = 1 }
        }
        };

        _budgetRepositoryMock.Setup(repo => repo.GetBudgetByIdAsync(1))
                             .ReturnsAsync(budget);

        _userBudgetRepositoryMock.Setup(repo => repo.RemoveUserBudgetAsync(It.IsAny<int>(), It.IsAny<int>()))
                                 .Returns(Task.CompletedTask);

        var result = await _budgetService.RemoveUsersFromBudget(1, 1);

        Assert.True(result.Success);
    }

    [Fact]
    public async Task RemoveUsersFromBudget_ShouldReturnFailure_WhenBudgetNotFound()
    {
        _budgetRepositoryMock.Setup(repo => repo.GetBudgetByIdAsync(1))
                             .ReturnsAsync((Budget)null);

        var result = await _budgetService.RemoveUsersFromBudget(1, 1);

        Assert.False(result.Success);
        Assert.Equal("Budget not found", result.Message);
    }

    [Fact]
    public async Task RemoveUsersFromBudget_ShouldReturnFailure_WhenUserIsNotOwner()
    {
        var budget = new Budget
        {
            Id = 1,
            OwnerId = 2, // Not the same as userId
            UserBudgets = new List<UserBudget>()
        };

        _budgetRepositoryMock.Setup(repo => repo.GetBudgetByIdAsync(1))
                             .ReturnsAsync(budget);

        var result = await _budgetService.RemoveUsersFromBudget(1, 1);

        Assert.False(result.Success);
        Assert.Equal("Only owner of a budget can remove users from it.", result.Message);
    }
    [Fact]
    public async Task DeleteBudgetAsync_ShouldReturnSuccess_WhenUserIsOwnerAndBudgetIsDeleted()
    {
        int budgetId = 1;
        int userId = 1;

        var budget = new Budget { Id = budgetId, OwnerId = userId };

        _budgetRepositoryMock.Setup(repo => repo.GetBudgetByIdAsync(budgetId))
                             .ReturnsAsync(budget);
        _budgetRepositoryMock.Setup(repo => repo.DeleteBudgetAsync(budgetId))
                             .ReturnsAsync(true);

        var result = await _budgetService.DeleteBudgetAsync(budgetId, userId);

        Assert.True(result.Success);
        Assert.Null(result.Message);
    }
    [Fact]
    public async Task DeleteBudgetAsync_ShouldReturnFailure_WhenBudgetNotFound()
    {
        int budgetId = 1;
        int userId = 1;

        _budgetRepositoryMock.Setup(repo => repo.GetBudgetByIdAsync(budgetId))
                             .ReturnsAsync((Budget)null);

        var result = await _budgetService.DeleteBudgetAsync(budgetId, userId);

        Assert.False(result.Success);
        Assert.Equal("Budget not found", result.Message);
    }
    [Fact]
    public async Task DeleteBudgetAsync_ShouldReturnFailure_WhenUserIsNotOwner()
    {
        int budgetId = 1;
        int userId = 1;
        var budget = new Budget { Id = budgetId, OwnerId = 2 }; // owner ≠ userId

        _budgetRepositoryMock.Setup(repo => repo.GetBudgetByIdAsync(budgetId))
                             .ReturnsAsync(budget);

        var result = await _budgetService.DeleteBudgetAsync(budgetId, userId);

        Assert.False(result.Success);
        Assert.Equal("Only the owner can delete this budget.", result.Message);
    }
    [Fact]
    public async Task DeleteBudgetAsync_ShouldReturnFailure_WhenRepositoryFailsToDelete()
    {
        int budgetId = 1;
        int userId = 1;
        var budget = new Budget { Id = budgetId, OwnerId = userId };

        _budgetRepositoryMock.Setup(repo => repo.GetBudgetByIdAsync(budgetId))
                             .ReturnsAsync(budget);
        _budgetRepositoryMock.Setup(repo => repo.DeleteBudgetAsync(budgetId))
                             .ReturnsAsync(false); // симуляція помилки

        var result = await _budgetService.DeleteBudgetAsync(budgetId, userId);

        Assert.False(result.Success);
        Assert.Equal("Failed to delete budget.", result.Message);
    }


}