using DeadWallet.DAL.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DeadWallet.BLL.Models;
using DeadWallet.DAL.Models;
using DeadWallet.DAL.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using DeadWallet.BLL.Interfaces;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;

namespace DeadWallet.BLL.Services
{
    public class UserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher<DeadWalletUser> _passwordHasher;
        private readonly IConfiguration _configuration;
        private readonly IEmailOtpRepository _otpRepository;
        private readonly IEmailService _emailService;

        public UserService(
            IUserRepository userRepository,
            IPasswordHasher<DeadWalletUser> passwordHasher,
            IConfiguration configuration,
            IEmailOtpRepository otpRepository,
            IEmailService emailService)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _configuration = configuration;
            _otpRepository = otpRepository;
            _emailService = emailService;
        }

        public async Task<Result> RegisterAsync(RegistrationModel model)
        {
            // Check if the user already exists
            var existingUser = await _userRepository.FindUserByEmailAsync(model.Email);
            if (existingUser != null)
            {
                throw new Exception("User already exists");
            }

            // Create a new user
            var user = new DeadWalletUser
            {
                Username = model.Username,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Password = model.Password,
                Email = model.Email
            };

            user.Password = _passwordHasher.HashPassword(user, model.Password);
            await _userRepository.CreateUserAsync(user);

            var otpCode = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
            var otp = new EmailOtp
            {
                Email = user.Email,
                Code = otpCode,
                Expiration = DateTime.UtcNow.AddMinutes(5)
            };

            await _otpRepository.SaveOtpAsync(otp);
            await _emailService.SendOtpAsync(user.Email, otpCode);

            return new Result { Success = true };
        }

        private string GenerateJwtToken(DeadWalletUser user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("Connection string 'DeadWallerContext' not found.")));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            int expTime = 0;

            if (int.TryParse(_configuration["Jwt:ExpirationTimeDays"], out int result))
            {
                expTime = result;
            }
            else
            {
                throw new InvalidOperationException("Expiration time of JWT token is not valid");
            }

            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: DateTime.Now.AddDays(expTime),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public void Logout(IHttpContextAccessor httpContextAccessor)
        {
            httpContextAccessor.HttpContext?.Response.Cookies.Delete("AuthToken");
        }


        public async Task<string> LoginAsync(LoginModel model)
        {
            // Find the user by username
            var user = await _userRepository.FindUserByEmailAsync(model.Email);
            if (user == null)
            {
                throw new Exception("Invalid username or password");
            }

            // Verify the password
            var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(user, user.Password, model.Password);
            if (passwordVerificationResult == PasswordVerificationResult.Failed)
            {
                throw new Exception("Invalid username or password");
            }

            // Generate JWT token
            return GenerateJwtToken(user);
        }

        public async Task<Result> VerifyOtpAsync(string email, string otpCode)
        {
            var otp = await _otpRepository.GetOtpByEmailAsync(email);
            if (otp == null || otp.Expiration < DateTime.UtcNow || otp.Code != otpCode)
                return new Result { Success = false, Message = "Invalid or expired OTP" };

            var user = await _userRepository.FindUserByEmailAsync(email);
            if (user == null)
                return new Result { Success = false, Message = "User not found" };

            await _otpRepository.DeleteOtpAsync(email);

            return new Result { Success = true, Message = GenerateJwtToken(user) };
        }

        public async Task<Result<List<DeadWalletUser>>> GetAllUsersAsync()
        {
            return new Result<List<DeadWalletUser>> { Success = true, Res = await _userRepository.GetAllUsersAsync() };
        }

        public async Task<Result> DeleteUserByIdAsync(int id, int currentUserId)
        {
            var currentUser = await _userRepository.FindUserByIdAsync(currentUserId);

            if (currentUser.Role != "Admin")
            {
                return new Result { Success = false, Message = "Access denied. Only admin can perform this action." };
            }

            await _userRepository.DeleteUserByIdAsync(id);
            return new Result { Success = true, Message = "User deleted successfully" };
        }

        public async Task<Result> sendOtpAsync(string email)
        {
            var otpCode = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
            var otp = new EmailOtp
            {
                Email = email,
                Code = otpCode,
                Expiration = DateTime.UtcNow.AddMinutes(5)
            };

            await _otpRepository.SaveOtpAsync(otp);
            await _emailService.SendOtpAsync(email, otpCode);

            return new Result { Success = true };
        }

        public async Task<Result> ResetPasswordAsync(string email, string otpCode, string newPassword)
        {
            var otp = await _otpRepository.GetOtpByEmailAsync(email);
            if (otp == null || otp.Expiration < DateTime.UtcNow || otp.Code != otpCode)
                return new Result { Success = false, Message = "Invalid or expired OTP" };

            var user = await _userRepository.FindUserByEmailAsync(email);
            if (user == null)
                return new Result { Success = false, Message = "User not found" };

            user.Password = _passwordHasher.HashPassword(user, newPassword);
            await _userRepository.UpdateUserAsync(user);
            await _otpRepository.DeleteOtpAsync(email);

            return new Result { Success = true };
        }

        public async Task<Result<List<DeadWalletUser>>> SearchUsers(string query, int userId)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return new Result<List<DeadWalletUser>>
                {
                    Success = false,
                    Message = "Search query cannot be empty."
                };
            }

            var users = await _userRepository.SearchUsersAsync(query);

            if (users == null || !users.Any())
            {
                return new Result<List<DeadWalletUser>>
                {
                    Success = false,
                    Message = "No users found matching the search criteria."
                };
            }

            users.RemoveAll(u => u.Id == userId || u.Id == 1);

            return new Result<List<DeadWalletUser>>
            {
                Success = true,
                Res = users
            };
        }

        public async Task<Result<DeadWalletUser>> GetUserByIdAsync(int id)
        {
            var user = await _userRepository.FindUserByIdAsync(id);

            if (user == null)
            {
                return new Result<DeadWalletUser> { Success = false, Message = "User not found" };
            }

            return new Result<DeadWalletUser> { Success = true, Res = user };
        }


        public async Task<Result> UpdateUserAsync(DeadWalletUser user)
        {
            var existingUser = await _userRepository.FindUserByUsernameAsync(user.Username);
            if (existingUser != null && existingUser.Id != user.Id)
            {
                return new Result
                {
                    Success = false,
                    Message = "This username is already taken."
                };
            }

            await _userRepository.UpdateUserAsync(user);
            return new Result { Success = true };
        }

    }
}

