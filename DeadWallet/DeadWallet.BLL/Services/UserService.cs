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

namespace DeadWallet.BLL.Services
{
    public class UserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher<DeadWalletUser> _passwordHasher;
        private readonly IConfiguration _configuration;

        public UserService(IUserRepository userRepository, IPasswordHasher<DeadWalletUser> passwordHasher, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _configuration = configuration;
        }

        public async Task<string> RegisterAsync(RegistrationModel model)
        {
            // Check if the user already exists
            var existingUser = await _userRepository.FindUserByUsernameAsync(model.Username);
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
                Password = model.Password
            };

            user.Password = _passwordHasher.HashPassword(user, model.Password);

            await _userRepository.CreateUserAsync(user);

            // Generate JWT token
            return GenerateJwtToken(user);
        }

        private string GenerateJwtToken(DeadWalletUser user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
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
        
        
        public async Task<string> LoginAsync(LoginModel model)
        {
            // Find the user by username
            var user = await _userRepository.FindUserByUsernameAsync(model.Username);
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
    }
}
