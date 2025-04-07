using DeadWallet.BLL.Interfaces;
using DeadWallet.DAL;
using DeadWallet.DAL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging; 
using System.Threading.Tasks;

namespace DeadWallet.PL.Controllers
{
    public class TagController : Controller
    {
        private readonly ITagService _tagService;
        private readonly DeadWalletContext _context;
        private readonly ILogger<TagController> _logger;

        public TagController(ITagService tagService,
                             DeadWalletContext context,
                             ILogger<TagController> logger)
        {
            _tagService = tagService;
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Entering Index method.");
            var tags = await _tagService.GetAllTagsAsync();
            return View(tags);
        }

        public async Task<IActionResult> AddTag(string name, string color)
        {
            _logger.LogInformation($"AddTag called with name: {name} and color: {color}");
            if (!string.IsNullOrEmpty(name))
            {
                await _tagService.AddTagAsync(new Tag { Name = name, Color = color });
                _logger.LogInformation($"Tag added with name: {name} and color: {color}");
            }
            else
            {
                _logger.LogWarning("Tag name is empty, skipping add.");
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> DeleteTag(int id)
        {
            _logger.LogInformation($"DeleteTag called for tag with id: {id}");
            await _tagService.DeleteTagAsync(id);
            _logger.LogInformation($"Tag with id {id} deleted.");
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            _logger.LogInformation($"Edit method called for tag with id: {id}");
            var tag = _context.Tags.FirstOrDefault(t => t.Id == id);
            if (tag == null)
            {
                _logger.LogWarning($"Tag with id {id} not found.");
                return NotFound();
            }
            _logger.LogInformation($"Tag with id {id} found: Name = {tag.Name}, Color = {tag.Color}");
            return View(tag);
        }

        [HttpPost]
        public IActionResult Edit(Tag tag)
        {
            _logger.LogInformation($"Edit POST method called for tag with id: {tag.Id}");
            if (ModelState.IsValid)
            {
                _logger.LogInformation($"Updating tag with id {tag.Id}");
                _context.Tags.Update(tag);
                _context.SaveChanges();
                _logger.LogInformation($"Tag with id {tag.Id} updated successfully.");
                return RedirectToAction("Index");
            }

            _logger.LogWarning($"ModelState is invalid for tag with id {tag.Id}. Returning to Edit view.");
            return View(tag);
        }

        public IActionResult ManageTags()
        {
            _logger.LogInformation("ManageTags method called.");
            var tags = _context.Tags.ToList();
            _logger.LogInformation($"Found {tags.Count} tags for management.");
            return View(tags);
        }
    }
}
