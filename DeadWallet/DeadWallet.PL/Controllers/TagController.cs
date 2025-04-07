using DeadWallet.BLL.Interfaces;
using DeadWallet.DAL;
using DeadWallet.DAL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace DeadWallet.PL.Controllers
{
    public class TagController : Controller
    {
        private readonly ITagService _tagService;
        private readonly DeadWalletContext _context;


        public TagController(ITagService tagService, 
                             DeadWalletContext context)
        {
            _tagService = tagService;
            _context = context;

        }

        public async Task<IActionResult> Index()
        {
            var tags = await _tagService.GetAllTagsAsync();
            return View(tags);
        }

        public async Task<IActionResult> AddTag(string name, string color)
        {
            if (!string.IsNullOrEmpty(name))
            {
                await _tagService.AddTagAsync(new Tag { Name = name, Color = color });
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> DeleteTag(int id)
        {
            await _tagService.DeleteTagAsync(id);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var tag = _context.Tags.FirstOrDefault(t => t.Id == id);
            if (tag == null)
                return NotFound();

            return View("Edit", tag);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Tag tag)
        {
            if (ModelState.IsValid)
            {
                _context.Tags.Update(tag);
                _context.SaveChanges();
                return RedirectToAction("ManageTags");
            }

            return View(tag);
        }

        public IActionResult ManageTags()
        {
            var tags = _context.Tags.ToList();
            return View(tags);
        }
    }
}