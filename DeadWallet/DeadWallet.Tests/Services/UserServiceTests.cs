using DeadWallet.BLL.Models;
using DeadWallet.BLL.Services;
using DeadWallet.DAL.Models;
using DeadWallet.DAL.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;
using Moq;
using DeadWallet.PL.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Reflection;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IPasswordHasher<DeadWalletUser>> _mockPasswordHasher;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockPasswordHasher = new Mock<IPasswordHasher<DeadWalletUser>>();
        _mockConfiguration = new Mock<IConfiguration>();

        _userService = new UserService(
            _mockUserRepository.Object,
            _mockPasswordHasher.Object,
            _mockConfiguration.Object
        );
    }

    [Fact]
    public async Task RegisterAsync_UserAlreadyExists_ThrowsException()
    {
        // Arrange
        var registrationModel = new RegistrationModel
        {
            Username = "existingUser",
            FirstName = "John",
            LastName = "Doe",
            Password = "password123"
        };

        _mockUserRepository
            .Setup(repo => repo.FindUserByUsernameAsync(registrationModel.Username))
            .ReturnsAsync(new DeadWalletUser { 
                Username = "existingUser",
                FirstName = "John",
                LastName = "Doe",
                Password = "password123"
            });

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _userService.RegisterAsync(registrationModel));
    }

    [Fact]
    public async Task RegisterAsync_NewUser_ReturnsJwtToken()
    {
        var registrationModel = new RegistrationModel
        {
            Username = "newUser",
            FirstName = "Jane",
            LastName = "Doe",
            Password = "password123"
        };

        _mockUserRepository
            .Setup(repo => repo.FindUserByUsernameAsync(registrationModel.Username))
            .ReturnsAsync((DeadWalletUser)null);

        _mockUserRepository
            .Setup(repo => repo.CreateUserAsync(It.IsAny<DeadWalletUser>()))
            .Returns(Task.CompletedTask);

        _mockPasswordHasher
            .Setup(hasher => hasher.HashPassword(It.IsAny<DeadWalletUser>(), registrationModel.Password))
            .Returns("hashedPassword");

        _mockConfiguration
            .Setup(config => config["Jwt:SecretKey"])
            .Returns("your-secret-key-with-at-least-32-chars");

        _mockConfiguration
            .Setup(config => config["Jwt:ExpirationTimeDays"])
            .Returns("7");

        _mockConfiguration
            .Setup(config => config["Jwt:Issuer"])
            .Returns("TestIssuer");

        _mockConfiguration
            .Setup(config => config["Jwt:Audience"])
            .Returns("TestAudience");

        var result = await _userService.RegisterAsync(registrationModel);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }
}

public class AuthControllerTests
{
    private readonly Mock<UserService> _mockUserService;
    private readonly Mock<ILogger<AuthController>> _mockLogger;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        // Створюємо моки для залежностей UserService
        var mockUserRepo = new Mock<IUserRepository>();
        var mockPasswordHasher = new Mock<IPasswordHasher<DeadWalletUser>>();
        var mockConfig = new Mock<IConfiguration>();

        // Ініціалізуємо мок UserService з усіма необхідними залежностями
        _mockUserService = new Mock<UserService>(
            mockUserRepo.Object,
            mockPasswordHasher.Object,
            mockConfig.Object
        );

        _mockLogger = new Mock<ILogger<AuthController>>();
        _controller = new AuthController(_mockUserService.Object, _mockLogger.Object);
    }

    [Fact]
    public void Logout_Should_DeleteAuthTokenCookie_AndRedirectToHome()
    {
        // Arrange
        var responseCookiesMock = new Mock<IResponseCookies>();
        var httpResponseMock = new Mock<HttpResponse>();
        httpResponseMock.SetupGet(r => r.Cookies).Returns(responseCookiesMock.Object);

        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.SetupGet(c => c.Response).Returns(httpResponseMock.Object);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContextMock.Object
        };

        // Act
        var result = _controller.Logout() as RedirectToActionResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Index", result.ActionName);
        Assert.Equal("Home", result.ControllerName);
        responseCookiesMock.Verify(c => c.Delete("AuthToken"), Times.Once);
    }
}