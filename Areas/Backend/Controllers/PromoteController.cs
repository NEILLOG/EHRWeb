using BASE.Areas.Backend.Models.Extend;
using BASE.Areas.Backend.Models;
using BASE.Areas.Backend.Service;
using BASE.Areas.Frontend.Service;
using BASE.Filters;
using BASE.Models.Enums;
using BASE.Service;
using Microsoft.AspNetCore.Mvc;
using BASE.Extensions;
using BASE.Models.DB;
using BASE.Models;
using NPOI.SS.UserModel;
using static Humanizer.In;
using System.Xml.Linq;

namespace BASE.Areas.Backend.Controllers
{
    public class PromoteController : BaseController
    {
        private readonly AllCommonService _allCommonService;
        private readonly CommonService _commonService;
        private readonly PromoteService _promoteService;
        private readonly ImportService _importService;
        private readonly MailService _mailService;
        private readonly IConfiguration _config;

        public PromoteController(AllCommonService allCommonService,
            CommonService commonService,
            PromoteService promoteService,
            ImportService importService,
            MailService mailService,
            IConfiguration configuration)
        {
            _allCommonService = allCommonService;
            _commonService = commonService;
            _promoteService = promoteService;
            _importService = importService;
            _mailService = mailService;
            _config = configuration;
        }

        /// <summary>
        /// 推廣管理
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [BackendCheckLogin("Menu000059", "ENABLED")]
        public async Task<IActionResult> PromoteManage(VM_Promote data)
        {
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "推廣管理", Action = "檢視";

            try
            {
                // 下拉：企業人數
                data.ddlNumCompanies = _promoteService.SetDDL_NumCompanies();

                // 下拉：計畫
                data.ddlPlan = _promoteService.SetDDL_Plan(1);

                //取資料
                List<TbPromotion>? dataList = _promoteService.GetPromotionList(ref _message, data.Search);

                //分頁
                if (dataList != null)
                    data.PromotionList = await PagerInfoService.GetRange(dataList.AsQueryable(), data.Search.PagerInfo);

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
        /// 匯入企業/計畫清單
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        [BackendCheckLogin("Menu000059", "DOWNLOAD")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PromoteImport(IFormFile file)
        {
            JsonResponse<string> result = new JsonResponse<string>();
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "推廣管理", Action = "資料匯入";

            DateTime dtnow = DateTime.Now;

            // 最終動作成功與否
            bool isSuccess = false;

            // 例外錯誤發生，特別記錄至 TbLog
            bool unCaughtError = false;

            try
            {
                bool hasError = false;

                /* 欄位數 */
                int column_num = 7;

                // 取得 附檔名
                string extension = Path.GetExtension(file.FileName);
                // 取得 Workbook 實例
                var wb = _importService.ReadExcel(extension, file);

                if (wb.IsSuccess == false || wb.Data == null)
                {
                    result.MessageDetail = "檔案讀取失敗";
                    _message += wb.Message;
                    hasError = true;
                }
                else
                {
                    List<TbPromotion> insert_Promotion = new List<TbPromotion>();

                    for (int i = 0; i < wb.Data.NumberOfSheets; i++)
                    {
                        // 取得 sheet
                        var sheet = wb.Data.GetSheetAt(i);

                        // sheet 名稱
                        string sheetName = sheet.SheetName;
                        
                        for (int row = 1; row <= sheet.PhysicalNumberOfRows; row++)
                        {
                            // 驗證不是空白列
                            IRow sheetRow = sheet.GetRow(row);

                            /* row 的欄位數對才處理 */
                            if (sheetRow != null)
                            {
                                if (sheetRow.PhysicalNumberOfCells == column_num)
                                {
                                    string Company = sheetRow.GetCell(0).ToString();
                                    string BusinessID = sheetRow.GetCell(1).ToString();
                                    string Location = sheetRow.GetCell(2).ToString();
                                    string EmpoyeeAmount = sheetRow.GetCell(3).ToString();
                                    string Applicant = sheetRow.GetCell(4).ToString();
                                    string Email = sheetRow.GetCell(5).ToString();
                                    string PlanCategory = sheetRow.GetCell(6).ToString();

                                    /* 檢查必填項 */
                                    if (StringExtensions.CheckIsNullOrEmpty(Company, BusinessID, Location, EmpoyeeAmount, Applicant, PlanCategory))
                                    {
                                        result.MessageDetail += $"第 {i + 1} 活頁簿，第 {row + 1} 列企業名稱、統一編號、企業所在地、就保人、申請人、電子郵件、計畫分類須必填，已略過\n";
                                        hasError = true;
                                    }
                                    else
                                    {
                                        TbPromotion newPromotion = new TbPromotion();
                                        newPromotion.CompanyName = Company;
                                        newPromotion.BusinessId = BusinessID;
                                        newPromotion.CompanyLocation = Location;
                                        newPromotion.EmpoyeeAmount = int.Parse(EmpoyeeAmount);
                                        newPromotion.Applicant = Applicant;
                                        newPromotion.Email = Email;
                                        newPromotion.Project = PlanCategory;

                                        insert_Promotion.Add(newPromotion);
                                    }
                                }
                                else
                                {
                                    result.MessageDetail += $"第 {i + 1} 活頁簿，第 {row + 1} 列欄位數目不正確，已略過\n";
                                    //hasError = true;
                                }
                            }
                        }
                    }

                    if (!hasError)
                    {
                        using (var transaction = _promoteService.GetTransaction())
                        {
                            try
                            {
                                /* 刪除此次處理結果 */
                                List<TbPromotion> delete_Promotion = new List<TbPromotion>();
                                delete_Promotion = _promoteService.Lookup<TbPromotion>(ref _message).ToList();
                                await _promoteService.DeleteRange(delete_Promotion, transaction);

                                /* 新增此次處理結果 */
                                await _promoteService.InsertRange(insert_Promotion, transaction);
                                transaction.Commit();
                                isSuccess = true;
                            }
                            catch (Exception ex)
                            {
                                _message += ex.ToString();
                                TempData["TempMsgDetail"] = "發生技術性錯誤，請聯絡技術人員或稍後再試一次";
                                unCaughtError = true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.Message = "伺服器連線異常，請檢查您的網路狀態後再試一次！";

                _message += ex.ToString();
                unCaughtError = true;
            }

            if (isSuccess)
            {
                result.alert_type = "success";
                result.Message = "匯入成功";
            }
            else
            {
                result.alert_type = "error";
                result.Message = result.Message ?? "匯入失敗";

                if (unCaughtError)
                {
                    await _allCommonService.Error_Record("Backend", Feature + "-" + Action, _message);
                }
            }

            string response = result.Message + "\r\n" + result.MessageDetail;

            await _commonService.OperateLog(userinfo.UserID, Feature, Action, null, null, _message, response, isSuccess);

            return Json(result);
        }

        /// <summary>
        /// 編輯信件
        /// </summary>
        /// <returns></returns>
        [BackendCheckLogin("Menu000063", "MODIFY")]
        public async Task<IActionResult> PromoteSendMail(string EmpoyeeAmount,string plan)
        {
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "編輯信件", Action = "檢視";

            // 最終動作成功與否
            bool isSuccess = false;

            // 例外錯誤發生，特別記錄至 TbLog
            bool unCaughtError = false;

            VM_Promote data = new VM_Promote();
            data.Search.NumCompanies = EmpoyeeAmount;
            data.Search.Plan = plan;

            await _commonService.OperateLog(userinfo.UserID, Feature, Action);
            return View(data);
        }

        /// <summary>
        /// [POST]編輯信件_存檔
        /// </summary>
        /// <returns></returns>
        [BackendCheckLogin("Menu000063", "MODIFY")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PromoteSendMail(VM_Promote datapost)
        {
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "編輯信件", Action = "檢視";

            // 最終動作成功與否
            bool isSuccess = false;

            // 例外錯誤發生，特別記錄至 TbLog
            bool unCaughtError = false;

            try
            {
                // 取得所有寄信對象
                List<TbPromotion>? dataList = _promoteService.GetPromotionList(ref _message, datapost.Search);
                List<string> listMail = dataList.Where(x => !string.IsNullOrEmpty(x.Email)).Select(x => x.Email).ToList();

                if (dataList != null && (listMail != null && listMail.Any()))
                {
                    List<MailAddressInfo> listToAddress = new List<MailAddressInfo>();
                    foreach (var itemMailAddress in listMail)
                    {
                        MailAddressInfo itemMail = new MailAddressInfo(itemMailAddress);
                        listToAddress.Add(itemMail);
                    }

                    //寄送預約信件
                    await _mailService.ReserveSendEmail(new MailViewModel()
                    {
                        ToList = listToAddress,
                        Subject = datapost.MailSubject,
                        Body = datapost.MailContent
                    }, userinfo.UserID, DateTime.Now, "PromoteMail");
                }
                else
                {
                    TempData["TempMsgType"] = MsgTypeEnum.error;
                    TempData["TempMsg"] = TempData["TempMsg"] ?? "無任何對象可寄送，請重新查詢";

                    return RedirectToAction("PromoteManage");
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
                TempData["TempMsg"] = "寄信成功";
            }
            else
            {
                TempData["TempMsgType"] = MsgTypeEnum.error;
                TempData["TempMsg"] = TempData["TempMsg"] ?? "寄信失敗";

                if (unCaughtError)
                {
                    await _allCommonService.Error_Record("Backend", Feature + "-" + Action, _message);
                }
            }

            //操作紀錄
            string response = (TempData["TempMsg"] == null ? "" : TempData["TempMsg"].ToString()) + "\r\n" + (TempData["TempMsgDetail"] == null ? "" : TempData["TempMsgDetail"].ToString());
            await _commonService.OperateLog(userinfo.UserID, Feature, Action, "", datapost, _message, response, isSuccess);

            return RedirectToAction("PromoteManage");

        }
    }
}
