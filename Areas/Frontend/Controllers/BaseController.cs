using Microsoft.AspNetCore.Mvc;

namespace BASE.Areas.Frontend.Controllers
{
    [Area("Frontend")]
    public class BaseController : Controller
    {
        public string _message = String.Empty;
        public string Platform = "前台";
    }
}
