using DeadWallet.BLL.Services;
using DeadWallet.BLL.Models;
using Microsoft.AspNetCore.Mvc;
using DeadWallet.PL.Models;
using DeadWaller.Controllers;
using Microsoft.AspNetCore.Authorization;
using NuGet.Common;

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

        public IActionResult OtpConfirm()
        {
            _logger.LogInformation("User visited OTP verification form");
            return View(new OtpViewModel());
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
                        Email = model.Email,
                        Password = model.Password
                    };

                    var result = await _userService.RegisterAsync(registerModel);

                    if (result.Success) 
                    {
                        Response.Cookies.Append("UserEmail", registerModel.Email, new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = true,
                            SameSite = SameSiteMode.Strict,
                            Expires = DateTime.UtcNow.AddDays(1)
                        });
                        return RedirectToAction("OtpConfirm", "Auth");
                    }

                    ModelState.AddModelError("", "Registration failed.");
                }
                _logger.LogWarning("Submitted data was invalid");
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> OtpConfirm(OtpViewModel model)
        {
            _logger.LogInformation("User submitted otp form");
            try
            {
                if (ModelState.IsValid)
                {
                    var email = Request.Cookies["UserEmail"];
                    if (email == null)
                    {
                        _logger.LogInformation("Not found email cookie");
                        return View(model);
                    }

                    var result = await _userService.VerifyOtpAsync(email, model.Otp);

                    if (result.Success)
                    {
                        Response.Cookies.Append("AuthToken", result.Message, new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = true,
                            SameSite = SameSiteMode.Strict,
                            Expires = DateTime.UtcNow.AddDays(7)
                        });

                        return RedirectToAction("Index", "Home");
                    }

                    ModelState.AddModelError("", "Otp code is not valid.");
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

        [HttpGet]
        public IActionResult Login()
        {
            _logger.LogInformation("User visited login form");
            return View(new LoginViewModel());
        }
        
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            _logger.LogInformation("User submitted login form");
            try
            {
                if (ModelState.IsValid)
                {
                    var loginModel = new LoginModel
                    {
                        Email = model.Email,
                        Password = model.Password
                    };

                    var token = await _userService.LoginAsync(loginModel);

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

                    ModelState.AddModelError("", "Login failed.");
                }

                _logger.LogWarning("Submitted data was invalid");
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
