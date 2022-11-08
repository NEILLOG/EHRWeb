using Microsoft.AspNetCore.Mvc;

namespace BASE.Areas.Backend.Controllers
{
    [Area("Backend")]
    public class BaseController : Controller
    {
        public string _message = String.Empty;
        public string Platform = "後台";
    }
}
