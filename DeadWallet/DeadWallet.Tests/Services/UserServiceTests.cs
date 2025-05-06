using DeadWallet.BLL.Models;
using DeadWallet.BLL.Services;
using DeadWallet.DAL.Models;
using DeadWallet.DAL.Interfaces;
using DeadWallet.BLL.Interfaces;
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
    private readonly Mock<IEmailOtpRepository> _mockOtpRepository;
    private readonly Mock<IEmailService> _mockEmailService;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockPasswordHasher = new Mock<IPasswordHasher<DeadWalletUser>>();
        _mockConfiguration = new Mock<IConfiguration>();
        _mockOtpRepository = new Mock<IEmailOtpRepository>();
        _mockEmailService = new Mock<IEmailService>();

        _userService = new UserService(
            _mockUserRepository.Object,
            _mockPasswordHasher.Object,
            _mockConfiguration.Object,
            _mockOtpRepository.Object,
            _mockEmailService.Object
        );
    }

    [Fact]
    public async Task RegisterAsync_UserAlreadyExists_ThrowsException()
    {
        // Arrange
        var registrationModel = new RegistrationModel
        {
            Email = "someemail@gmail.com",
            Username = "existingUser",
            FirstName = "John",
            LastName = "Doe",
            Password = "password123"
        };

        _mockUserRepository
            .Setup(repo => repo.FindUserByEmailAsync(registrationModel.Email))
            .ReturnsAsync(new DeadWalletUser
            {
                Email = "someemail@gmail.com",
                Username = "existingUser",
                FirstName = "John",
                LastName = "Doe",
                Password = "password123"
            });

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _userService.RegisterAsync(registrationModel));
    }

    [Fact]
    public async Task RegisterAsync_NewUser_ReturnsSuccessResult()
    {
        

        var registrationModel = new RegistrationModel
        {
            Email = "someemail@gmail.com",
            Username = "newUser",
            FirstName = "Jane",
            LastName = "Doe",
            Password = "password123"
        };

        _mockUserRepository
            .Setup(repo => repo.FindUserByEmailAsync(registrationModel.Email))
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

        Assert.True(result.Success);
        Assert.NotNull(result);
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
            _mockConfiguration.Object,
            _mockOtpRepository.Object,
            _mockEmailService.Object
        );

        userService.Logout(mockHttpContextAccessor.Object);

        mockCookies.Verify(c => c.Delete("AuthToken"), Times.Once);
    }
    
    [Fact]
    public async Task LoginAsync_UserDoesNotExist_ThrowsException()
    {
        var loginModel = new LoginModel
        {
            Email = "nonExistingUser@gmail.com",
            Password = "password123"
        };
    
        _mockUserRepository
            .Setup(repo => repo.FindUserByEmailAsync(loginModel.Email))
            .ReturnsAsync((DeadWalletUser)null);
    
        await Assert.ThrowsAsync<Exception>(() => _userService.LoginAsync(loginModel));
    }
    
    [Fact]
    public async Task LoginAsync_InvalidPassword_ThrowsException()
    {
        // Arrange
        var loginModel = new LoginModel
        {
            Email = "existingUser@gmail.com",
            Password = "wrongPassword"
        };

        var user = new DeadWalletUser
        {
            Email = "existingUser@gmail.com",
            FirstName = "Jane",
            LastName = "Doe",
            Username = "existingUser",
            Password = "hashedPassword"
        };

        _mockUserRepository
            .Setup(repo => repo.FindUserByEmailAsync(loginModel.Email))
            .ReturnsAsync(user);

        _mockPasswordHasher
            .Setup(hasher => hasher.VerifyHashedPassword(user, user.Password, loginModel.Password))
            .Returns(PasswordVerificationResult.Failed);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _userService.LoginAsync(loginModel));
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsJwtToken()
    {
        // Arrange
        var loginModel = new LoginModel
        {
            Email = "existingUser@gmail.com",
            Password = "correctPassword"
        };

        var user = new DeadWalletUser
        {
            Email = "existingUser@gmail.com",
            FirstName = "Jane",
            LastName = "Doe",
            Username = "existingUser",
            Password = "hashedPassword"
        };

        _mockUserRepository
            .Setup(repo => repo.FindUserByEmailAsync(loginModel.Email))
            .ReturnsAsync(user);

        _mockPasswordHasher
            .Setup(hasher => hasher.VerifyHashedPassword(user, user.Password, loginModel.Password))
            .Returns(PasswordVerificationResult.Success);

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

        // Act
        var result = await _userService.LoginAsync(loginModel);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task VerifyOtpAsync_ValidOtp_ReturnsSuccessAndToken()
    {
        var email = "user@example.com";
        var otpCode = "123456";

        var otp = new EmailOtp
        {
            Email = email,
            Code = otpCode,
            Expiration = DateTime.UtcNow.AddMinutes(5)
        };

        var user = new DeadWalletUser
        {
            Email = email,
            FirstName = "John",
            LastName = "Doe",
            Username = "johndoe",
            Password = "hashedPassword"
        };

        _mockOtpRepository
            .Setup(repo => repo.GetOtpByEmailAsync(email))
            .ReturnsAsync(otp);

        _mockUserRepository
            .Setup(repo => repo.FindUserByEmailAsync(email))
            .ReturnsAsync(user);

        _mockOtpRepository
            .Setup(repo => repo.DeleteOtpAsync(email))
            .Returns(Task.CompletedTask);

        _mockConfiguration.Setup(c => c["Jwt:SecretKey"]).Returns("your-secret-key-with-at-least-32-chars");
        _mockConfiguration.Setup(c => c["Jwt:ExpirationTimeDays"]).Returns("7");
        _mockConfiguration.Setup(c => c["Jwt:Issuer"]).Returns("TestIssuer");
        _mockConfiguration.Setup(c => c["Jwt:Audience"]).Returns("TestAudience");

        var result = await _userService.VerifyOtpAsync(email, otpCode);

        Assert.True(result.Success);
        Assert.NotNull(result.Message);
        Assert.IsType<string>(result.Message);
    }

    [Fact]
    public async Task SendOtpAsync_ValidEmail_SavesOtpAndSendsEmail()
    {
        // Arrange
        var email = "user@example.com";

        _mockOtpRepository
            .Setup(repo => repo.SaveOtpAsync(It.IsAny<EmailOtp>()))
            .Returns(Task.CompletedTask);

        _mockEmailService
            .Setup(service => service.SendOtpAsync(email, It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _userService.sendOtpAsync(email);

        // Assert
        Assert.True(result.Success);
        _mockOtpRepository.Verify(repo => repo.SaveOtpAsync(It.Is<EmailOtp>(o => o.Email == email)), Times.Once);
        _mockEmailService.Verify(service => service.SendOtpAsync(email, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task ResetPasswordAsync_ValidOtpAndUser_ResetsPasswordAndDeletesOtp()
    {
        // Arrange
        var email = "user@example.com";
        var otpCode = "123456";
        var newPassword = "newPassword123";

        var otp = new EmailOtp
        {
            Email = email,
            Code = otpCode,
            Expiration = DateTime.UtcNow.AddMinutes(5)
        };

        var user = new DeadWalletUser
        {
            Email = email,
            Password = "oldPasswordHash",
            FirstName = "John",
            LastName = "Doe",
            Username = "johndoe"    
        };

        _mockOtpRepository.Setup(repo => repo.GetOtpByEmailAsync(email)).ReturnsAsync(otp);
        _mockUserRepository.Setup(repo => repo.FindUserByEmailAsync(email)).ReturnsAsync(user);
        _mockPasswordHasher.Setup(hasher => hasher.HashPassword(user, newPassword)).Returns("newHashedPassword");
        _mockUserRepository.Setup(repo => repo.UpdateUserAsync(user)).Returns(Task.CompletedTask);
        _mockOtpRepository.Setup(repo => repo.DeleteOtpAsync(email)).Returns(Task.CompletedTask);

        // Act
        var result = await _userService.ResetPasswordAsync(email, otpCode, newPassword);

        // Assert
        Assert.True(result.Success);
        _mockUserRepository.Verify(repo => repo.UpdateUserAsync(It.Is<DeadWalletUser>(u => u.Password == "newHashedPassword")), Times.Once);
        _mockOtpRepository.Verify(repo => repo.DeleteOtpAsync(email), Times.Once);
    }
    [Fact]
    public async Task GetAllUsersAsync_ReturnsListOfUsers()
    {
        // Arrange
        var users = new List<DeadWalletUser>
        {
            new DeadWalletUser { Id = 1, Username = "User1", Email = "", LastName = "", FirstName = "", Password = "" },
            new DeadWalletUser { Id = 2, Username = "User2", Email = "", LastName = "", FirstName = "", Password = "" }
        };

        _mockUserRepository
            .Setup(repo => repo.GetAllUsersAsync())
            .ReturnsAsync(users);

        // Act
        var result = await _userService.GetAllUsersAsync();

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Res);
        Assert.Equal(2, result.Res.Count);
    }

    [Fact]
    public async Task DeleteUserByIdAsync_UserIsNotAdmin_ReturnsAccessDenied()
    {
        // Arrange
        var currentUser = new DeadWalletUser
        {
            Id = 2,
            Username = "NonAdmin",
            Email = "",
            LastName = "",
            FirstName = "",
            Password = "",
            Role = "User"
        };

        _mockUserRepository
            .Setup(repo => repo.FindUserByIdAsync(currentUser.Id))
            .ReturnsAsync(currentUser);

        // Act
        var result = await _userService.DeleteUserByIdAsync(1, currentUser.Id);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Access denied. Only admin can perform this action.", result.Message);
    }

    [Fact]
    public async Task DeleteUserByIdAsync_UserIsAdmin_DeletesUserAndReturnsSuccess()
    {
        // Arrange
        var currentUser = new DeadWalletUser
        {
            Id = 1,
            Username = "AdminUser",
            Email = "",
            LastName = "",
            FirstName = "",
            Password = "",
            Role = "Admin"
        };

        _mockUserRepository
            .Setup(repo => repo.FindUserByIdAsync(currentUser.Id))
            .ReturnsAsync(currentUser);

        _mockUserRepository
            .Setup(repo => repo.DeleteUserByIdAsync(2))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _userService.DeleteUserByIdAsync(2, currentUser.Id);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("User deleted successfully", result.Message);
        _mockUserRepository.Verify(repo => repo.DeleteUserByIdAsync(2), Times.Once);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SearchUsers_QueryIsNullOrWhiteSpace_ReturnsError(string query)
    {
        var result = await _userService.SearchUsers(query, userId: 2);

        Assert.False(result.Success);
        Assert.Equal("Search query cannot be empty.", result.Message);
    }

    [Fact]
    public async Task SearchUsers_NoUsersReturned_ReturnsError()
    {
        _mockUserRepository.Setup(r => r.SearchUsersAsync("test")).ReturnsAsync(new List<DeadWalletUser>());

        var result = await _userService.SearchUsers("test", userId: 2);

        Assert.False(result.Success);
        Assert.Equal("No users found matching the search criteria.", result.Message);
    }

    [Fact]
    public async Task SearchUsers_OnlyAdminAndSelfFiltered_ReturnsError()
    {
        var users = new List<DeadWalletUser>
        {
            new DeadWalletUser { Id = 2, Email = "", FirstName = "", LastName = "", Password = "", Username = "" },
            new DeadWalletUser { Id = 1, Email = "", FirstName = "", LastName = "", Password = "", Username = "" }
        };
        _mockUserRepository.Setup(r => r.SearchUsersAsync("test")).ReturnsAsync(users);

        var result = await _userService.SearchUsers("test", userId: 2);

        Assert.True(result.Success);
        Assert.Equal(new List<DeadWalletUser> { }, result.Res);
    }

    [Fact]
    public async Task SearchUsers_ValidUsersReturned_ReturnsSuccess()
    {
        var users = new List<DeadWalletUser>
        {
            new DeadWalletUser { Id = 3, Email = "", FirstName = "", LastName = "", Password = "", Username = "" },
            new DeadWalletUser { Id = 2, Email = "", FirstName = "", LastName = "", Password = "", Username = "" }, // self
            new DeadWalletUser { Id = 1, Email = "", FirstName = "", LastName = "", Password = "", Username = "" }  // admin
        };
        _mockUserRepository.Setup(r => r.SearchUsersAsync("test")).ReturnsAsync(users);

        var result = await _userService.SearchUsers("test", userId: 2);

        Assert.True(result.Success);
        Assert.Single(result.Res);
        Assert.Equal(3, result.Res.First().Id);
    }
}
