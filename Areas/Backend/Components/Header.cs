using BASE.Areas.Backend.Models;
using Microsoft.AspNetCore.Mvc;
using BASE.Service;
using BASE.Models.DB;
using BASE.Extensions;
using Microsoft.EntityFrameworkCore;
using BASE.Areas.Backend.Service;


namespace BASE.Areas.Backend.Components
{
    public class HeaderViewComponent : ViewComponent
    {
        private readonly DBContext _dBContext;
        private readonly AllCommonService _allCommonService;
        private readonly CommonService _commonService;
        private readonly UserService _userService;

        private string _Message = string.Empty;

        public HeaderViewComponent(DBContext context,
                                   AllCommonService allCommonService,
                                   CommonService commonService,
                                   UserService userService)
        {
            _dBContext = context;
            _allCommonService = allCommonService;
            _commonService = commonService;
            _userService = userService;
        }


        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
