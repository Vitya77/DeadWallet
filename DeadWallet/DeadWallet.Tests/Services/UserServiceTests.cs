using DeadWallet.BLL.Models;
using DeadWallet.BLL.Services;
using DeadWallet.DAL.Models;
using DeadWallet.DAL.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;
using Moq;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Http;

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
            .ReturnsAsync(new DeadWalletUser
            {
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

    [Fact]
    public void Logout_Should_DeleteAuthToken()
    {
        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        var mockHttpResponse = new Mock<HttpResponse>();
        var mockCookies = new Mock<IResponseCookies>();
        var mockHttpContext = new Mock<HttpContext>();

        mockHttpResponse.Setup(r => r.Cookies).Returns(mockCookies.Object);
        mockHttpContext.Setup(c => c.Response).Returns(mockHttpResponse.Object);
        mockHttpContextAccessor.Setup(c => c.HttpContext).Returns(mockHttpContext.Object);

        var userService = new UserService(
            _mockUserRepository.Object,
            _mockPasswordHasher.Object,
            _mockConfiguration.Object
        );

        userService.Logout(mockHttpContextAccessor.Object);

        mockCookies.Verify(c => c.Delete("AuthToken"), Times.Once);
    }

}
