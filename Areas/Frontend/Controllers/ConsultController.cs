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
using BASE.Areas.Backend.Service;
using ConsultService = BASE.Areas.Frontend.Service.ConsultService;

namespace BASE.Areas.Frontend.Controllers
{
    public class ConsultController : BaseController
    {
        private readonly IConfiguration _conf;
        private readonly AllCommonService _allCommonService;
        private readonly OnePageService _onepageService;
        private readonly ConsultService _consultService;

        public ConsultController(IConfiguration configuration,
            AllCommonService allCommonService,
            OnePageService onepageService,
            ConsultService consultService)
        {
            _conf = configuration;
            _allCommonService = allCommonService;
            _onepageService = onepageService;
            _consultService = consultService;
        }

        public async Task<IActionResult> Index()
        {
            VM_OnePage data = new VM_OnePage();

            try
            {
                data.ExtendItem = _onepageService.GetExtendItem(ref _message, "OP000003"); //TODO: 待補編號
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Home", new { area = "Frontend" });
            }

            return View(data);
        }


        public async Task<IActionResult> Register()
        {
            VM_ConsultRegister data = new VM_ConsultRegister();
         
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
        public async Task<IActionResult> Register(VM_ConsultRegister datapost)
        {
            string Feature = "諮詢輔導服務", Action = "新增";

            
            bool isSuccess = false; // 最終動作成功與否
            bool unCaughtError = false; // 例外錯誤發生，特別記錄至 TbLog

            try
            {
                datapost.ExtendItem.CreateDate = DateTime.Now;
                datapost.ExtendItem.ConsultSubjects = String.Join(",", datapost.CheckedSubjects);
                try
                {
                    await _consultService.Insert(datapost.ExtendItem);
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
                TempData["CostomTempEmail"] = datapost.ExtendItem.ContactEmail;
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
                return RedirectToAction("Index");
            }
            else
            {
                if (datapost.ExtendItem == null)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    return View(datapost);
                }
            }

        }
    }
}
