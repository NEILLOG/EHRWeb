using BASE.Areas.Backend.Models;
using BASE.Areas.Backend.Models.Extend;
using BASE.Areas.Backend.Service;
using BASE.Extensions;
using BASE.Service;
using BASE.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using BASE.Filters;
using BASE.Models.DB;
using Microsoft.EntityFrameworkCore;
using BASE.Models;
using System.Data.Entity;
using NPOI.SS.Formula.Functions;

namespace BASE.Areas.Backend.Controllers
{
    public class ContactUsController : BaseController
    {
        private readonly AllCommonService _allCommonService;
        private readonly CommonService _commonService;
        private readonly ExportService _exportService;
        private readonly B_ContactUsService _ContactService;
        private readonly FileService _fileService;

        public ContactUsController(AllCommonService allCommonService,
            FileService fileService,
            CommonService commonService,
            ExportService exportService,
            B_ContactUsService ContactService)
        {
            _allCommonService = allCommonService;
            _fileService = fileService;
            _commonService = commonService;
            _exportService = exportService;
            _ContactService = ContactService;
        }

        /// <summary>
        /// ContactUsList列表
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [BackendCheckLogin("Menu000044", "ENABLED")]
        public async Task<IActionResult> ContactUsList(VM_ContactUs data)
        {
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "網站建議管理", Action = "檢視";

            try
            {
                //取資料
                IQueryable<TbContactUs>? dataList = _ContactService.GetContactUsdList(ref _message);

                //分頁
                if (dataList != null)
                    data.ContactUsList = await PagerInfoService.GetRange(dataList.OrderByDescending(x => x.CreateDate), data.Search.PagerInfo);

                //操作紀錄
                await _commonService.OperateLog(userinfo.UserID, Feature, Action, null, data);
            }
            catch (Exception ex)
            {
                TempData["TempMsgType"] = MsgTypeEnum.error;
                TempData["TempMsg"] = "伺服器連線異常，請檢查您的網路狀態後再試一次！";

                string response = (TempData["TempMsg"] == null ? "" : TempData["TempMsg"].ToString()) + "\r\n" + (TempData["TempMsgDetail"] == null ? "" : TempData["TempMsgDetail"].ToString());

                await _commonService.OperateLog(userinfo.UserID, Feature, Action, null, data, ex.ToString(), response, false);
                await _allCommonService.Error_Record("Backend", Feature + "-" + Action, ex.ToString());

                return RedirectToAction("Index", "Home", new { area = "Backend" });
            }

            return View(data);
        }

        /// <summary>
        /// 匯出網站建議管理
        /// </summary>
        /// <returns></returns>
        [BackendCheckLogin("Menu000044", "DOWNLOAD")]
        [ValidateAntiForgeryToken]
        [RequestFormLimits(ValueCountLimit = int.MaxValue)] // post form data 大小限制 
        [HttpPost]
        public async Task<IActionResult> ContactUsExport(VM_ContactUs data)
        {
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "網站建議管理", Action = "匯出";

            // 最終動作成功與否
            bool isSuccess = false;

            // 例外錯誤發生，特別記錄至 TbLog
            bool unCaughtError = false;

            ActionResultModel<MemoryStream> result = new ActionResultModel<MemoryStream>();
            string FileName = "網站建議管理_" + DateTime.Today.ToString("yyyyMMdd");

            try
            {
                IQueryable<ContactUsExtend>? dataList = _ContactService.GetContactUsExtendList(ref _message);

                if (dataList != null)
                    result = _exportService.ContactUsExcel(dataList);

                if (!result.IsSuccess)
                {
                    TempData["TempMsgDetail"] = "發生技術性錯誤，請聯絡技術人員或稍後再試一次";
                    _message += result.Message;
                    unCaughtError = true;
                }

                isSuccess = result.IsSuccess;
            }
            catch (Exception ex)
            {
                TempData["TempMsgType"] = MsgTypeEnum.error;
                TempData["TempMsg"] = "伺服器連線異常，請檢查您的網路狀態後再試一次！";

                _message += ex.ToString();
                unCaughtError = true;
            }
            if (isSuccess)
            {
                TempData["TempMsgType"] = MsgTypeEnum.success;
                TempData["TempMsg"] = "匯出成功";

            }
            else
            {
                TempData["TempMsgType"] = MsgTypeEnum.error;
                TempData["TempMsg"] = TempData["TempMsg"] ?? "匯出失敗";

                if (unCaughtError)
                {
                    await _allCommonService.Error_Record("Backend", Feature + "-" + Action, _message);
                }
            }

            //操作紀錄
            string response = (TempData["TempMsg"] == null ? "" : TempData["TempMsg"].ToString()) + "\r\n" + (TempData["TempMsgDetail"] == null ? "" : TempData["TempMsgDetail"].ToString());
            await _commonService.OperateLog(userinfo.UserID, Feature, Action, null, null, _message, response, isSuccess);

            if (isSuccess)
            {
                return File(result.Data.ToArray(), "application/vnd.ms-excel", FileName + ".xlsx");
            }
            else
            {
                return RedirectToAction("ContactUsList");
            }
        }
    }
}
