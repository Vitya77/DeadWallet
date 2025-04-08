using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeadWallet.PL.Controllers
{
    public class AdminController : Controller
    {
        private readonly ITagService _tagService;

        public AdminController(ITagService tagService)
        {
            _tagService = tagService;
        }
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Tags()
        {
            var tags = await _tagService.GetAllTagsAsync();
            return PartialView("_TagsPartial", tags);
        }

        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeleteTag(int id)
        {
            await _tagService.DeleteTagAsync(id);
            return RedirectToAction("Tags");
        }

        [Authorize(Policy = "AdminOnly")]
        public IActionResult Index()
        {
            return View();
        }
    }

}
