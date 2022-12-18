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
using BASE.Areas.Backend.Service;
using System.Text;
using BASE.Models;
using Microsoft.Win32;
using System.Net.Mail;

namespace BASE.Areas.Backend.Controllers
{
    public class HomeController : BaseController
    {
        private readonly DBContext _dBContext;
        private readonly AllCommonService _allCommonService;
        private readonly MemberService _memberService;
        private string _Message = string.Empty;
        private readonly IConfiguration _conf;
        private readonly MailService _mailService;
        private readonly IHttpContextAccessor _contextAccessor = null!;

        public HomeController(DBContext context,
                              AllCommonService allCommonService,
                              IConfiguration conf,
                              MemberService memberService,
                              MailService mailService,
                              IHttpContextAccessor contextAccessor)
        {
            _dBContext = context;
            _allCommonService = allCommonService;
            _conf = conf;
            _memberService = memberService;
            _mailService = mailService;
            _contextAccessor = contextAccessor;
        }
        /// <summary>
        /// 後台首頁
        /// </summary>
        /// <returns></returns>
        [BackendCheckLogin]
        public IActionResult Index()
        {
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string ErrorMsg = string.Empty;
            // 判斷密碼是否超過三個月未修改
            bool change = _memberService.NeedChangePWD(ref ErrorMsg, userinfo.UserID);
            if (change)
            {
                TempData["TempMsgType"] = MsgTypeEnum.warning;
                TempData["TempMsg"] = "密碼已超過2個月未修改，請至個人維護頁面進行密碼修改";

                // 轉址到個人維護資料
                return RedirectToAction("PersonalManage", "Member");
            }

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
            string LoginMessage = string.Empty;
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

                            // 寫入登入紀錄
                            LoginMessage = "登入失敗";
                            await _allCommonService.LoginRecord(Platform, LoginMessage, datapost.acct);

                            int? ErrorCount;
                            // 錯誤達3次鎖定機制
                            TbUserInfo CheckAccout = await _allCommonService.Lookup<TbUserInfo>(ref _Message, x => x.Account == datapost.acct).FirstOrDefaultAsync();
                            if (CheckAccout != null)
                            {
                                // 判斷是否為鎖定期間
                                if (CheckAccout.LockTime != null)
                                {
                                    if (DateTime.Now.Subtract((DateTime)CheckAccout.LockTime).Minutes < 15)
                                    {
                                        TempData["TempMsgType"] = MsgTypeEnum.error;
                                        TempData["TempMsg"] = "帳密錯誤達三次，此帳號已被鎖定，請稍後15分鐘後再試";
                                        return View(datapost);
                                    }
                                    else
                                    {
                                        CheckAccout.ErrorCount = 0;
                                        CheckAccout.LockTime = null;
                                    }                                   
                                }
                                ErrorCount = 0;
                                if (CheckAccout.ErrorCount == null)
                                {
                                    ErrorCount = 1;
                                }
                                else
                                {
                                    ErrorCount = CheckAccout.ErrorCount + 1;
                                }
                                CheckAccout.ErrorCount = ErrorCount;
                                if (ErrorCount >= 3)
                                {
                                    TempData["TempMsgType"] = MsgTypeEnum.error;
                                    TempData["TempMsg"] = "帳密錯誤達三次，此帳號已被鎖定，請稍後15分鐘後再試";

                                    CheckAccout.LockTime = DateTime.Now;
                                }
                                else
                                {
                                    TempData["TempMsgType"] = MsgTypeEnum.error;
                                    TempData["TempMsg"] = "帳號或密碼錯誤，請重新輸入";
                                }
                                // update 資料表
                                await _allCommonService.Update<TbUserInfo>(CheckAccout);
                            }
                            
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

                                // 寫入登入紀錄
                                LoginMessage = "登入成功";
                                await _allCommonService.LoginRecord(Platform, LoginMessage, datapost.acct, User.UserId);

                                // 判斷是否為鎖定期間
                                if (User.LockTime != null)
                                {
                                    if (DateTime.Now.Subtract((DateTime)User.LockTime).Minutes < 15)
                                    {
                                        TempData["TempMsgType"] = MsgTypeEnum.error;
                                        TempData["TempMsg"] = "帳密錯誤達三次，此帳號已被鎖定，請稍後15分鐘後再試";

                                        return View(datapost);
                                    }
                                }

                                // 清除LockTime 及 ErrorCount
                                User.LockTime = null;
                                User.ErrorCount = 0;
                                // update 資料表
                                await _allCommonService.Update<TbUserInfo>(User);

                                return RedirectToAction("Index", "Home", new { area = "Backend" });
                            }
                            else
                            {
                                TempData["TempMsgType"] = MsgTypeEnum.error;
                                TempData["TempMsg"] = "此帳號已被停用";

                                // 寫入登入紀錄
                                LoginMessage = "帳號停用";
                                await _allCommonService.LoginRecord(Platform, LoginMessage, datapost.acct, User.UserId);

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
        /// 忘記密碼
        /// </summary>
        /// <returns></returns>
        public IActionResult ForgetPassword()
        {
            VM_Home data = new VM_Home();

            return View(data);
        }

        /// <summary>
        /// 忘記密碼
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgetPassword(VM_Home datapost)
        {
            HttpRequest httpRequest = _contextAccessor.HttpContext.Request;
            string webSiteDomain = new StringBuilder().Append(httpRequest.Scheme).Append("://").Append(httpRequest.Host).ToString();

            if (string.IsNullOrEmpty(datapost.acct) || string.IsNullOrEmpty(datapost.email))
            {
                TempData["TempMsgType"] = MsgTypeEnum.error;
                TempData["TempMsg"] = "請輸入帳號及信箱資訊";

                return View(datapost);
            }

            try
            {
                // 搜尋此帳號是否存在
                TbUserInfo User = await _allCommonService.Lookup<TbUserInfo>(ref _Message, x => x.Account == datapost.acct && x.Email == datapost.email).SingleOrDefaultAsync();
                if (User == null)
                {
                    TempData["TempMsgType"] = MsgTypeEnum.error;
                    TempData["TempMsg"] = "此帳號不存在或錯誤";
                }
                else
                {
                    try
                    {
                        string random = new Random(Guid.NewGuid().GetHashCode()).ToString();
                        string code = EncryptService.AES.Base64Encrypt(random);
                        DateTime dtnow = DateTime.Now;

                        byte[] bytes = Encoding.UTF8.GetBytes(code);
                        string bs_code = Convert.ToBase64String(bytes);

                        // 寄發忘記密碼信件
                        string mailBody = string.Empty;
                        string Data = @"
                        <html>
                        <head>
                        </head>
                        <body>
                        您好：<br>
                        <a href='{0}'>點此連結進行密碼重設</a><br>
                        注意!!此連結5分鐘有效，請盡速進行密碼重設!!
                        </body>
                        </html>
                        ";
                        StringBuilder sb = new StringBuilder();
                        sb.AppendFormat(Data, webSiteDomain + "/Backend/Home/ResetPassword?id=" + datapost.acct + "&code=" + bs_code + "&time=" + dtnow);
                        mailBody = sb.ToString();

                        await _mailService.SendEmail(new MailViewModel()
                        {
                            ToList = new List<MailAddressInfo>() { new MailAddressInfo(User.Email) },
                            Subject = "忘記密碼申請",
                            Body = mailBody,
                        });
                        // 更新token
                        User.Token = code;
                        await _allCommonService.Update(User);

                        TempData["TempMsgType"] = MsgTypeEnum.success;
                        TempData["TempMsg"] = "成功，請收信按照流程重設密碼！";

                        return RedirectToAction("Login", "Home");
                    }
                    catch (Exception ex)
                    {
                        TempData["TempMsgType"] = MsgTypeEnum.error;
                        TempData["TempMsg"] = "發生技術性錯誤，請聯絡技術人員或稍後再試一次";
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["TempMsgType"] = MsgTypeEnum.error;
                TempData["TempMsg"] = "伺服器連線異常，請檢查您的網路狀態後再試一次";
            }
            

            return View(datapost);
        }

        /// <summary>
        /// 修改密碼
        /// </summary>
        /// <returns></returns>
        public IActionResult ResetPassword(string id, string code, string time)
        {
            VM_Home data = new VM_Home();

            // 判斷時間戳記
            DateTime dtnow = DateTime.Now;
            TimeSpan span = dtnow - Convert.ToDateTime(time);
            int minute = span.Minutes;
            if (minute > 5)
            {
                TempData["TempMsgType"] = MsgTypeEnum.warning;
                TempData["TempMsg"] = "此連結已失效！";

                return RedirectToAction("Login");
            }

            TbUserInfo UserInfo = _allCommonService.Lookup<TbUserInfo>(ref _Message, x => x.Account == id).SingleOrDefault();
            if (UserInfo == null)
            {
                TempData["TempMsgType"] = MsgTypeEnum.warning;
                TempData["TempMsg"] = "此連結帳號不存在或錯誤！";

                return RedirectToAction("Login");
            }
            else
            {
                byte[] bytes = Convert.FromBase64String(code);
                string bs_code = Encoding.UTF8.GetString(bytes);

                if (UserInfo.Token != bs_code)
                {
                    TempData["TempMsgType"] = MsgTypeEnum.warning;
                    TempData["TempMsg"] = "此連結Token已失效！";

                    return RedirectToAction("Login");
                }
            }

            data.acct = id;

            return View(data);
        }

        /// <summary>
        /// 修改密碼
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(VM_Home datapost)
        {
            if (string.IsNullOrEmpty(datapost.aua8) || string.IsNullOrEmpty(datapost.aua8Check))
            {
                TempData["TempMsgType"] = MsgTypeEnum.error;
                TempData["TempMsg"] = "請輸入密碼資訊";

                return View(datapost);
            }

            if (datapost.aua8 != datapost.aua8Check)
            {
                TempData["TempMsgType"] = MsgTypeEnum.error;
                TempData["TempMsg"] = "密碼資訊不一致";

                return View(datapost);
            }

            TbUserInfo User = _allCommonService.Lookup<TbUserInfo>(ref _Message, x => x.Account == datapost.acct).SingleOrDefault();
            if (User == null)
            {
                TempData["TempMsgType"] = MsgTypeEnum.warning;
                TempData["TempMsg"] = "此連帳號不存在或錯誤！";

                return RedirectToAction("Login");
            }
            else
            {
                User.Aua8 = EncryptService.AES.Base64Encrypt(datapost.aua8);
                // 判斷密碼是否可以修改
                string ErrorMsg = string.Empty;
                bool checkPwd = _memberService.UserPWDCheck(ref ErrorMsg, User.UserId, User.Aua8);
                if (!checkPwd)
                {
                    TempData["TempMsgType"] = MsgTypeEnum.error;
                    TempData["TempMsg"] = TempData["TempMsg"] ?? ErrorMsg;

                    /* 失敗回原頁 */
                    return View(datapost);
                }

                // 更新密碼               
                User.ModifyDate = DateTime.Now;
                await _allCommonService.Update(User);

                // 寫入密碼歷程
                TbPwdLog PwdLog = new TbPwdLog();
                PwdLog.UserId = User.UserId;
                PwdLog.Password = User.Aua8;
                PwdLog.CreateUser = User.UserId;
                PwdLog.CreateDate = DateTime.Now;

                await _allCommonService.Insert(PwdLog);

                TempData["TempMsgType"] = MsgTypeEnum.success;
                TempData["TempMsg"] = TempData["TempMsg"] ?? "密碼重設成功";
            }

            return RedirectToAction("Login");
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
