using DeadWallet.BLL.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Linq;
using System.Threading.Tasks;
using DeadWallet.BLL.Models;

public class ProfileController : Controller
{
    private readonly UserService _userService;

    public ProfileController(UserService userService)
    {
        _userService = userService;
    }

    public async Task<IActionResult> Index()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        var result = await _userService.GetUserByIdAsync(userId.Value);
        if (!result.Success || result.Res == null)
            return NotFound();

        var model = new UserProfileViewModel
        {
            Id = result.Res.Id,
            FirstName = result.Res.FirstName,
            LastName = result.Res.LastName,
            Username = result.Res.Username,
            Email = result.Res.Email
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(UserProfileViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var userId = GetCurrentUserId();
        if (userId == null || model.Id != userId)
            return Unauthorized();

        var result = await _userService.GetUserByIdAsync(userId.Value);
        if (!result.Success || result.Res == null)
            return NotFound();

        var user = result.Res;

        if (!string.Equals(user.Username, model.Username, System.StringComparison.OrdinalIgnoreCase))
        {
            var allUsers = await _userService.GetAllUsersAsync();
            if (!allUsers.Success || allUsers.Res == null)
                return StatusCode(500, "User list failed to load.");

            bool usernameExists = allUsers.Res.Any(u => u.Username == model.Username && u.Id != userId);
            if (usernameExists)
            {
                ModelState.AddModelError("Username", "This username is already taken.");
                return View(model);
            }
        }

        user.FirstName = model.FirstName;
        user.LastName = model.LastName;
        user.Username = model.Username;

        await _userService.UpdateUserAsync(user);

        ViewBag.Message = "Profile updated successfully!";
        return View(model);
    }

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
            return userId;
        return null;
    }
}
