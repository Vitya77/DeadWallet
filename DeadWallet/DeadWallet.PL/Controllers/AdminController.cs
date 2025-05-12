using DeadWallet.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DeadWallet.DAL.Models;
using DeadWallet.BLL.Services;
using System.Security.Claims;
using DeadWallet.PL.Models;

namespace DeadWallet.PL.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class AdminController : Controller
    {
        private readonly ITagService _tagService;
        private readonly UserService _userService;
        private readonly ITransactionService _transactionService;

        public AdminController(ITagService tagService, UserService userService, ITransactionService transactionService)
        {
            _tagService = tagService;
            _userService = userService;
            _transactionService = transactionService;
        }

        public async Task<IActionResult> Transactions()
        {
            var transactions = await _transactionService.GetAllTransactionsAsync();
            return PartialView("_TransactionsPartial", transactions);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteTransaction(int id)
        {
            await _transactionService.DeleteTransactionAsync(id);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Index()
        {
            var transactions = await _transactionService.GetAllTransactionsAsync();
            var tags = await _tagService.GetAllTagsAsync();
            var users = await _userService.GetAllUsersAsync();

            return View(new AdminViewModel { tags = tags, users = users.Res, transactions = transactions.Res });
        }

        public async Task<IActionResult> Tags()
        {
            var tags = await _tagService.GetAllTagsAsync();
            return PartialView("_TagsPartial", tags);
        }

        public async Task<IActionResult> Users()
        {
            var users = await _userService.GetAllUsersAsync();
            return PartialView("_UsersPartial", users);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Redirect("/Home");
            }

            int parsedUserId;
            if (!int.TryParse(userId, out parsedUserId))
            {
                return Redirect("/Home");
            }

            await _userService.DeleteUserByIdAsync(id, parsedUserId);
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
