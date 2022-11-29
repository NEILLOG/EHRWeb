using BASE.Areas.Frontend.Models;
using BASE.Service;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Serialization;
using BASE.Extensions;
using BASE.Filters;
using BASE.Models.Enums;
using BASE.Models.DB;
using Microsoft.EntityFrameworkCore;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using System.Reflection;
using BASE.Areas.Frontend.Service;
using BASE.Models;
using BASE.Areas.Frontend.Models.Extend;

namespace BASE.Areas.Frontend.Controllers
{
    public class ContactUsController : BaseController
    {
        private readonly IConfiguration _conf;
        private readonly AllCommonService _allCommonService;


        public ContactUsController(IConfiguration configuration,
            AllCommonService allCommonService)
        {
            _conf = configuration;
            _allCommonService = allCommonService;
        }

        public async Task<IActionResult> Index()
        {

            VM_ContactUs data = new VM_ContactUs();

            try
            {
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Home", new { area = "Frontend" });
            }

            return View(data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(VM_ContactUs datapost)
        {
            string Feature = "聯絡我們", Action = "新增";

            // 最終動作成功與否
            bool isSuccess = false;

            // 例外錯誤發生，特別記錄至 TbLog
            bool unCaughtError = false;

            datapost.ExtendItem.CreateDate = DateTime.Now;

            try
            {
                try
                {
                    //新增
                    await _allCommonService.Insert(datapost.ExtendItem);

                    isSuccess = true;
                }
                catch (Exception ex)
                {
                    _message += ex.ToString();
                    TempData["TempMsgDetail"] = "發生技術性錯誤，請聯絡技術人員或稍後再試一次";
                    unCaughtError = true;
                }
            }
            catch (Exception ex)
            {
                TempData["TempMsg"] = "伺服器連線異常，請檢查您的網路狀態後再試一次！";

                _message += ex.ToString();
                unCaughtError = true;
            }


            if (isSuccess)
            {
                TempData["TempMsgType"] = MsgTypeEnum.success;
                TempData["TempMsg"] = "傳送成功";
            }
            else
            {
                TempData["TempMsgType"] = MsgTypeEnum.error;
                TempData["TempMsg"] = TempData["TempMsg"] ?? "儲存失敗";

                if (unCaughtError)
                {
                    await _allCommonService.Error_Record("Frontend", Feature + "-" + Action, _message);
                }
            }

            if (isSuccess)
            {
                return RedirectToAction("Index", "Home"); //導向清單頁
            }
            else
            {
                return View(datapost);
            }
        }

    }
}
