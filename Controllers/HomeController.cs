using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OrderItApp.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        // Landing page for authenticated users. Keep thin: forward to Orders index.
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Orders");
        }
    }
}
