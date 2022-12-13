using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using BASE.Filters;
using BASE.Areas.Backend.Models;
using BASE.Service;
using BASE.Models.DB;
using BASE.Models.Enums;
using BASE.Extensions;
using Microsoft.EntityFrameworkCore;
using BASE.Extensions;

namespace BASE.Areas.Backend.Controllers
{
    public class HomeController : BaseController
    {
        private readonly DBContext _dBContext;
        private readonly AllCommonService _allCommonService;
        private string _Message = string.Empty;
        private readonly IConfiguration _conf;

        public HomeController(DBContext context,
                              AllCommonService allCommonService,
                              IConfiguration conf)
        {
            _dBContext = context;
            _allCommonService = allCommonService;
            _conf = conf;
        }
        /// <summary>
        /// 後台首頁
        /// </summary>
        /// <returns></returns>
        [BackendCheckLogin]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 後台登入頁
        /// </summary>
        /// <returns></returns>
        public IActionResult Login()
        {
            VM_Home data = new VM_Home();

            return View(data);
        }
        
        /// <summary>
        /// 後台登入頁
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(VM_Home datapost)
        {
            string ActionName = "登入";
            try
            {
                if (!string.IsNullOrEmpty(datapost.acct) && !string.IsNullOrEmpty(datapost.aua8))
                {
                    // 檢查驗證碼是否存在
                    string? captchaCookie = HttpContext.Request.Cookies.Get<string>(CookieStruct.Captcha);

                    if (string.IsNullOrEmpty(captchaCookie))
                    {
                        TempData["TempMsgType"] = MsgTypeEnum.error;
                        TempData["TempMsg"] = "等待時間過久，請重新輸入驗證碼";
                        return View(datapost);
                    }
                    else if (string.Compare(datapost.captcha, captchaCookie, true) != 0)
                    {
                        TempData["TempMsgType"] = MsgTypeEnum.error;
                        TempData["TempMsg"] = "驗證碼輸入錯誤";
                        return View(datapost);
                    }
                    else
                    {
                        string aua8 = EncryptService.AES.Base64Encrypt(datapost.aua8);
                        TbUserInfo User = await _allCommonService.Lookup<TbUserInfo>(ref _Message, x => x.Account == datapost.acct && x.Aua8 == aua8).SingleOrDefaultAsync();
                        if (User == null)
                        {
                            TempData["TempMsgType"] = MsgTypeEnum.error;
                            TempData["TempMsg"] = "帳號或密碼錯誤，請重新輸入";

                            return View(datapost);
                        }
                        else
                        {
                            TbUserInGroup userInDroup = await _allCommonService.Lookup<TbUserInGroup>(ref _Message, x => x.UserId == User.UserId).SingleOrDefaultAsync();

                            if (User.IsActive)
                            {
                                UserSessionModel U = new UserSessionModel();
                                U.UserID = User.UserId;
                                U.UserName = User.UserName;
                                U.GroupID = userInDroup.GroupId;

                                HttpContext.Session.Set(SessionStruct.Login.UserInfo, U);
                                TempData["TempMsgType"] = MsgTypeEnum.success;
                                TempData["TempMsg"] = "登入成功";

                                return RedirectToAction("Index", "Home", new { area = "Backend" });
                            }
                            else
                            {
                                TempData["TempMsgType"] = MsgTypeEnum.error;
                                TempData["TempMsg"] = "此帳號已被停用";

                                return View(datapost);
                            }
                        }
                    }
                   
                }
                else
                {
                    TempData["TempMsgType"] = MsgTypeEnum.error;
                    TempData["TempMsg"] = "帳號或密碼不得為空，請重新輸入";

                    return View(datapost);
                }

            }
            catch (Exception ex)
            {
                TempData["TempMsgType"] = MsgTypeEnum.error;
                TempData["TempMsg"] = "發生錯誤";

                await _allCommonService.Error_Record(Platform, ActionName, ex.ToString());

                return View(datapost);
            }

        }

        /// <summary>
        /// 登出
        /// </summary>
        /// <returns></returns>
        [BackendCheckLogin]
        public IActionResult SignOut()
        {
            string ActionName = "登出";
            HttpContext.Session.Clear();
            TempData["TempMsgType"] = MsgTypeEnum.success;
            TempData["TempMsg"] = "登出成功";

            return RedirectToAction("Login", "Home", new { area = "Backend" });
        }

        /// <summary>
        /// 產生驗證碼
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> GetCaptcha()
        {
            bool SSL = _conf.GetValue<string>("Site:SSL") == "Y";

            try
            {
                //產生驗證碼 並 記錄在Cookies(加密)
                string CheckCode = CaptchaService.CreateCheckCode();
                HttpContext.Response.Cookies.Set(CookieStruct.Captcha, CheckCode, new CookieOptions
                {
                    Expires = DateTime.Now.AddMinutes(5),
                    IsEssential = true,
                    Secure = SSL,
                    HttpOnly = true,
                    SameSite = SameSiteMode.Strict,
                });

                //產生驗證碼圖片
                MemoryStream imageStream = CaptchaService.Create(CheckCode);
                return File(imageStream.GetBuffer(), "image/png");
            }
            catch (Exception ex)
            {
                await _allCommonService.Error_Record("後台登入", "驗證碼", ex.Message);
                return BadRequest();
            }
        }
    }
}
