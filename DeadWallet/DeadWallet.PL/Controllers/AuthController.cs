using DeadWallet.BLL.Services;
using DeadWallet.BLL.Models;
using Microsoft.AspNetCore.Mvc;
using DeadWallet.PL.Models;
using DeadWaller.Controllers;
using Microsoft.AspNetCore.Authorization;

namespace DeadWallet.PL.Controllers
{
    public class AuthController : Controller
    {
        private readonly UserService _userService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(UserService userService, ILogger<AuthController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        public IActionResult Register()
        {
            _logger.LogInformation("User visited registration form");
            return View(new RegistrationViewModel());
        }

        [Authorize]
        public IActionResult Logout()
        {
            _logger.LogInformation("User logout, delete auth token.");

            var httpContextAccessor = new HttpContextAccessor { HttpContext = HttpContext };
            _userService.Logout(httpContextAccessor);

            return RedirectToAction("Index", "Home");
        }



        [HttpPost]
        public async Task<IActionResult> Register(RegistrationViewModel model)
        {
            _logger.LogInformation("User submited registration form");
            try
            {
                if (ModelState.IsValid)
                {
                    var registerModel = new RegistrationModel
                    {
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        Username = model.Username,
                        Password = model.Password
                    };

                    var token = await _userService.RegisterAsync(registerModel);

                    if (token != null)
                    {
                        _logger.LogInformation("User was given a token");
                        Response.Cookies.Append("AuthToken", token, new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = true,
                            SameSite = SameSiteMode.Strict,
                            Expires = DateTime.UtcNow.AddDays(7)
                        });

                        return RedirectToAction("Index", "Home");

                    }

                    ModelState.AddModelError("", "Registration failed.");
                }
                _logger.LogWarning("Submited data was invalid");
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }
    }
}
