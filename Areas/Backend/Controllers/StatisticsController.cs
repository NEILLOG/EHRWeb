using BASE.Areas.Backend.Models;
using BASE.Areas.Backend.Models.Extend;
using BASE.Areas.Backend.Service;
using BASE.Areas.Frontend.Service;
using BASE.Extensions;
using BASE.Filters;
using BASE.Models;
using BASE.Models.DB;
using BASE.Models.Enums;
using BASE.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BASE.Areas.Backend.Controllers
{
    public class StatisticsController : BaseController
    {
        private readonly AllCommonService _allCommonService;
        private readonly CommonService _commonService;
        private readonly IConfiguration _config;
        private readonly StatisticsService _statisticsService;

        public StatisticsController(
            AllCommonService allCommonService,
            CommonService commonService,
            StatisticsService statisticsService,
            IConfiguration configuration)
        {
            _allCommonService = allCommonService;
            _commonService = commonService;
            _statisticsService = statisticsService;
            _config = configuration;
        }

        /// <summary>
        /// 統計分析
        /// </summary>
        /// <returns></returns>
        [BackendCheckLogin("Menu000064", "ENABLED")]
        public async Task<IActionResult> Index()
        {
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "統計分析", Action = "檢視";

            VM_Statistics data = new VM_Statistics();

            try
            {
                // 下拉資料
                data.ddlActivityName = _statisticsService.SetDDL_ActivityName(0).ToList();
                data.ddlYear = _statisticsService.SetDDL_Year();

                //操作紀錄
                await _commonService.OperateLog(userinfo.UserID, Feature, Action, null, "");
            }
            catch (Exception ex)
            {

                TempData["TempMsgType"] = MsgTypeEnum.error;
                TempData["TempMsg"] = "伺服器連線異常，請檢查您的網路狀態後再試一次！";

                string response = (TempData["TempMsg"] == null ? "" : TempData["TempMsg"].ToString()) + "\r\n" + (TempData["TempMsgDetail"] == null ? "" : TempData["TempMsgDetail"].ToString());

                await _commonService.OperateLog(userinfo.UserID, Feature, Action, null, "", ex.ToString(), response, false);
                await _allCommonService.Error_Record("Backend", Feature + "-" + Action, ex.ToString());

                return RedirectToAction("Index", "Home", new { area = "Backend" });
            }

            return View(data);
        }

        /// <summary>
        /// 取得活動清單資料
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [BackendCheckLogin("Menu000064", "ENABLED")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetActivity(string year)
        {
            VM_Statistics data = new VM_Statistics();

            int search_year = 0;
            if (!string.IsNullOrEmpty(year))
            {
                search_year = (int.Parse(year) + 1911);
            }           

            data.ddlActivityName = _statisticsService.SetDDL_ActivityName(search_year).ToList();

            return Json(data.ddlActivityName);
        }

        /// <summary>
        /// 取得圖表資料(活動)
        /// </summary>
        /// <param name="id"></param>
        /// <param name="id"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [BackendCheckLogin("Menu000064", "ENABLED")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetActivityChart(string ActID, string Filter)
        {
            string _Msg = string.Empty;

            VM_Statistics data = new VM_Statistics();

            var registers = _statisticsService.getActivityData(ActID, Filter);

            data.ChartInfo = registers;

            return Json(data.ChartInfo);
        }

        /// <summary>
        /// 取得諮詢輔導資料
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [BackendCheckLogin("Menu000064", "ENABLED")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetConsultCount(string year)
        {
            VM_Statistics data = new VM_Statistics();

            int search_year = 0;
            data.TotalCount = 0;
            if (!string.IsNullOrEmpty(year))
            {
                search_year = (int.Parse(year) + 1911);
            }

            data.TotalCount = _statisticsService.getTotalCount(search_year);

            return Json(data.TotalCount);
        }

        /// <summary>
        /// 取得圖表資料(諮詢輔導)
        /// </summary>
        /// <param name="id"></param>
        /// <param name="id"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [BackendCheckLogin("Menu000064", "ENABLED")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetConsultChart(string Year)
        {
            string _Msg = string.Empty;

            VM_Statistics data = new VM_Statistics();

            int search_year = 0;
            search_year = (int.Parse(Year) + 1911);
            var registers = _statisticsService.getConsultData(search_year);
            data.ChartInfo = registers;

            return Json(data.ChartInfo);
        }

    }
}
