using DeadWallet.DAL;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

public class ProfileController : Controller
{
    private readonly DeadWalletContext _context;

    public ProfileController(DeadWalletContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        var user = _context.DeadWalletUsers.Find(userId);

        if (user == null)
            return NotFound();

        var model = new UserProfileViewModel
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Username = user.Username,
            Email = user.Email
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Index(UserProfileViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var userId = GetCurrentUserId();
        if (userId == null || model.Id != userId)
            return Unauthorized();

        var user = _context.DeadWalletUsers.Find(userId);
        if (user == null)
            return NotFound();

        if (!string.Equals(user.Username, model.Username, StringComparison.OrdinalIgnoreCase))
        {
            bool usernameExists = _context.DeadWalletUsers
                .Any(u => u.Username == model.Username && u.Id != userId);

            if (usernameExists)
            {
                ModelState.AddModelError("Username", "Це ім’я користувача вже зайняте.");
                return View(model);
            }
        }

        user.FirstName = model.FirstName;
        user.LastName = model.LastName;
        user.Username = model.Username;

        _context.SaveChanges();

        ViewBag.Message = "Профіль оновлено успішно!";
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
