using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeadWallet.PL.Controllers
{
    public class AdminController : Controller
    {
        [Authorize(Policy = "AdminOnly")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
