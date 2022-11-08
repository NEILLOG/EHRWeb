using BASE.Areas.Backend.Models;
using BASE.Areas.Backend.Models.Extend;
using BASE.Extensions;
using BASE.Models;
using BASE.Models.DB;
using BASE.Service;
using BASE.Service.Base;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace BASE.Areas.Backend.Service
{
    public class CommonService : ServiceBase
    {
        private readonly AllCommonService _allCommonService;
        private readonly IConfiguration _conf;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IActionContextAccessor _actionAccessor;

        private string _Message = string.Empty;

        public CommonService(DBContext context,
                           AllCommonService allCommonService,
                           IConfiguration configuration,
                           IHttpContextAccessor contextAccessor,
                           IActionContextAccessor actionAccessor) : base(context)
        {
            _allCommonService = allCommonService;
            _actionAccessor = actionAccessor;
            _contextAccessor = contextAccessor;
            _conf = configuration;
        }

        /// <summary>
        /// 後台選單 
        /// </summary>
        /// <returns></returns>
        public async Task<List<TreeMenu>> getMenu()
        {
            UserSessionModel userInfo = _contextAccessor.HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);

            List<TreeMenu> MenuModel = new List<TreeMenu>();
            List<TbMenuBack> all_Menus = new List<TbMenuBack>();

            //確認權限選單
            List<string> MenuIDlist = await Lookup<TbUserRight>(ref _Message).Where(x => x.UserId == userInfo.UserID && x.Enabled).Select(x => x.MenuId).ToListAsync();
            //取出選單(過濾掉刪除的)
            List<TbMenuBack> MenuList = await Lookup<TbMenuBack>(ref _Message, x => x.IsDelete == false).Where(x => MenuIDlist.Contains(x.MenuId)).OrderBy(x => x.MenuOrder).ToListAsync();
            //確定有幾層選單
            string route = string.Empty;
            string area = _actionAccessor.ActionContext.RouteData.Values["area"].ToString();
            string controller = _actionAccessor.ActionContext.RouteData.Values["controller"].ToString();
            string action = _actionAccessor.ActionContext.RouteData.Values["action"].ToString();
            route = "/" + area + "/" + controller + "/" + action;

            foreach (TbMenuBack menu in MenuList)
            {
                MenuModel.Add(new TreeMenu { MenuID = menu.MenuId, ParentID = menu.MenuParent, Level = menu.MenuLevel, Icon = menu.Icon, Title = menu.MenuName, Url = menu.MenuUrl, Tag = menu.TagName, Selected = menu.MenuUrl.Contains(route) ? true : false, ShowInMenuSidebar = menu.ShowInMainSideBar });
            }

            List<TreeMenu> selected_menus = MenuModel.Where(x => x.Selected).ToList();

            foreach (TreeMenu selected_menu in selected_menus)
            {
                TreeMenu target = selected_menu;

                do
                {
                    target.MenuOpen = true;
                    target = MenuModel.Where(x => x.MenuID == target.ParentID && x.Level > 1).FirstOrDefault();
                }
                while (target != null);
            }

            //回傳拼湊完的menu資料
            return MenuModel.Where(x => x.ShowInMenuSidebar).OrderBy(x => x.Level).ToList();
        }

        /// <summary>
        /// 後台麵包屑 
        /// </summary>
        /// <returns></returns>
        public async Task<List<TreeMenu>> getBreadcrumb()
        {
            List<TreeMenu> finalroute = new List<TreeMenu>();
            string route = string.Empty;
            string area = _actionAccessor.ActionContext.RouteData.Values["area"].ToString();
            string controller = _actionAccessor.ActionContext.RouteData.Values["controller"].ToString();
            string action = _actionAccessor.ActionContext.RouteData.Values["action"].ToString();
            route = "/" + area + "/" + controller + "/" + action;
            TbMenuBack currentroute = await Lookup<TbMenuBack>(ref _Message, x => x.IsDelete == false && x.MenuUrl == route).SingleOrDefaultAsync();
            while (currentroute != null)
            {
                finalroute.Add(new TreeMenu { Url = currentroute.MenuUrl, Title = currentroute.MenuName, ShowInMenuSidebar = currentroute.ShowInMainSideBar, Level = currentroute.MenuLevel });
                currentroute = await Lookup<TbMenuBack>(ref _Message, x => x.IsDelete == false && x.MenuId == currentroute.MenuParent && x.MenuLevel != currentroute.MenuLevel).SingleOrDefaultAsync();
            }

            //回傳拼湊完的麵包屑資料
            return finalroute;
        }

        /// <summary>
        /// 取得當前選單
        /// </summary>
        /// <returns></returns>
        public async Task<TbMenuBack> getCurrentMenu()
        {

            string route = string.Empty;
            string area = _actionAccessor.ActionContext.RouteData.Values["area"].ToString();
            string controller = _actionAccessor.ActionContext.RouteData.Values["controller"].ToString();
            string action = _actionAccessor.ActionContext.RouteData.Values["action"].ToString();
            route = "/" + area + "/" + controller + "/" + action;
            TbMenuBack currentroute = await Lookup<TbMenuBack>(ref _Message, x => x.IsDelete == false && x.MenuUrl == route).SingleOrDefaultAsync();

            //回傳拼湊完的麵包屑資料
            return currentroute;
        }

        
        /// <summary>操作紀錄</summary>
        /// <param name="request">請求JSON</param>
        /// <param name="response">回傳JSON</param>
        /// <param name="route_path">API_URL</param>
        /// <returns>新ID</returns>
        public async Task<bool> OperateLog<TRequest, TResponse>(string UserID, string Feature, string Action, string? id = null, TRequest? T_request = null, string? Message = null, TResponse? T_response = null, bool IsSuccess = true) where TRequest : class
                                                                                                                                                                                                    where TResponse : class
        {
            ActionResultModel<TbBackendOperateLog> result = new ActionResultModel<TbBackendOperateLog>();

            TbBackendOperateLog log = new TbBackendOperateLog()
            {
                Request = JsonSerializer.Serialize(T_request, new JsonSerializerOptions()
                {
                    ReferenceHandler = ReferenceHandler.IgnoreCycles,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                }),
                Response = JsonSerializer.Serialize(T_response, new JsonSerializerOptions()
                {
                    ReferenceHandler = ReferenceHandler.IgnoreCycles,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                }),
                Ip = _allCommonService.GetIPAddress_IPv4(),
                Action = Action,
                Message = Message,
                Url = _allCommonService.GetAbsoluteUrl(),
                UserAgent = _allCommonService.GetUserAgent(),
                CreateDate = DateTime.Now,
                Feature = Feature,
                UserId = UserID,
                IsSuccess = IsSuccess,
                DataKey = id,
            };

            if (_conf.GetValue<bool>("Site:BackendOperateLog"))
            {
                result = await base.Insert(log);
            }

            return result.IsSuccess;
        }

        public async Task<bool> OperateLog<TRequest>(string UserID, string Feature, string Action, string? id = null, TRequest? T_request = null, string? Message = null, string? response = null, bool IsSuccess = true) where TRequest : class
        {
            ActionResultModel<TbBackendOperateLog> result = new ActionResultModel<TbBackendOperateLog>();

            TbBackendOperateLog log = new TbBackendOperateLog()
            {
                Request = JsonSerializer.Serialize(T_request, new JsonSerializerOptions()
                {
                    ReferenceHandler = ReferenceHandler.IgnoreCycles,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                }),
                Response = response,
                Ip = _allCommonService.GetIPAddress_IPv4(),
                Action = Action,
                Message = Message,
                Url = _allCommonService.GetAbsoluteUrl(),
                UserAgent = _allCommonService.GetUserAgent(),
                CreateDate = DateTime.Now,
                Feature = Feature,
                UserId = UserID,
                IsSuccess = IsSuccess,
                DataKey = id,
            };

            if (_conf.GetValue<bool>("Site:BackendOperateLog"))
            {
                result = await base.Insert(log);
            }

            return result.IsSuccess;
        }

        public async Task<bool> OperateLog<TResponse>(string UserID, string Feature, string Action, string? id = null, string? request = null, string? Message = null, TResponse? T_response = null, bool IsSuccess = true) where TResponse : class
        {
            ActionResultModel<TbBackendOperateLog> result = new ActionResultModel<TbBackendOperateLog>();

            TbBackendOperateLog log = new TbBackendOperateLog()
            {
                Request = request == null ? null : request.ToString(),
                Response = JsonSerializer.Serialize(T_response, new JsonSerializerOptions()
                {
                    ReferenceHandler = ReferenceHandler.IgnoreCycles,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                }),
                Ip = _allCommonService.GetIPAddress_IPv4(),
                Action = Action,
                Message = Message,
                Url = _allCommonService.GetAbsoluteUrl(),
                UserAgent = _allCommonService.GetUserAgent(),
                CreateDate = DateTime.Now,
                Feature = Feature,
                UserId = UserID,
                IsSuccess = IsSuccess,
                DataKey = id,
            };

            if (_conf.GetValue<bool>("Site:BackendOperateLog"))
            {
                result = await base.Insert(log);
            }

            return result.IsSuccess;
        }

        public async Task<bool> OperateLog(string UserID, string Feature, string Action, string? id = null, string? request = null, string? Message = null, string? response = null, bool IsSuccess = true)
        {
            ActionResultModel<TbBackendOperateLog> result = new ActionResultModel<TbBackendOperateLog>();

            TbBackendOperateLog log = new TbBackendOperateLog()
            {
                Request = request,
                Response = response,
                Ip = _allCommonService.GetIPAddress_IPv4(),
                Action = Action,
                Message = Message,
                Url = _allCommonService.GetAbsoluteUrl(),
                UserAgent = _allCommonService.GetUserAgent(),
                CreateDate = DateTime.Now,
                Feature = Feature,
                UserId = UserID,
                IsSuccess = IsSuccess,
                DataKey = id,
            };
            if (_conf.GetValue<bool>("Site:BackendOperateLog"))
            {
                result = await base.Insert(log);
            }

            return result.IsSuccess;
        }


    }
}
