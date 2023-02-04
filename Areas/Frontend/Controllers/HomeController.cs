using BASE.Areas.Frontend.Models;
using BASE.Areas.Frontend.Service;
using BASE.Extensions;
using BASE.Models.DB;
using BASE.Models.Enums;
using BASE.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Drawing;
using System.Speech.Synthesis;
using System.Text.RegularExpressions;

namespace BASE.Areas.Frontend.Controllers
{
    public class HomeController : BaseController
    {
        private readonly IConfiguration _conf;
        private readonly AllCommonService _allCommonService;
        private readonly FileService _fileService;
        private readonly NewsService _newsService;
        private readonly ActivityService _activityService;
        private readonly AdService _adService;
        private readonly RelationLinkService _relationlinkService;
        private readonly VistorService _vistorService;

        public HomeController(IConfiguration configuration,
            AllCommonService allCommonService,
            NewsService newsService,
            FileService fileService,
            AdService adService,
            RelationLinkService relationlinkService,
            ActivityService activityService,
            VistorService vistorService)
        {
            _conf = configuration;
            _allCommonService = allCommonService;
            _newsService = newsService;
            _fileService = fileService;
            _adService = adService;
            _relationlinkService = relationlinkService;
            _activityService = activityService;
            _vistorService = vistorService;
        }

        public async Task<IActionResult> Index()
        {
            String id = "2"; id = EncryptService.AES.RandomizedEncrypt(id);
            String a = "Act0000004"; a = EncryptService.AES.RandomizedEncrypt(a);
            String b = "21"; b = EncryptService.AES.RandomizedEncrypt(b);

            VM_Index data = new VM_Index();

            try
            {
                data.Ads = _adService.GetExtendItemList(ref _message);
                data.News = _newsService.GetNewsList(ref _message, new VM_NewsQueryParam()).Take(5).ToList();
                data.Activities = _activityService.GetActivityList(ref _message, new VM_ActivityQueryParam()).Take(7).ToList();
                data.RelationLinks = _relationlinkService.GetExtendItemList(ref _message);

                foreach (var item in data.Activities)
                    item.Sections = _allCommonService.Lookup<TbActivitySection>(ref _message, x => x.ActivityId == item.Header.Id).OrderBy(x => x.Day).ToList();

            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Home", new { area = "Frontend" });
            }

            return View(data);
        }

        public async Task<IActionResult> Accessible()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> ajaxSubscript(string email, List<String> types)
        {
            string Feature = "訂閱服務", Action = "新增";

            // 最終動作成功與否
            bool isSuccess = false;

            // 例外錯誤發生，特別記錄至 TbLog
            bool unCaughtError = false;

            String Message = "";
            Boolean IsUpdate = true;

            try
            {
                try
                {
                    TbSubScript? main = _allCommonService.Lookup<TbSubScript>(ref _message, x => x.Email == email).FirstOrDefault();
                    if (main == null)
                    {
                        main = new TbSubScript();
                        IsUpdate = false;
                    }

                    main.Email = email;
                    main.IsSubscriptActivity = false; //重置，由本次結果重新決定訂閱項目
                    main.IsSubsrcriptProjectNews = false;  //重置，由本次結果重新決定訂閱項目

                    foreach (var type in types)
                    {
                        switch (type)
                        {
                            case "Activity":
                                main.IsSubscriptActivity = true;
                                break;
                            case "ProjectNews":
                                main.IsSubsrcriptProjectNews = true;
                                break;
                        }
                    }

                    //新增
                    if(IsUpdate)
                        await _fileService.Update(main);
                    else
                        await _fileService.Insert(main);

                    isSuccess = true;
                }
                catch (Exception ex)
                {
                    _message += ex.ToString();
                    Message = "發生技術性錯誤，請聯絡技術人員或稍後再試一次";
                    unCaughtError = true;
                }
            }
            catch (Exception ex)
            {
                Message = "伺服器連線異常，請檢查您的網路狀態後再試一次！";

                _message += ex.ToString();
                unCaughtError = true;
            }


            if (isSuccess)
            {
                Message = $"您的訂閱服務{(IsUpdate ? "異動" : "新增")}已完成";
            }
            else
            {
                Message = Message ?? "儲存失敗";

                if (unCaughtError)
                {
                    await _allCommonService.Error_Record("Frontend", Feature + "-" + Action, _message);
                }
            }

            return Json(new { IsSuccess = isSuccess, Message = Message });
        }

        public async Task<ActionResult> TTS(string id)
        {
            String code = String.Empty;
            switch (id)
            {
                default:
                case "Activity": code = HttpContext.Session.Get<String>(Backend.Models.SessionStruct.VerifyCode.Activity); break;
                case "Consult": code = HttpContext.Session.Get<String>(Backend.Models.SessionStruct.VerifyCode.Consult); break;
                case "Subscript": code = HttpContext.Session.Get<String>(Backend.Models.SessionStruct.VerifyCode.Subscript); break;
                case "ContactUs": code = HttpContext.Session.Get<String>(Backend.Models.SessionStruct.VerifyCode.ContactUs); break;
            }

            Task<FileContentResult> task = Task.Run(() =>
            {
                using (SpeechSynthesizer speechSynthesizer = new SpeechSynthesizer())
                {
                    using (MemoryStream stream = new MemoryStream())
                    {
                        speechSynthesizer.SetOutputToWaveStream(stream);

                        var pb = new PromptBuilder();

                        foreach (var s in code)
                        {
                            if (Regex.IsMatch(s.ToString(), "\\d"))
                            {
                                pb.StartVoice("Microsoft Hanhan Desktop");
                                pb.AppendText(s.ToString(), PromptRate.Slow);
                            }
                            else
                            {
                                pb.StartVoice("Microsoft Zira Desktop");
                                pb.AppendText(s.ToString(), PromptRate.Slow);
                            }
                            
                            pb.AppendBreak();

                            pb.EndVoice();
                        }

                        speechSynthesizer.Speak(pb);

                        var bytes = stream.GetBuffer();
                        return File(bytes, "audio/x-wav");
                    }
                }
            });

            return await task;
        }

        public ActionResult GetValidateCode(string id, string refresh = "")
        {
            String code = String.Empty;
            if(refresh == "Y")
                switch (id)
                {
                    default:
                    case "Activity": HttpContext.Session.Set(Backend.Models.SessionStruct.VerifyCode.Activity, new ValidImageHelper().RandomCode(5)); break;
                    case "Consult": HttpContext.Session.Set(Backend.Models.SessionStruct.VerifyCode.Consult, new ValidImageHelper().RandomCode(5)); break;
                    case "Subscript": HttpContext.Session.Set(Backend.Models.SessionStruct.VerifyCode.Subscript, new ValidImageHelper().RandomCode(5)); break;
                    case "ContactUs": HttpContext.Session.Set(Backend.Models.SessionStruct.VerifyCode.ContactUs, new ValidImageHelper().RandomCode(5));  break;
                }

            switch (id)
            {
                default:
                case "Activity": code = HttpContext.Session.Get<String>(Backend.Models.SessionStruct.VerifyCode.Activity); break;
                case "Consult": code = HttpContext.Session.Get<String>(Backend.Models.SessionStruct.VerifyCode.Consult); break;
                case "Subscript": code = HttpContext.Session.Get<String>(Backend.Models.SessionStruct.VerifyCode.Subscript); break;
                case "ContactUs": code = HttpContext.Session.Get<String>(Backend.Models.SessionStruct.VerifyCode.ContactUs); break;
            }

            byte[] data = null;

            ValidImageHelper Helper = new ValidImageHelper();

            //定義一個畫板
            MemoryStream ms = new MemoryStream();
            using (Bitmap map = new Bitmap(164, 50))
            {
                //畫筆,在指定畫板畫板上畫圖
                //g.Dispose();
                using (Graphics g = Graphics.FromImage(map))
                {
                    g.Clear(Color.White);
                    g.DrawString(code, new Font("黑體", 18.0F), Brushes.Blue, new Point(10, 8));
                    //繪製干擾線(數字代表幾條)
                    Helper.PaintInterLine(g, 10, map.Width, map.Height);
                }
                map.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            }
            data = ms.GetBuffer();
            return File(data, "image/jpeg");
        }

        [HttpPost]
        public async Task<JsonResult> ajaxValidVerify(string UserInput, string type)
        {
            bool isSuccess = false;
            string Message = String.Empty;

            String code = String.Empty;
            switch (type)
            {
                default:
                case "Activity": code = HttpContext.Session.Get<String>(Backend.Models.SessionStruct.VerifyCode.Activity); break;
                case "Consult": code = HttpContext.Session.Get<String>(Backend.Models.SessionStruct.VerifyCode.Consult); break;
                case "Subscript": code = HttpContext.Session.Get<String>(Backend.Models.SessionStruct.VerifyCode.Subscript); break;
                case "ContactUs": code = HttpContext.Session.Get<String>(Backend.Models.SessionStruct.VerifyCode.ContactUs); break;
            }

            isSuccess = code == UserInput ? true : false;

            return Json(new { isSuccess = isSuccess, Message = Message });
        }

        public async Task<JsonResult> Visitors()
        {
            String ErrorMsg = "";
            String ip = GetIPAddress_IPv4();

            Int32 visitors = await _vistorService.Visitors(ip);

            return Json(new { isSuccess = true, data = visitors });
        }

        public string GetIPAddress_IPv4()
        {
            string sIPAddress = string.Empty;

            try
            {
                sIPAddress = HttpContext.Connection.RemoteIpAddress.ToString();
            }
            catch (Exception ex)
            {
                sIPAddress = string.Empty;
            }

            return sIPAddress;

        }

    }
}
