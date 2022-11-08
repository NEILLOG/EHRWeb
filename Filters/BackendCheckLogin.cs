using BASE.Areas.Backend.Models;
using BASE.Areas.Backend.Models.Extend;
using BASE.Areas.Backend.Service;
using BASE.Models.DB;
using Microsoft.AspNetCore.Mvc.Filters;
using BASE.Extensions;
using BASE.Models.Enums;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BASE.Filters
{
    /// Asynchronous filters work a bit differently: first execute code that must be executed before the action, call next() for the actual logic, finally add code to be executed after the action.
    /// 非同步 filter 會先執行進入 action 前的程式碼（await next() 前），
    /// 再執行 await next()，即進入 Action
    /// Action 跑完後（使用者還沒看到 View），再執行 await next() 後的程式碼
    public class BackendCheckLoginAttribute : TypeFilterAttribute
    {
        public BackendCheckLoginAttribute(params string?[] parameters) : base(typeof(BackendCheckLoginImpl))
        {
            Arguments = new object[] { parameters };
        }

        private class BackendCheckLoginImpl : IAsyncActionFilter
        {
            private readonly IHttpContextAccessor _contextAccessor;
            private readonly AuthorityService _authorityService;
            private readonly string?[] _parameters;

            public BackendCheckLoginImpl(AuthorityService authorityService, IHttpContextAccessor contextAccessor, string?[] parameters)
            {
                _authorityService = authorityService;
                _contextAccessor = contextAccessor;
                _parameters = parameters;
            }

            /// [檢查Session]  使用者登入資訊 Session 是否存在，及判斷目錄權限
            /// <para>不存在，則導頁至登入畫面</para>
            /// </summary>
            /// Example 判斷Session和目錄權限: [BackendCheckLogin(MenuID = "TbMenuBack.MenuID", Type = "檢驗類別")]
            /// Example 僅判斷Session: [BackendCheckLogin]
            /// 檢驗類別請參閱下方Type(檢驗類別)參數註解
            public async Task OnActionExecutionAsync(ActionExecutingContext filterContext, ActionExecutionDelegate next)
            {

                #region 進入 action 前行為

                /* 功能ID */
                string? MenuID = _parameters.ElementAtOrDefault(0) ?? null;

                /* 檢驗類別（啟用: ENABLED, 檢視: VIEW, 新增: ADD, 編輯: MODIFY, 刪除: DELETE, 下載: DOWNLOAD, 上傳: UPLOAD） */
                string? Type = _parameters.ElementAtOrDefault(1) ?? null;

                UserSessionModel? userInfo = filterContext.HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);

                // [檢查] 沒登入Session 
                if (userInfo == null)
                {
                    this.RedirectLogin(filterContext);
                    return;
                }
                else
                {
                    string message = string.Empty;
                    RouteValueDictionary Backend_Homepage = new RouteValueDictionary
                    {
                        { "area", "Backend" },
                        { "controller", "Home" },
                        { "action", "Index" }
                    };

                    Controller controller = (Controller)filterContext.Controller;

                    if (!string.IsNullOrEmpty(MenuID) && !string.IsNullOrEmpty(Type))
                    {
                        AuthorityExtend auth = await _authorityService.GetRight(userInfo.UserID, MenuID);
                        TbMenuBack? menu = await _authorityService.Lookup<TbMenuBack>(ref message, x => !x.IsDelete && x.MenuId == MenuID).SingleOrDefaultAsync();

                        List<string> typeList = Type.Split(',').ToList();

                        bool pass = false;

                        if (auth == null)
                        {
                            controller.TempData["TempMsgType"] = MsgTypeEnum.info;
                            controller.TempData["TempMsg"] = "您尚無權限";
                            //filterContext.Controller.TempData["TempMsgType"] = MsgTypeEnum.info;
                            //filterContext.Controller.TempData["TempMsg"] = "您尚無權限！";

                            filterContext.Result = new RedirectToRouteResult(Backend_Homepage);
                        }
                        else
                        {

                            foreach (string type in typeList)
                            {
                                if (!auth.Enabled)
                                {
                                    pass = false;
                                    break;
                                }

                                switch (type.ToUpper())
                                {
                                    case "ENABLED":
                                        if (auth.View_Enabled)
                                        {
                                            pass = true;
                                        }
                                        break;
                                    case "VIEW":
                                        if (auth.View_Enabled)
                                        {
                                            pass = true;
                                        }
                                        break;
                                    case "ADD":
                                        if (auth.Add_Enabled)
                                        {
                                            pass = true;
                                        }
                                        break;
                                    case "MODIFY":
                                        if (auth.Modify_Enabled)
                                        {
                                            pass = true;
                                        }
                                        break;
                                    case "DELETE":
                                        if (auth.Delete_Enabled)
                                        {
                                            pass = true;
                                        }
                                        break;
                                    case "UPLOAD":
                                        if (auth.Upload_Enabled)
                                        {
                                            pass = true;
                                        }
                                        break;
                                    case "DOWNLOAD":
                                        if (auth.Download_Enabled)
                                        {
                                            pass = true;
                                        }
                                        break;
                                    default:
                                        //filterContext.Controller.TempData["TempMsgType"] = MsgTypeEnum.error;
                                        //filterContext.Controller.TempData["TempMsg"] = "傳入參數錯誤！";

                                        controller.TempData["TempMsgType"] = MsgTypeEnum.error;
                                        controller.TempData["TempMsg"] = "傳入參數錯誤！";

                                        filterContext.Result = new RedirectToRouteResult(Backend_Homepage);
                                        break;
                                }

                                if (pass)
                                {
                                    break;
                                }

                            }

                            if (!pass)
                            {
                                //filterContext.Controller.TempData["TempMsgType"] = MsgTypeEnum.info;
                                //filterContext.Controller.TempData["TempMsg"] = "您尚無權限！";

                                controller.TempData["TempMsgType"] = MsgTypeEnum.info;
                                controller.TempData["TempMsg"] = "您尚無權限！";

                                if (filterContext.HttpContext.Request.IsAjaxRequest())//是Ajax的話
                                {
                                    filterContext.HttpContext.Response.StatusCode = 400;//
                                    filterContext.HttpContext.Response.Headers.Add("error", "no auth");//設定無此權限
                                    if (menu == null || !auth.Enabled)
                                    {
                                        //filterContext.HttpContext.Response.Headers.Add("redirect_url", Backend_Homepage);//設定無此權限
                                    }
                                    else
                                    {
                                        filterContext.HttpContext.Response.Headers.Add("redirect_url", menu.MenuUrl);//設定無此權限
                                    }

                                    //filterContext.Result = new RedirectResult("");//不改導向頁面，會繼續執行AJAX
                                    filterContext.HttpContext.Response.Redirect("");
                                }
                                else
                                {
                                    if (menu == null || !auth.Enabled)
                                    {
                                        filterContext.Result = new RedirectToRouteResult(Backend_Homepage);
                                    }
                                    else
                                    {
                                        filterContext.HttpContext.Response.Redirect(menu.MenuUrl);
                                    }
                                }

                                //await filterContext.Result.ExecuteResultAsync(filterContext);

                            }
                        }


                    }
                }

                #endregion

                await next(); // 進入 action

                #region 進入 action 後、使用者看到畫面前行為

                #endregion
            }

            //public void OnActionExecuted(ActionExecutedContext context)
            //{

            //}

            private void RedirectLogin(ActionExecutingContext filterContext)
            {
                Controller controller = (Controller)filterContext.Controller;

                controller.TempData["TempMsgType"] = MsgTypeEnum.info;
                controller.TempData["TempMsg"] = "請登入";

                /* redirect to action */
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary
                    {
                        { "area", "Backend" },
                        { "controller", "Home" },
                        { "action", "Login" }
                    });
            }
        }
    }
}
