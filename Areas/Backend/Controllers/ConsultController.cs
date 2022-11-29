using BASE.Areas.Backend.Models;
using BASE.Areas.Backend.Service;
using BASE.Filters;
using BASE.Models.Enums;
using BASE.Service;
using Microsoft.AspNetCore.Mvc;
using BASE.Extensions;
using BASE.Models.DB;
using BASE.Areas.Backend.Models.Extend;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using BASE.Models;

namespace BASE.Areas.Backend.Controllers
{
    public class ConsultController : BaseController
    {
        private readonly AllCommonService _allCommonService;
        private readonly CommonService _commonService;
        private readonly ConsultService _consultService;
        private readonly FileService _fileService;
        private readonly ExportService _exportService;
        private readonly MailService _mailService;
        private readonly IConfiguration _config;

        public ConsultController(AllCommonService allCommonService,
            FileService fileService,
            CommonService commonService,
            ConsultService consultService,
            ExportService exportService,
            MailService mailService,
            IConfiguration configuration)
        {
            _allCommonService = allCommonService;
            _commonService = commonService;
            _consultService = consultService;
            _fileService = fileService;
            _exportService = exportService;
            _mailService = mailService;
            _config = configuration;
        }

        /// <summary>
        /// 諮詢輔導服務介紹
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [BackendCheckLogin("Menu000052", "ENABLED")]
        public async Task<IActionResult> ConsultIntro()
        {
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "諮詢輔導服務介紹", Action = "檢視";
            VM_Consult data = new VM_Consult();

            try
            {
                data.consultIntro = _consultService.Lookup<TbOnePage>(ref _message, x => x.Id == "OP000003").FirstOrDefault();

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
        /// 諮詢輔導服務介紹
        /// </summary>
        /// <returns></returns>
        [BackendCheckLogin("Menu000052", "MODIFY")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConsultIntro(VM_Consult datapost)
        {
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "諮詢輔導服務介紹", Action = "編輯";

            // 最終動作成功與否
            bool isSuccess = false;

            // 例外錯誤發生，特別記錄至 TbLog
            bool unCaughtError = false;

            DateTime dtnow = DateTime.Now;

            TbOnePage? item = new TbOnePage();

            try
            {
                if (datapost.consultIntro == null)
                {
                    TempData["TempMsg"] = "資料回傳有誤，請重新操作！";
                }
                else
                {
                    item = _consultService.Lookup<TbOnePage>(ref _message, x => x.Id == "OP000003").FirstOrDefault();
                    item.Contents = datapost.consultIntro.Contents;
                    item.ModifyDate = dtnow;
                    item.ModifyUser = userinfo.UserID;

                    using (var transaction = _consultService.GetTransaction())
                    {
                        try
                        {
                            //新增
                            await _consultService.Update(item, transaction);

                            transaction.Commit();
                            isSuccess = true;
                        }
                        catch (Exception ex)
                        {
                            TempData["TempMsgDetail"] = "發生技術性錯誤，請聯絡技術人員或稍後再試一次";

                            _message += ex.ToString();
                            unCaughtError = true;
                        }
                    }
                }

                //操作紀錄
                await _commonService.OperateLog(userinfo.UserID, Feature, Action, null, datapost);
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
                TempData["TempMsg"] = "儲存成功";
            }
            else
            {
                TempData["TempMsgType"] = MsgTypeEnum.error;
                TempData["TempMsg"] = TempData["TempMsg"] ?? "儲存失敗";

                if (unCaughtError)
                {
                    await _allCommonService.Error_Record("Backend", Feature + "-" + Action, _message);
                }
            }

            //操作紀錄
            string response = (TempData["TempMsg"] == null ? "" : TempData["TempMsg"].ToString()) + "\r\n" + (TempData["TempMsgDetail"] == null ? "" : TempData["TempMsgDetail"].ToString());
            await _commonService.OperateLog(userinfo.UserID, Feature, Action);

            if (isSuccess)
            {
                return RedirectToAction("ConsultIntro");
            }
            else
            {
                if (item == null || item == null)
                {
                    return RedirectToAction("ConsultIntro");
                }
                else
                {
                    /* 失敗回原頁 */
                    return View(datapost);
                }
            }
        }

        /// <summary>
        /// 諮詢輔導服務報名名單列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [BackendCheckLogin("Menu000053", "ENABLED")]
        public async Task<IActionResult> ConsultList(VM_Consult data)
        {
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "諮詢輔導服務報名名單", Action = "檢視";

            try
            {
                //取資料
                List<ConsultExtend>? dataList = _consultService.GetConsultExtendList(ref _message, data.Search);

                //分頁
                if (dataList != null)
                    data.ConsultExtendList = await PagerInfoService.GetRange(dataList.AsQueryable(), data.Search.PagerInfo);

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
        /// 諮詢輔導指派及編輯
        /// </summary>
        /// <returns></returns>
        [BackendCheckLogin("Menu000054", "MODIFY")]
        public async Task<IActionResult> ConsultEdit(string id) 
        {
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "諮詢輔導審核", Action = "檢視";

            VM_Consult data = new VM_Consult();

            // 最終動作成功與否
            bool isSuccess = false;

            // 例外錯誤發生，特別記錄至 TbLog
            bool unCaughtError = false;

            string decrypt_id = EncryptService.AES.RandomizedDecrypt(id);

            try {
                if (string.IsNullOrEmpty(decrypt_id))
                {
                    TempData["TempMsgDetail"] = "金鑰逾時！請重新再操作一次！";
                }
                else 
                {
                    ConsultExtend? dataItem = _consultService.GetConsultExtendItem(ref _message, decrypt_id);

                    if (dataItem != null)
                    {
                        data.ConsultExtendItem = dataItem;

                        // 審核狀態綁定
                        data.ConsultExtendItem.sApprove = dataItem.ConsultRegister.IsApprove.HasValue ? (dataItem.ConsultRegister.IsApprove.Value == true ?"1":"2") : "-1";

                    }

                    if (data.ConsultExtendItem == null)
                    {
                        TempData["TempMsgDetail"] = "查無指定項目！";
                    }
                    else
                    {
                        isSuccess = true;
                    }

                    // 下拉_顧問
                    data.ddlConsult = _consultService.SetDDL_Consult(1,dataItem.ConsultRegister.ConsultSubjects);
                    // 下拉_輔導助理
                    data.ddlAssistant = _consultService.SetDDL_Assistant(1);
                    // 下拉_審核
                    data.ddlAudit = _consultService.SetDDL_AuditStatus();
                }
            }
            catch (Exception ex) {
                TempData["TempMsg"] = "伺服器連線異常，請檢查您的網路狀態後再試一次！";

                _message += ex.ToString();
                unCaughtError = true;
            }

            if (isSuccess)
            {
                await _commonService.OperateLog(userinfo.UserID, Feature, Action, decrypt_id, id);
                return View(data);
            }
            else
            {
                TempData["TempMsgType"] = MsgTypeEnum.error;
                TempData["TempMsg"] = TempData["TempMsg"] ?? "操作失敗";

                //操作紀錄
                string response = (TempData["TempMsg"] == null ? "" : TempData["TempMsg"].ToString()) + "\r\n" + (TempData["TempMsgDetail"] == null ? "" : TempData["TempMsgDetail"].ToString());
                await _commonService.OperateLog(userinfo.UserID, Feature, Action, decrypt_id, id, _message, response, isSuccess);

                if (unCaughtError)
                {
                    await _allCommonService.Error_Record("Backend", Feature + "-" + Action, _message);
                }

                return RedirectToAction("ConsultList");
            }
        }

        /// <summary>
        /// [POST]諮詢輔導指派及編輯
        /// </summary>
        /// <returns></returns>
        [BackendCheckLogin("Menu000054", "MODIFY")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConsultEdit(string id, VM_Consult datapost)
        {
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "諮詢輔導審核", Action = "編輯";

            // 最終動作成功與否
            bool isSuccess = false;

            // 例外錯誤發生，特別記錄至 TbLog
            bool unCaughtError = false;

            DateTime dtnow = DateTime.Now;

            string decrypt_id = EncryptService.AES.RandomizedDecrypt(id);

            TbConsultRegister? item = null;

            try {

                if (string.IsNullOrEmpty(decrypt_id))
                {
                    TempData["TempMsgDetail"] = "金鑰逾時！請重新再操作一次！";
                }
                else 
                {
                    long CRId = Convert.ToInt64(decrypt_id);
                    IQueryable<TbConsultRegister>? temp = _consultService.Lookup<TbConsultRegister>(ref _message, x => x.Id == CRId);
                    if (temp != null)
                        item = await temp.SingleOrDefaultAsync();

                    if (item == null)
                    {
                        TempData["TempMsgDetail"] = "查無指定項目！";
                    }
                    else if (datapost.ConsultExtendItem == null)
                    {
                        TempData["TempMsgDetail"] = "資料回傳有誤，請重新操作！";
                    }
                    else
                    {
                        // 尚未審核通過方可編輯
                        item.AssignAdviser1 = datapost.ConsultExtendItem.ConsultRegister.AssignAdviser1;
                        item.AssignAdviser2 = datapost.ConsultExtendItem.ConsultRegister.AssignAdviser2;
                        item.AssignAdviser3 = datapost.ConsultExtendItem.ConsultRegister.AssignAdviser3;
                        item.AssignAdviserAssistant = datapost.ConsultExtendItem.ConsultRegister.AssignAdviserAssistant;
                        item.ReAssignDate = datapost.ConsultExtendItem.ConsultRegister.ReAssignDate;
                        item.ReAssignTime = datapost.ConsultExtendItem.ConsultRegister.ReAssignTime;
                        item.IsApprove = datapost.ConsultExtendItem.sApprove != "-1" ?(datapost.ConsultExtendItem.sApprove == "1" ? true : false) : null;
                        item.ModifyDate = dtnow;
                        item.ModifyUser = userinfo.UserID;

                        using (var transaction = _consultService.GetTransaction())
                        {
                            try
                            {
                                //編輯
                                await _consultService.Update(item, transaction);

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
            catch (Exception ex){
                TempData["TempMsg"] = "伺服器連線異常，請檢查您的網路狀態後再試一次！";
                _message += ex.ToString();
                unCaughtError = true;
            }
            if (isSuccess)
            {
                // 寄信通知
                if (item.IsApprove.HasValue && item.IsApprove.Value)
                {
                    // 輔導日期處理
                    string sYear = item.ReAssignDate.HasValue ? (item.ReAssignDate.Value.Year - 1911).ToString() : "--";
                    string sMonth = item.ReAssignDate.HasValue ? (item.ReAssignDate.Value.Month).ToString() : "--";
                    string sDay = item.ReAssignDate.HasValue ? (item.ReAssignDate.Value.Day).ToString() : "--";
                    string sTime = item.ReAssignTime.HasValue ? (item.ReAssignTime.Value).ToString() : "--";

                    // 指派顧問資料撈處
                    List<TbUserInfo> listConsultant = new List<TbUserInfo>();
                    string sAdviser = "";
                    List<string> listConsultantID = new List<string>();
                    if (!string.IsNullOrEmpty(item.AssignAdviser1))
                        listConsultantID.Add(item.AssignAdviser1);
                    if (!string.IsNullOrEmpty(item.AssignAdviser2))
                        listConsultantID.Add(item.AssignAdviser2);
                    if (!string.IsNullOrEmpty(item.AssignAdviser3))
                        listConsultantID.Add(item.AssignAdviser3);

                    if (listConsultantID != null && listConsultantID.Any())
                    {
                        listConsultant = _consultService.Lookup<TbUserInfo>(ref _message, x => listConsultantID.Contains(x.UserId)).ToList();
                        foreach (var itemAdviser in listConsultant)
                        {
                            sAdviser += string.IsNullOrEmpty(sAdviser) ? string.Concat(itemAdviser.UserName, "顧問") : string.Concat(sAdviser, "、", itemAdviser.UserName, "顧問");
                        }
                    }

                    // 指派輔導助理資料
                    TbUserInfo assistantItem = new TbUserInfo();
                    string sAssistant = "";
                    string sAssistantPhone = "";
                    string sAssistantEmail = "";
                    if (!string.IsNullOrEmpty(item.AssignAdviserAssistant))
                    {
                        assistantItem = _consultService.Lookup<TbUserInfo>(ref _message, x => x.UserId == item.AssignAdviserAssistant).FirstOrDefault();
                        sAssistant = assistantItem != null ? string.Concat(assistantItem.UserName, "業務輔導員 ") : "--";
                        sAssistantPhone = (assistantItem != null && !string.IsNullOrEmpty(assistantItem.Phone)) ? assistantItem.Phone : "--";
                        sAssistantEmail = (assistantItem != null && !string.IsNullOrEmpty(assistantItem.Email)) ? assistantItem.Email : "--";
                    }

                    //-- 系統需發送行前通知信給企業
                    if (!string.IsNullOrEmpty(item.ContactEmail))
                    {
                        //主旨
                        string sSubject = string.Concat("【勞動部桃竹苗分署人資整合案_諮詢服務】會前通知_", sYear,"年", sMonth, "月", sDay, "日 ", sTime, item.Name);

                        //內容
                        string sContent = string.Concat(item.Name,item.ContactName, item.ContactJobTitle, "您好<br /><br />");
                        sContent += "感謝您與我們確認進場輔導時間，有關輔導資訊於此紀錄供您參酌。<br /><br />";
                        sContent += string.Concat("有關", sMonth, "月", sDay, "日諮詢輔導服務當日資訊如下：<br />");
                        sContent += string.Concat("輔導日期時間：", sYear, "年", sMonth, "月", sDay, "日 ", sTime, "<br />");
                        sContent += string.Concat("輔導地點：", item.ConsultAddress, "<br />");
                        sContent += string.Concat("輔導顧問：", sAdviser, "<br />");
                        sContent += string.Concat("當日輔導隨行助理：", sAssistant, sAssistantPhone, "<br /><br />");
                        sContent += "敬祝 順心平安<br />";
                        sContent += "勞動部勞動力發展署桃竹苗分署<br />";
                        sContent += "桃竹苗區域運籌人力資源整合服務計畫_專案辦公室<br />";
                        sContent += sAssistant + "<br />";
                        sContent += string.Concat("TEL：",sAssistantPhone,"<br />");
                        sContent += string.Concat("Email：", sAssistantEmail);

                        //直接測試寄信
                        await _mailService.SendEmail(new MailViewModel()
                        {
                            ToList = new List<MailAddressInfo>() { new MailAddressInfo(item.ContactEmail) },
                            Subject = sSubject,
                            Body = sContent
                        });

                    }

                    //-- 發送提醒信件給顧問
                    foreach (var itemConsultant in listConsultant)
                    {
                        //主旨
                        string sSubject = string.Concat("【勞動部桃竹苗分署人資整合案_諮詢服務】", itemConsultant.UserName,"顧問您好，邀約顧問協助企業輔導_", sMonth, "月", sDay, "日 ", sTime, item.Name,"，謝謝您!");

                        //內容
                        string sContent = string.Concat("敬愛的", itemConsultant.UserName,"顧問您好：<br /><br />");
                        sContent += "非常感謝顧問今年度幫忙本計畫進行相關輔導，謝謝顧問的幫忙~<br />";
                        sContent += "稍早有致電聯繫顧問，請顧問幫忙輔導的企業如下：<br /><br />";
                        sContent += string.Concat("輔導日期時間：", sMonth, "月", sDay, "日 ", sTime,"<br />");
                        sContent += string.Concat("● 輔導企業：", item.Name, "<br />");
                        sContent += string.Concat("● 輔導地址：", item.ConsultAddress, "<br />");
                        sContent += string.Concat("● 附件資料：諮詢需求調查表待企業填寫完畢再提供給顧問<br />");
                        sContent += string.Concat("● 當日輔導隨行助理：", sAssistant, sAssistantPhone, "<br /><br />");
                        sContent += "以上再麻煩顧問了，<br />有任何問題都歡迎您隨時致電或來信，謝謝顧問的幫忙~<br /><br />";
                        sContent += "敬祝 順心平安<br />";
                        sContent += "勞動部勞動力發展署桃竹苗分署<br />";
                        sContent += "桃竹苗區域運籌人力資源整合服務計畫_專案辦公室<br />";
                        sContent += sAssistant + "<br />";
                        sContent += string.Concat("TEL：", sAssistantPhone, "<br />");
                        sContent += string.Concat("Email：", sAssistantEmail);

                        //直接測試寄信
                        await _mailService.SendEmail(new MailViewModel()
                        {
                            ToList = new List<MailAddressInfo>() { new MailAddressInfo(itemConsultant.Email) },
                            Subject = sSubject,
                            Body = sContent
                        });
                    }

                }

                TempData["TempMsgType"] = MsgTypeEnum.success;
                TempData["TempMsg"] = "儲存成功";
            }
            else
            {
                TempData["TempMsgType"] = MsgTypeEnum.error;
                TempData["TempMsg"] = TempData["TempMsg"] ?? "儲存失敗";

                if (unCaughtError)
                {
                    await _allCommonService.Error_Record("Backend", Feature + "-" + Action, _message);
                }
            }

            //操作紀錄
            string response = (TempData["TempMsg"] == null ? "" : TempData["TempMsg"].ToString()) + "\r\n" + (TempData["TempMsgDetail"] == null ? "" : TempData["TempMsgDetail"].ToString());
            await _commonService.OperateLog(userinfo.UserID, Feature, Action, decrypt_id, datapost, _message, response, isSuccess);

            if (isSuccess)
                return RedirectToAction("ConsultList");
            else
            {
                if (item == null)
                {
                    return RedirectToAction("ConsultList");
                }
                else
                {
                    /* 失敗回原頁 */
                    return View(datapost);
                }
            }
        }

        /// <summary>
        /// 諮詢輔導報名名單管理
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [BackendCheckLogin("Menu000055", "ENABLED")]
        public async Task<IActionResult> ConsultManage(string id)
        {
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "諮詢輔導報名名單管理", Action = "檢視";

            VM_Consult data = new VM_Consult();

            // 最終動作成功與否
            bool isSuccess = false;

            // 例外錯誤發生，特別記錄至 TbLog
            bool unCaughtError = false;

            string decrypt_id = EncryptService.AES.RandomizedDecrypt(id);

            try {
                if (string.IsNullOrEmpty(decrypt_id))
                {
                    TempData["TempMsgDetail"] = "金鑰逾時！請重新再操作一次！";
                }
                else
                {
                    // 群組ID
                    data.GroupId = userinfo.GroupID;

                    ConsultExtend? dataItem = _consultService.GetConsultExtendItem(ref _message, decrypt_id);

                    if (dataItem != null)
                    {
                        data.ConsultExtendItem = dataItem;

                        // 審核狀態綁定
                        data.ConsultExtendItem.sClose = dataItem.ConsultRegister.IsClose == true ? "1" : "0";
                    }

                    if (data.ConsultExtendItem == null)
                    {
                        TempData["TempMsgDetail"] = "查無指定項目！";
                    }
                    else
                    {
                        isSuccess = true;
                    }

                    // 下拉_是否結案
                    data.ddlClose = _consultService.SetDDL_CloseStatus();
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
                await _commonService.OperateLog(userinfo.UserID, Feature, Action, decrypt_id, id);
                return View(data);
            }
            else
            {
                TempData["TempMsgType"] = MsgTypeEnum.error;
                TempData["TempMsg"] = TempData["TempMsg"] ?? "操作失敗";

                //操作紀錄
                string response = (TempData["TempMsg"] == null ? "" : TempData["TempMsg"].ToString()) + "\r\n" + (TempData["TempMsgDetail"] == null ? "" : TempData["TempMsgDetail"].ToString());
                await _commonService.OperateLog(userinfo.UserID, Feature, Action, decrypt_id, id, _message, response, isSuccess);

                if (unCaughtError)
                {
                    await _allCommonService.Error_Record("Backend", Feature + "-" + Action, _message);
                }

                return RedirectToAction("ConsultList");
            }
        }

        /// <summary>
        /// [POST]諮詢輔導報名名單管理
        /// </summary>
        /// <returns></returns>
        [BackendCheckLogin("Menu000055", "MODIFY")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConsultManage(string id, VM_Consult datapost)
        {
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "諮詢輔導報名名單管理", Action = "編輯";

            // 最終動作成功與否
            bool isSuccess = false;

            // 例外錯誤發生，特別記錄至 TbLog
            bool unCaughtError = false;

            DateTime dtnow = DateTime.Now;

            string decrypt_id = EncryptService.AES.RandomizedDecrypt(id);

            TbConsultRegister? item = null;

            try
            {

                if (string.IsNullOrEmpty(decrypt_id))
                {
                    TempData["TempMsgDetail"] = "金鑰逾時！請重新再操作一次！";
                }
                else
                {
                    long CRId = Convert.ToInt64(decrypt_id);
                    IQueryable<TbConsultRegister>? temp = _consultService.Lookup<TbConsultRegister>(ref _message, x => x.Id == CRId);
                    if (temp != null)
                        item = await temp.SingleOrDefaultAsync();

                    if (item == null)
                    {
                        TempData["TempMsgDetail"] = "查無指定項目！";
                    }
                    else if (datapost.ConsultExtendItem == null)
                    {
                        TempData["TempMsgDetail"] = "資料回傳有誤，請重新操作！";
                    }
                    else
                    {
                        item.IsClose = datapost.ConsultExtendItem.sClose == "1" ?  true : false;
                        item.ModifyDate = dtnow;
                        item.ModifyUser = userinfo.UserID;

                        using (var transaction = _consultService.GetTransaction())
                        {
                            try
                            {
                                // 輔導紀錄
                                if (datapost.CounselingLogFile != null)
                                {
                                    var CounselingLog_upload = await _fileService.FileUploadAsync(datapost.CounselingLogFile, "Consult/" + item.Id, "Consult", item.CounselingLogFile, null, transaction);
                                    if (CounselingLog_upload.IsSuccess == true && !string.IsNullOrEmpty(CounselingLog_upload.FileID))
                                    {
                                        item.CounselingLogFile = CounselingLog_upload.FileID;
                                    }
                                    else
                                    {
                                        _message += CounselingLog_upload.Message;
                                    }
                                }

                                // 已填寫完之簽到表
                                if (datapost.SigninFormFile != null)
                                {
                                    var SigninForm_upload = await _fileService.FileUploadAsync(datapost.SigninFormFile, "Consult/" + item.Id, "Consult", item.SigninFormFile, null, transaction);
                                    if (SigninForm_upload.IsSuccess == true && !string.IsNullOrEmpty(SigninForm_upload.FileID))
                                    {
                                        item.SigninFormFile = SigninForm_upload.FileID;
                                    }
                                    else
                                    {
                                        _message += SigninForm_upload.Message;
                                    }
                                }

                                //編輯
                                await _consultService.Update(item, transaction);

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
                TempData["TempMsg"] = "伺服器連線異常，請檢查您的網路狀態後再試一次！";
                _message += ex.ToString();
                unCaughtError = true;
            }
            if (isSuccess)
            {
                TempData["TempMsgType"] = MsgTypeEnum.success;
                TempData["TempMsg"] = "儲存成功";
            }
            else
            {
                TempData["TempMsgType"] = MsgTypeEnum.error;
                TempData["TempMsg"] = TempData["TempMsg"] ?? "儲存失敗";

                if (unCaughtError)
                {
                    await _allCommonService.Error_Record("Backend", Feature + "-" + Action, _message);
                }
            }

            //操作紀錄
            string response = (TempData["TempMsg"] == null ? "" : TempData["TempMsg"].ToString()) + "\r\n" + (TempData["TempMsgDetail"] == null ? "" : TempData["TempMsgDetail"].ToString());
            await _commonService.OperateLog(userinfo.UserID, Feature, Action, decrypt_id, datapost, _message, response, isSuccess);

            if (isSuccess)
                return RedirectToAction("ConsultList");
            else
            {
                if (item == null)
                {
                    return RedirectToAction("ConsultList");
                }
                else
                {
                    /* 失敗回原頁 */
                    return View(datapost);
                }
            }
        }

        /// <summary>
        /// 歷史輔導紀錄列表
        /// </summary>
        /// <returns></returns>
        [BackendCheckLogin("Menu000058", "ENABLED")]
        public async Task<IActionResult> CounselingHistory(string id)
        {
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "歷史輔導紀錄", Action = "檢視";

            VM_Consult data = new VM_Consult();

            // 最終動作成功與否
            bool isSuccess = false;

            // 例外錯誤發生，特別記錄至 TbLog
            bool unCaughtError = false;

            string decrypt_id = EncryptService.AES.RandomizedDecrypt(id);
            
            try {
                if (string.IsNullOrEmpty(decrypt_id))
                {
                    TempData["TempMsgDetail"] = "金鑰逾時！請重新再操作一次！";
                }
                else
                {
                    //List<TbConsultRegister>? dataItem = _consultService.Lookup<TbConsultRegister>(ref _message, x => x.BusinessId == decrypt_id && !string.IsNullOrEmpty(x.CounselingLogFile)).OrderByDescending(x => x.CreateDate).ToList();
                    List<CounselingHistoryExtend>? dataItem = _consultService.GetCounselingHistoryExtendList(ref _message, decrypt_id);

                    if (dataItem != null)
                    {
                        data.CounselingHistoryExtendList = dataItem;
                        // 諮詢輔導報名主表ID
                        data.ConsultRegisterId = dataItem.FirstOrDefault().ConsultRegister.Id.ToString();
                    }

                    if (data.CounselingHistoryExtendList == null)
                        TempData["TempMsgDetail"] = "查無指定項目！";
                    else
                        isSuccess = true;
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
                await _commonService.OperateLog(userinfo.UserID, Feature, Action, decrypt_id, id);
                return View(data);
            }
            else
            {
                TempData["TempMsgType"] = MsgTypeEnum.error;
                TempData["TempMsg"] = TempData["TempMsg"] ?? "操作失敗";

                //操作紀錄
                string response = (TempData["TempMsg"] == null ? "" : TempData["TempMsg"].ToString()) + "\r\n" + (TempData["TempMsgDetail"] == null ? "" : TempData["TempMsgDetail"].ToString());
                await _commonService.OperateLog(userinfo.UserID, Feature, Action, decrypt_id, id, _message, response, isSuccess);

                if (unCaughtError)
                {
                    await _allCommonService.Error_Record("Backend", Feature + "-" + Action, _message);
                }

                return RedirectToAction("ConsultManage");
            }
        }

        /// <summary>
        /// 匯出諮詢輔導報名名單列表
        /// </summary>
        /// <returns></returns>
        [BackendCheckLogin("Menu000055", "DOWNLOAD")]
        [ValidateAntiForgeryToken]
        [RequestFormLimits(ValueCountLimit = int.MaxValue)] // post form data 大小限制 
        [HttpPost]
        public async Task<IActionResult> ConsultRegistrationExport(VM_Consult data)
        {
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "諮詢輔導服務報名名單", Action = "匯出";

            // 最終動作成功與否
            bool isSuccess = false;

            // 例外錯誤發生，特別記錄至 TbLog
            bool unCaughtError = false;

            ActionResultModel<MemoryStream> result = new ActionResultModel<MemoryStream>();
            string FileName = "諮詢輔導服務報名名單_" + DateTime.Today.ToString("yyyyMMdd");

            try
            {
                //取資料
                List<ConsultExtend>? dataList = _consultService.GetConsultExtendList(ref _message, data.Search);

                result = _exportService.ConsultRegistrationExcel(dataList.AsQueryable());

                isSuccess = true;

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
                return RedirectToAction("EventInfoManage");
            }
        }

        /// <summary>
        /// 匯出諮詢輔導簽到表
        /// </summary>
        /// <returns></returns>
        [BackendCheckLogin("Menu000055", "DOWNLOAD")]
        [ValidateAntiForgeryToken]
        [RequestFormLimits(ValueCountLimit = int.MaxValue)] // post form data 大小限制 
        [HttpPost]
        public async Task<IActionResult> ConsultSigninExport(VM_Consult data) 
        {
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "諮詢輔導服務簽到表", Action = "匯出";

            // 最終動作成功與否
            bool isSuccess = false;

            // 例外錯誤發生，特別記錄至 TbLog
            bool unCaughtError = false;

            ActionResultModel<MemoryStream> result = new ActionResultModel<MemoryStream>();
            string FileName = "諮詢輔導服務簽到表_" + DateTime.Today.ToString("yyyyMMdd");
            try
            {
                //取資料
                CounselingSigninExtend? dataItem = _consultService.GetCounselingSignin(ref _message, data.ConsultRegisterId);

                result = _exportService.ConsultSigninExcel(dataItem);

                isSuccess = true;

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
                return RedirectToAction("EventInfoManage");
            }

        }
    }
}
