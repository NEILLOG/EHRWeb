using BASE.Extensions;
using BASE.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BASE.Areas.Frontend.Controllers
{
    [Area("Frontend")]
    public class BaseController : Controller
    {
        public string _message = String.Empty;
        public string Platform = "前台";
    }
}
