using BASE.Areas.Backend.Models;
using Microsoft.AspNetCore.Mvc;
using BASE.Service;
using BASE.Models.DB;
using BASE.Extensions;
using Microsoft.EntityFrameworkCore;
using BASE.Areas.Backend.Service;


namespace BASE.Areas.Backend.Components
{
    public class MenuSidebarViewComponent : ViewComponent
    {
        private readonly DBContext _dBContext;
        private readonly AllCommonService _allCommonService;
        private readonly CommonService _commonService;
        private readonly UserService _userService;

        private string _Message = string.Empty;

        public MenuSidebarViewComponent(DBContext context,
                                        AllCommonService allCommonService,
                                        CommonService commonService,
                                        UserService userService)
        {
            _dBContext = context;
            _allCommonService = allCommonService;
            _commonService = commonService;
            _userService = userService;
        }


        public async Task<IViewComponentResult> InvokeAsync()
        {
            var data = await GetMenuAsync();

            return View(data);
        }
        private async Task<VM_Menu> GetMenuAsync()
        {
            VM_Menu data = new VM_Menu();
            //使用者資訊
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);

            if (userinfo != null)
            {
                data.UserExtendItem = await _userService.getUserExtend(x => x.User.UserId == userinfo.UserID).SingleOrDefaultAsync();
            }

            data.MenuList = await _commonService.getMenu();

            return data;
        }
    }
}
