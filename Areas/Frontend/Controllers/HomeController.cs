using Microsoft.AspNetCore.Mvc;

namespace BASE.Areas.Frontend.Controllers
{
    public class HomeController : Controller
    {
        [Area("Frontend")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
