using DeadWallet.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DeadWallet.DAL.Models;

namespace DeadWallet.PL.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class AdminController : Controller
    {
        private readonly ITagService _tagService;
        private readonly IUserService _userService;

        public AdminController(ITagService tagService, IUserService userService)
        {
            _tagService = tagService;
            _userService = userService;
        }

        public async Task<IActionResult> Index()
        {
            var tags = await _tagService.GetAllTagsAsync();
            var currentUser = await GetCurrentUserAsync();
            var users = await _userService.GetAllUsersAsync(currentUser);
            ViewBag.Users = users;
            return View(tags);
        }


        public async Task<IActionResult> Tags()
        {
            var tags = await _tagService.GetAllTagsAsync();
            return PartialView("_TagsPartial", tags);
        }

        public async Task<IActionResult> Users()
        {
            var currentUser = await GetCurrentUserAsync(); // Реалізуй метод витягування юзера з токена
            var users = await _userService.GetAllUsersAsync(currentUser);
            return PartialView("_UsersPartial", users);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var currentUser = await GetCurrentUserAsync();
            await _userService.DeleteUserByIdAsync(id, currentUser);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteTag(int id)
        {
            await _tagService.DeleteTagAsync(id);
            return RedirectToAction("Index");
        }
    }
}
