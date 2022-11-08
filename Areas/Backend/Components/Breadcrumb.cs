using BASE.Areas.Backend.Models;
using Microsoft.AspNetCore.Mvc;
using BASE.Service;
using BASE.Models.DB;
using BASE.Extensions;
using Microsoft.EntityFrameworkCore;
using BASE.Areas.Backend.Service;


namespace BASE.Areas.Backend.Components
{
    public class BreadcrumbViewComponent : ViewComponent
    {
        private readonly DBContext _dBContext;
        private readonly AllCommonService _allCommonService;
        private readonly CommonService _commonService;
        private readonly UserService _userService;

        private string _Message = string.Empty;

        public BreadcrumbViewComponent(DBContext context,
                                   AllCommonService allCommonService,
                                   CommonService commonService,
                                   UserService userService)
        {
            _dBContext = context;
            _allCommonService = allCommonService;
            _commonService = commonService;
            _userService = userService;
        }


        public async Task<IViewComponentResult> InvokeAsync(string test)
        {
            var data = await GetBreadcrumbAsync();

            return View(data);
        }

        private async Task<VM_Breadcrumb> GetBreadcrumbAsync()
        {
            VM_Breadcrumb data = new VM_Breadcrumb();
            data.BreadcrumbList = await _commonService.getBreadcrumb();
            data.currentmenu = await _commonService.getCurrentMenu();

            // 預設 頁首標題
            HttpContext.Items["Title"] = ViewData["Title"] ?? data.currentmenu.MenuName;

            return data;
        }
    }
}
