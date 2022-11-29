using BASE.Areas.Frontend.Models;
using BASE.Areas.Frontend.Service;
using BASE.Models.DB;
using BASE.Models.Enums;
using BASE.Service;
using Microsoft.AspNetCore.Mvc;

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

        public HomeController(IConfiguration configuration,
            AllCommonService allCommonService,
            NewsService newsService,
            FileService fileService,
            AdService adService,
            RelationLinkService relationlinkService,
            ActivityService activityService)
        {
            _conf = configuration;
            _allCommonService = allCommonService;
            _newsService = newsService;
            _fileService = fileService;
            _adService= adService;
            _relationlinkService = relationlinkService;
            _activityService = activityService;
        }

        public async Task<IActionResult> Index()
        {
            String id = "1"; id = EncryptService.AES.RandomizedEncrypt(id);

            VM_Index data = new VM_Index();

            try
            {
                data.Ads = _adService.GetExtendItemList(ref _message);
                data.News = _newsService.GetNewsList(ref _message, new VM_NewsQueryParam()).Take(5).ToList();
                data.Activities = _activityService.GetActivityList(ref _message, new VM_ActivityQueryParam()).Take(7).ToList();
                data.RelationLinks = _relationlinkService.GetExtendItemList(ref _message);

            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Home", new { area = "Frontend" });
            }

            return View(data);
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

    }
}
