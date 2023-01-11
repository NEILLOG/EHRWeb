using BASE.Areas.Backend.Models;
using BASE.Areas.Backend.Models.Extend;
using BASE.Areas.Backend.Service;
using BASE.Areas.Frontend.Models.Extend;
using BASE.Extensions;
using BASE.Filters;
using BASE.Models;
using BASE.Models.DB;
using BASE.Models.Enums;
using BASE.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using NPOI.HPSF;
using NPOI.SS.Formula.Functions;
using Quartz.Impl.AdoJobStore;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;
using static Humanizer.In;
using static NuGet.Packaging.PackagingConstants;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.CompilerServices.RuntimeHelpers;


namespace BASE.Areas.Backend.Controllers
{
    public class EventController : BaseController
    {
        private readonly AllCommonService _allCommonService;
        private readonly CommonService _commonService;
        private readonly EventService _eventService;
        private readonly FileService _fileService;
        private readonly ExportService _exportService;
        private readonly MailService _mailService;
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _contextAccessor = null!;

        public EventController(AllCommonService allCommonService,
            FileService fileService,
            CommonService commonService,
            EventService eventService,
            ExportService exportService,
            MailService mailService,
            IConfiguration configuration,
            IHttpContextAccessor contextAccessor)
        {
            _allCommonService = allCommonService;
            _commonService = commonService;
            _fileService = fileService;
            _eventService = eventService;
            _exportService = exportService;
            _mailService = mailService;
            _config = configuration;
            _contextAccessor = contextAccessor;
        }

        /// <summary>
        /// 活動訊息管理
        /// </summary>
        /// <returns></returns>
        [BackendCheckLogin("Menu000015", "ENABLED")]
        public async Task<IActionResult> EventInfoManage(VM_Event data)
        {
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "活動訊息管理", Action = "檢視";

            try
            {
                // 群組ID
                data.GroupId = userinfo.GroupID;

                // 下拉_類型
                data.ddlCategory = _eventService.SetDDL_Category(1);

                //取資料
                List<EventInfoExtend>? dataList = _eventService.GetEventInfoExtendList(ref _message, data.Search);

                //分頁
                if (dataList != null)
                    data.EventInfoList = await PagerInfoService.GetRange(dataList.OrderByDescending(x=>x.activity.RegStartDate).AsQueryable(), data.Search.PagerInfo);

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
        /// 活動訊息新增
        /// </summary>
        /// <returns></returns>
        [BackendCheckLogin("Menu000015", "ADD")]
        public async Task<IActionResult> EventInfoAdd()
        {
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "新增活動", Action = "檢視";

            VM_Event data = new VM_Event();

            // 下拉_問卷
            data.ddlQuiz = _eventService.SetDDL_quiz(1);

            // 下拉_活動參與模式
            data.ddlEventType = _eventService.SetDDL_eventType(0);

            await _commonService.OperateLog(userinfo.UserID, Feature, Action);
            return View(data);
        }

        /// <summary>
        /// [POST]活動訊息新增_存檔
        /// </summary>
        /// <returns></returns>
        [BackendCheckLogin("Menu000015", "ADD")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EventInfoAdd(VM_Event datapost)
        {
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "新增活動", Action = "新增";

            // 最終動作成功與否
            bool isSuccess = false;
            
            // 例外錯誤發生，特別記錄至 TbLog
            bool unCaughtError = false;

            DateTime dtnow = DateTime.Now;

            TbActivity? item = new TbActivity();
            List<TbActivitySection>? listSection = new List<TbActivitySection>();

            try 
            {
                if (datapost.EventInfoItem == null)
                {
                    TempData["TempMsg"] = "資料回傳有誤，請重新操作！";
                }
                else
                {
                    /* 配置 key */
                    item.Id = await _allCommonService.IDGenerator<TbActivity>();
                    item.Category = datapost.EventInfoItem.activity.Category;
                    item.Qid = datapost.EventInfoItem.activity.Qid;
                    item.RegStartDate = datapost.EventInfoItem.activity.RegStartDate;
                    item.RegEndDate = datapost.EventInfoItem.activity.RegEndDate.AddHours(23).AddMinutes(59).AddSeconds(59);
                    item.Title = datapost.EventInfoItem.activity.Title;
                    item.Subject = datapost.EventInfoItem.activity.Subject;
                    item.DateType = datapost.EventInfoItem.activity.DateType;
                    item.LecturerInfo = datapost.EventInfoItem.activity.LecturerInfo;
                    item.Description = datapost.EventInfoItem.activity.Description;
                    item.Quota = datapost.EventInfoItem.activity.Quota;
                    item.RegisterFor = datapost.EventInfoItem.activity.RegisterFor;
                    item.Place = datapost.EventInfoItem.activity.Place;
                    item.IsDelete = false;  // 預設:非刪除
                    item.IsPublish = false;  // 預設:報名開關為off
                    item.IsValid = false;   // 預設:未審核
                    item.CreateUser = userinfo.UserID;
                    item.CreateDate = dtnow;

                    // 活動報名場次
                    if (datapost.EventInfoItem.sectionExtendList != null && datapost.EventInfoItem.sectionExtendList.Any())
                    {
                        foreach (var itemSection in datapost.EventInfoItem.sectionExtendList.Where(x=> !x.isRemove))
                        {
                            TbActivitySection newSection = new TbActivitySection();
                            newSection.ActivityId = item.Id;
                            newSection.Day = itemSection.sectionDay.Date;
                            newSection.StartTime = itemSection.startTime.TimeOfDay;
                            newSection.EndTime = itemSection.endTime.TimeOfDay;
                            newSection.SectionType = itemSection.sectionType;
                            listSection.Add(newSection);
                        }
                    }

                    using (var transaction = _eventService.GetTransaction())
                    {
                        try {
                            //活動圖片
                            if (datapost.ActivityImageFile != null)
                            {
                                var photo_upload = await _fileService.FileUploadAsync(datapost.ActivityImageFile, "Activity/" + item.Id, "ActivityPhoto", item.ActivityImage, null,transaction);
                                if (photo_upload.IsSuccess == true && !string.IsNullOrEmpty(photo_upload.FileID))
                                {
                                    item.ActivityImage = photo_upload.FileID;
                                }
                                else
                                {
                                    _message += photo_upload.Message;
                                }
                            }

                            // 壓縮檔for實體
                            if (datapost.EntityFile != null)
                            {
                                var entityFile_upload = await _fileService.FileUploadAsync(datapost.EntityFile, "Activity/" + item.Id, "EntityFile", item.FileForEntity, null, transaction);
                                if (entityFile_upload.IsSuccess == true && !string.IsNullOrEmpty(entityFile_upload.FileID))
                                {
                                    item.FileForEntity = entityFile_upload.FileID;
                                }
                                else
                                {
                                    _message += entityFile_upload.Message;
                                }
                            }

                            // 壓縮檔for線上
                            if (datapost.OnlineFile != null)
                            {
                                var onlineFile_upload = await _fileService.FileUploadAsync(datapost.OnlineFile, "Activity/" + item.Id, "OnlineFile", item.FileForOnline, null, transaction);
                                if (onlineFile_upload.IsSuccess == true && !string.IsNullOrEmpty(onlineFile_upload.FileID))
                                {
                                    item.FileForOnline = onlineFile_upload.FileID;
                                }
                                else
                                {
                                    _message += onlineFile_upload.Message;
                                }
                            }

                            // 上傳講義
                            if (datapost.HandoutFile != null)
                            {
                                var handoutFile_upload = await _fileService.FileUploadAsync(datapost.HandoutFile, "Activity/" + item.Id, "HandoutFile", item.HandoutFile, null, transaction);
                                if (handoutFile_upload.IsSuccess == true && !string.IsNullOrEmpty(handoutFile_upload.FileID))
                                {
                                    item.HandoutFile = handoutFile_upload.FileID;
                                }
                                else
                                {
                                    _message += handoutFile_upload.Message;
                                }
                            }

                            //新增
                            await _eventService.Insert(item, transaction);

                            // 判斷前端是否有新增經歷// 判斷前端是否有新增經歷
                            if (listSection != null && listSection.Count > 0)
                                await _eventService.InsertRange(listSection, transaction);
                            
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
                return RedirectToAction("EventInfoManage");
            }
            else
            {
                if (item == null || item == null)
                {
                    return RedirectToAction("EventInfoManage");
                }
                else
                {
                    /* 失敗回原頁 */
                    return View(datapost);
                }
            }
        }

        /// <summary>
        /// 活動訊息編輯
        /// </summary>
        /// <returns></returns>
        [BackendCheckLogin("Menu000015", "MODIFY")]
        public async Task<IActionResult> EventInfoEdit(string id)
        {
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "編輯活動訊息", Action = "檢視";

            VM_Event data = new VM_Event();

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
                else {
                    // 下拉_問卷
                    data.ddlQuiz = _eventService.SetDDL_quiz(1);

                    // 下拉_活動參與模式
                    data.ddlEventType = _eventService.SetDDL_eventType(0);

                    EventInfoExtend? dataList = _eventService.GetEventInfoExtendItem(ref _message, decrypt_id);
                    if (dataList != null)
                    {
                        data.EventInfoItem = dataList;
                    }
                    if (data.EventInfoItem == null)
                    {
                        TempData["TempMsgDetail"] = "查無指定項目！";
                    }
                    else
                    {
                        isSuccess = true;
                    }
                }
            } catch (Exception ex) {
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

                return RedirectToAction("EventInfoManage");
            }
        }

        /// <summary>
        /// [POST]活動訊息編輯_存檔
        /// </summary>
        /// <returns></returns>
        [BackendCheckLogin("Menu000015", "MODIFY")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EventInfoEdit(string id,VM_Event datapost)
        {
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "活動訊息編輯", Action = "編輯";

            // 最終動作成功與否
            bool isSuccess = false;

            // 例外錯誤發生，特別記錄至 TbLog
            bool unCaughtError = false;

            DateTime dtnow = DateTime.Now;

            string decrypt_id = EncryptService.AES.RandomizedDecrypt(id);

            TbActivity? item = null;
            List<TbActivitySection>? listActivitySection = new List<TbActivitySection>();

            try
            {
                if (string.IsNullOrEmpty(decrypt_id))
                {
                    TempData["TempMsgDetail"] = "金鑰逾時！請重新再操作一次！";
                }
                else
                {
                    IQueryable<TbActivity>? temp = _eventService.Lookup<TbActivity>(ref _message, x => x.Id == decrypt_id && x.IsDelete == false);
                    if (temp != null)
                        item = await temp.SingleOrDefaultAsync();
                    if (item == null)
                    {
                        TempData["TempMsgDetail"] = "查無指定項目！";
                    }
                    else if (datapost.EventInfoItem == null)
                    {
                        TempData["TempMsgDetail"] = "資料回傳有誤，請重新操作！";
                    }
                    else 
                    {
                        // 尚未審核通過方可編輯
                        if (!item.IsValid)
                        {
                            item.Category = datapost.EventInfoItem.activity.Category;
                            item.Qid = datapost.EventInfoItem.activity.Qid;
                            item.RegStartDate = datapost.EventInfoItem.activity.RegStartDate;
                            item.RegEndDate = datapost.EventInfoItem.activity.RegEndDate.AddHours(23).AddMinutes(59).AddSeconds(59);
                            item.Title = datapost.EventInfoItem.activity.Title;
                            item.Subject = datapost.EventInfoItem.activity.Subject;
                            item.DateType = datapost.EventInfoItem.activity.DateType;
                            item.LecturerInfo = datapost.EventInfoItem.activity.LecturerInfo;
                            item.Description = datapost.EventInfoItem.activity.Description;
                            item.Quota = datapost.EventInfoItem.activity.Quota;
                            item.RegisterFor = datapost.EventInfoItem.activity.RegisterFor;
                            item.Place = datapost.EventInfoItem.activity.Place;

                            // 活動場次，尚未開放報名的場次才可編輯
                            if (!item.IsPublish && datapost.EventInfoItem.sectionExtendList != null && datapost.EventInfoItem.sectionExtendList.Any())
                            {

                                foreach (var itemUserEXP in datapost.EventInfoItem.sectionExtendList.Where(x=>!x.isRemove))
                                {
                                    TbActivitySection tempActivitySection = new TbActivitySection();
                                    tempActivitySection.ActivityId = item.Id;
                                    tempActivitySection.Day = itemUserEXP.sectionDay;
                                    tempActivitySection.StartTime = itemUserEXP.startTime.TimeOfDay;
                                    tempActivitySection.EndTime = itemUserEXP.endTime.TimeOfDay;
                                    tempActivitySection.SectionType = itemUserEXP.sectionType;
                                    listActivitySection.Add(tempActivitySection);
                                }
                            }
                        }
                        item.ModifyDate = dtnow;
                        item.ModifyUser = userinfo.UserID;

                        using (var transaction = _eventService.GetTransaction())
                        {
                            try
                            {
                                // 尚未審核通過方可編輯
                                if (!item.IsValid)
                                {
                                    //活動圖片
                                    if (datapost.ActivityImageFile != null)
                                    {
                                        var photo_upload = await _fileService.FileUploadAsync(datapost.ActivityImageFile, "Activity/" + item.Id, "ActivityPhoto", item.ActivityImage, null, transaction);
                                        if (photo_upload.IsSuccess == true && !string.IsNullOrEmpty(photo_upload.FileID))
                                        {
                                            item.ActivityImage = photo_upload.FileID;
                                        }
                                        else
                                        {
                                            _message += photo_upload.Message;
                                        }
                                    }

                                    // 壓縮檔for實體
                                    if (datapost.EntityFile != null)
                                    {
                                        var entityFile_upload = await _fileService.FileUploadAsync(datapost.EntityFile, "Activity/" + item.Id, "EntityFile", item.FileForEntity, null, transaction);
                                        if (entityFile_upload.IsSuccess == true && !string.IsNullOrEmpty(entityFile_upload.FileID))
                                        {
                                            item.FileForEntity = entityFile_upload.FileID;
                                        }
                                        else
                                        {
                                            _message += entityFile_upload.Message;
                                        }
                                    }

                                    // 壓縮檔for線上
                                    if (datapost.OnlineFile != null)
                                    {
                                        var onlineFile_upload = await _fileService.FileUploadAsync(datapost.OnlineFile, "Activity/" + item.Id, "OnlineFile", item.FileForOnline, null, transaction);
                                        if (onlineFile_upload.IsSuccess == true && !string.IsNullOrEmpty(onlineFile_upload.FileID))
                                        {
                                            item.FileForOnline = onlineFile_upload.FileID;
                                        }
                                        else
                                        {
                                            _message += onlineFile_upload.Message;
                                        }
                                    }

                                    // 尚未開放報名才可編輯場次編輯
                                    if (!item.IsPublish)
                                    {
                                        // 刪除活動時段
                                        List<TbActivitySection> oriActivitySectionList = new List<TbActivitySection>();
                                        oriActivitySectionList = _eventService.Lookup<TbActivitySection>(ref _message, x => x.ActivityId == item.Id).ToList();
                                        // 先清除原先經歷
                                        if (oriActivitySectionList != null && oriActivitySectionList.Any())
                                            await _eventService.DeleteRange(oriActivitySectionList, transaction);
                                        // 重新新增經歷
                                        if (listActivitySection != null && listActivitySection.Any())
                                            await _eventService.InsertRange(listActivitySection, transaction);
                                    }
                                }
                                else {
                                    // 尚未審核通過-上傳講義
                                    if (datapost.HandoutFile != null)
                                    {
                                        var handoutFile_upload = await _fileService.FileUploadAsync(datapost.HandoutFile, "Activity/" + item.Id, "HandoutFile", item.HandoutFile, null, transaction);
                                        if (handoutFile_upload.IsSuccess == true && !string.IsNullOrEmpty(handoutFile_upload.FileID))
                                        {
                                            item.HandoutFile = handoutFile_upload.FileID;
                                        }
                                        else
                                        {
                                            _message += handoutFile_upload.Message;
                                        }
                                    }
                                }
                                
                                //編輯
                                await _eventService.Update(item, transaction);

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
            {
                return RedirectToAction("EventInfoManage");
            }
            else
            {
                if (item == null)
                {
                    return RedirectToAction("EventInfoManage");
                }
                else
                {
                    /* 失敗回原頁 */
                    return View(datapost);
                }
            }

        }

        /// <summary>
        /// 刪除活動訊息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [BackendCheckLogin("Menu000015", "DELETE")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EventInfoDelete(string id)
        {
            JsonResponse<NtuFaq> result = new JsonResponse<NtuFaq>();
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "活動訊息管理", Action = "刪除";

            DateTime dtnow = DateTime.Now;

            // 最終動作成功與否
            bool isSuccess = false;

            // 例外錯誤發生，特別記錄至 TbLog
            bool unCaughtError = false;

            string decrypt_id = EncryptService.AES.RandomizedDecrypt(id);

            try
            {
                if (string.IsNullOrEmpty(decrypt_id))
                {
                    result.MessageDetail = "金鑰逾時！請重新再操作一次！";
                }
                else
                {
                    TbActivity? item = null;
                    IQueryable<TbActivity>? temp = _eventService.Lookup<TbActivity>(ref _message, x => x.Id == decrypt_id && x.IsDelete == false);
                    if (temp != null)
                        item = await temp.SingleOrDefaultAsync();

                    if (item == null)
                    {
                        TempData["TempMsgDetail"] = "查無指定項目！";
                    }
                    else
                    {
                        item.IsDelete = true;
                        item.ModifyUser = userinfo.UserID;
                        item.ModifyDate = dtnow;

                        using (var transaction = _eventService.GetTransaction())
                        {
                            try
                            {
                                //編輯
                                await _eventService.Update(item, transaction);

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
                result.Message = "刪除成功";
            }
            else
            {
                result.alert_type = "error";
                result.Message = result.Message ?? "刪除失敗";

                if (unCaughtError)
                {
                    await _allCommonService.Error_Record("Backend", Feature + "-" + Action, _message);
                }
            }

            //操作紀錄
            string response = result.Message + "\r\n" + result.MessageDetail;
            await _commonService.OperateLog(userinfo.UserID, Feature, Action, decrypt_id, id, _message, response, isSuccess);

            return Json(result);
        }

        /// <summary>
        /// 報名開關切換
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [BackendCheckLogin("Menu000015", "MODIFY")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PublicSwitch(string id)
        {
            JsonResponse<NtuFaq> result = new JsonResponse<NtuFaq>();
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "活動訊息管理", Action = "編輯";

            DateTime dtnow = DateTime.Now;

            // 最終動作成功與否
            bool isSuccess = false;

            // 例外錯誤發生，特別記錄至 TbLog
            bool unCaughtError = false;


            try
            {
                TbActivity? item = null;
                IQueryable<TbActivity>? temp = _eventService.Lookup<TbActivity>(ref _message, x => x.Id == id && x.IsDelete == false);
                if (temp != null)
                    item = await temp.SingleOrDefaultAsync();

                if (item == null)
                {
                    TempData["TempMsgDetail"] = "查無指定項目！";
                }
                else
                {
                    item.IsPublish = !item.IsPublish;
                    item.ModifyUser = userinfo.UserID;
                    item.ModifyDate = dtnow;

                    using (var transaction = _eventService.GetTransaction())
                    {
                        try
                        {
                            //編輯
                            await _eventService.Update(item, transaction);

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
            catch (Exception ex)
            {
                result.Message = "伺服器連線異常，請檢查您的網路狀態後再試一次！";

                _message += ex.ToString();
                unCaughtError = true;
            }

            if (isSuccess)
            {
                result.alert_type = "success";
                result.Message = "編輯成功";
            }
            else
            {
                result.alert_type = "error";
                result.Message = result.Message ?? "編輯失敗";

                if (unCaughtError)
                {
                    await _allCommonService.Error_Record("Backend", Feature + "-" + Action, _message);
                }
            }

            //操作紀錄
            string response = result.Message + "\r\n" + result.MessageDetail;
            await _commonService.OperateLog(userinfo.UserID, Feature, Action, id, id, _message, response, isSuccess);

            return Json(result);
        }

        /// <summary>
        /// 審核活動訊息內容
        /// </summary>
        /// <returns></returns>
        [BackendCheckLogin("Menu000019", "MODIFY")]
        public async Task<IActionResult> EventInfoAudit(string id)
        {
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "", Action = "檢視";

            VM_Event data = new VM_Event();

            // 最終動作成功與否
            bool isSuccess = false;

            // 例外錯誤發生，特別記錄至 TbLog
            bool unCaughtError = false;

            string decrypt_id = EncryptService.AES.RandomizedDecrypt(id);

            try
            {
                if (string.IsNullOrEmpty(decrypt_id))
                {
                    TempData["TempMsgDetail"] = "金鑰逾時！請重新再操作一次！";
                }
                else
                {
                    // 下拉_問卷
                    data.ddlQuiz = _eventService.SetDDL_quiz(1);

                    // 下拉_活動參與模式
                    data.ddlEventType = _eventService.SetDDL_eventType(0);

                    EventInfoExtend? dataList = _eventService.GetEventInfoExtendItem(ref _message, decrypt_id);
                    if (dataList != null)
                    {
                        data.EventInfoItem = dataList;
                    }
                    if (data.EventInfoItem == null)
                    {
                        TempData["TempMsgDetail"] = "查無指定項目！";
                    }
                    else
                    {
                        isSuccess = true;
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

                return RedirectToAction("EventInfoManage");
            }
        }

        /// <summary>
        /// [POST]審核活動訊息內容_存檔
        /// </summary>
        /// <returns></returns>
        [BackendCheckLogin("Menu000019", "MODIFY")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EventInfoAudit(string id, VM_Event datapost)
        {
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "審核活動訊息", Action = "編輯";

            // 最終動作成功與否
            bool isSuccess = false;

            // 例外錯誤發生，特別記錄至 TbLog
            bool unCaughtError = false;

            DateTime dtnow = DateTime.Now;

            string decrypt_id = EncryptService.AES.RandomizedDecrypt(id);

            TbActivity? item = null;

            try
            {
                if (string.IsNullOrEmpty(decrypt_id))
                {
                    TempData["TempMsgDetail"] = "金鑰逾時！請重新再操作一次！";
                }
                else
                {
                    IQueryable<TbActivity>? temp = _eventService.Lookup<TbActivity>(ref _message, x => x.Id == decrypt_id && x.IsDelete == false);
                    if (temp != null)
                        item = await temp.SingleOrDefaultAsync();
                    if (item == null)
                    {
                        TempData["TempMsgDetail"] = "查無指定項目！";
                    }
                    else if (datapost.EventInfoItem == null)
                    {
                        TempData["TempMsgDetail"] = "資料回傳有誤，請重新操作！";
                    }
                    else
                    {
                        item.IsValid = true; // 審核通過
                        item.IsPublish = true; // 報名開關同時on
                        item.ModifyUser = userinfo.UserID;
                        item.ModifyDate = dtnow;

                        using (var transaction = _eventService.GetTransaction())
                        {
                            try
                            {
                                //編輯
                                await _eventService.Update(item, transaction);
                                
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
                TempData["TempMsg"] = "審核成功";
            }
            else
            {
                TempData["TempMsgType"] = MsgTypeEnum.error;
                TempData["TempMsg"] = TempData["TempMsg"] ?? "審核失敗";

                if (unCaughtError)
                {
                    await _allCommonService.Error_Record("Backend", Feature + "-" + Action, _message);
                }
            }

            //操作紀錄
            string response = (TempData["TempMsg"] == null ? "" : TempData["TempMsg"].ToString()) + "\r\n" + (TempData["TempMsgDetail"] == null ? "" : TempData["TempMsgDetail"].ToString());
            await _commonService.OperateLog(userinfo.UserID, Feature, Action, decrypt_id, datapost, _message, response, isSuccess);

            if (isSuccess)
            {
                return RedirectToAction("EventInfoManage");
            }
            else
            {
                if (item == null)
                {
                    return RedirectToAction("EventInfoManage");
                }
                else
                {
                    /* 失敗回原頁 */
                    return View(datapost);
                }
            }

        }

        #region 報名名單

        /// <summary>
        /// 活動報名
        /// </summary>
        /// <param name="id">活動訊息主表ID</param>
        /// <returns></returns>
        [BackendCheckLogin("Menu000016", "ENABLED")]
        public async Task<IActionResult> EventInfoRegistrationList(string id, VM_Event data)
        {
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "活動報名名單", Action = "檢視";

            // 最終動作成功與否
            bool isSuccess = false;

            // 例外錯誤發生，特別記錄至 TbLog
            bool unCaughtError = false;

            DateTime dtnow = DateTime.Now;

            string decrypt_id = EncryptService.AES.RandomizedDecrypt(id);

            List<TbActivitySection>? listActivitySection = new List<TbActivitySection>();

            try {
                if (string.IsNullOrEmpty(decrypt_id))
                {
                    TempData["TempMsgDetail"] = "金鑰逾時！請重新再操作一次！";
                }
                else
                {
                    IQueryable<TbActivity>? temp = _eventService.Lookup<TbActivity>(ref _message, x => x.Id == decrypt_id && x.IsDelete == false);
                    if (temp != null)
                        data.ActivityItem = await temp.SingleOrDefaultAsync();
                    if (data.ActivityItem == null)
                    {
                        TempData["TempMsgDetail"] = "查無指定項目！";
                    }
                    else
                    {
                        // 下拉_活動時段
                        data.ddlSection = _eventService.SetDDL_section(0, data.ActivityItem.Id);

                        // 下拉_審核
                        data.ddlVerify = _eventService.SetDDL_VerifyStatus();

                        // 查詢條件綁定-活動日期
                        data.Search.sSection = string.IsNullOrEmpty(data.Search.sSection) ?  data.ddlSection.FirstOrDefault().Value : data.Search.sSection;
                        // 查詢條件綁定-活動主表ID
                        data.Search.activityId = data.ActivityItem.Id;

                        //取資料
                        List<RegistrationExtend>? dataList = _eventService.GetRegistrationExtendList(ref _message, data.Search);

                        //分頁
                        if (dataList != null)
                            data.RegistrationExtendList = await PagerInfoService.GetRange(dataList.AsQueryable(), data.Search.PagerInfo);

                        isSuccess = true;
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

                return RedirectToAction("EventInfoManage");
            }
        }

        /// <summary>
        /// 報名企業詳細資訊
        /// </summary>
        /// <returns></returns>
        [BackendCheckLogin("Menu000016", "ENABLED")]
        public async Task<IActionResult> EventInfoRegistrationDetail(string id) 
        {
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "報名企業詳細資訊", Action = "檢視";

            VM_Event data = new VM_Event();

            // 最終動作成功與否
            bool isSuccess = false;

            // 例外錯誤發生，特別記錄至 TbLog
            bool unCaughtError = false;

            DateTime dtnow = DateTime.Now;

            string decrypt_id = EncryptService.AES.RandomizedDecrypt(id);

            RegistrationExtend? registrationExtend = new RegistrationExtend();

            try {
                if (string.IsNullOrEmpty(decrypt_id))
                {
                    TempData["TempMsgDetail"] = "金鑰逾時！請重新再操作一次！";
                }
                else 
                {
                    IQueryable<RegistrationExtend>? temp = _eventService.GetRegistrationExtendItem(ref _message, decrypt_id);
                    if (temp != null)
                        data.RegistrationExtendItem = await temp.FirstOrDefaultAsync();

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

                return RedirectToAction("EventInfoManage");
            }
        }

        /// <summary>
        /// 報名名單_查看更多表單內容
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [BackendCheckLogin("Menu000016", "ENABLED")]
        public async Task<IActionResult> EventInfoRegistrationMore(string id)
        {
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "報名名單查看更多表單內容", Action = "檢視";

            VM_Event data = new VM_Event();

            // 最終動作成功與否
            bool isSuccess = false;

            // 例外錯誤發生，特別記錄至 TbLog
            bool unCaughtError = false;

            DateTime dtnow = DateTime.Now;

            string decrypt_id = EncryptService.AES.RandomizedDecrypt(id);

            try
            {
                if (string.IsNullOrEmpty(decrypt_id))
                {
                    TempData["TempMsgDetail"] = "金鑰逾時！請重新再操作一次！";
                }
                else
                {
                    IQueryable<RegistrationExtend>? temp = _eventService.GetRegistrationExtendItem(ref _message, decrypt_id);
                    if (temp != null) { 
                        data.RegistrationExtendItem = await temp.SingleOrDefaultAsync();

                        // 是否填寫問卷?
                        data.isFillQuiz = _eventService.Lookup<TbActivityQuizResponse>(ref _message, x => x.RegisterId == data.RegistrationExtendItem.register.Id).Any();
                    }

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

                return RedirectToAction("EventInfoManage");
            }
        }

        /// <summary>
        /// 全數審核通過
        /// </summary>
        /// <param name="id">活動ID</param>
        /// <returns></returns>
        [BackendCheckLogin("Menu000016", "MODIFY")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AllApproved(string id,string sectioniId)
        {
            JsonResponse<TbActivityRegister> result = new JsonResponse<TbActivityRegister>();
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "活動報名名單管理", Action = "全部審核";

            DateTime dtnow = DateTime.Now;

            // 最終動作成功與否
            bool isSuccess = false;

            // 例外錯誤發生，特別記錄至 TbLog
            bool unCaughtError = false;

            TbActivity? activity = new TbActivity();
            TbActivitySection? activitySection = new TbActivitySection();
            List<TbActivityRegister>? activityRegister = new List<TbActivityRegister>();
            List<long>? listSuccess = new List<long>();
            List<long>? listReserve = new List<long>();

            try
            {
                if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(sectioniId))
                {
                    result.MessageDetail = "金鑰逾時！請重新再操作一次！";
                }
                else
                {
                    VM_EventQueryParam searchParam = new VM_EventQueryParam();
                    searchParam.activityId = id;
                    searchParam.sSection = sectioniId;

                    // 活動主表
                    activity = _eventService.Lookup<TbActivity>(ref _message, x => x.Id == id).FirstOrDefault();
                    // 活動時段表
                    long longSection = Convert.ToInt64(sectioniId);
                    activitySection = _eventService.Lookup<TbActivitySection>(ref _message, x => x.Id == longSection).FirstOrDefault();

                    //取資料
                    List<RegistrationExtend>? dataList = _eventService.GetRegistrationExtendList(ref _message, searchParam);

                    if (dataList == null)
                    {
                        TempData["TempMsgDetail"] = "查無指定項目！";
                    }
                    else
                    {
                        foreach (var item in dataList)
                        {
                            if (item.register.IsValid.HasValue && item.register.IsValid.Value)
                            {
                                continue;
                            }
                            else
                            {
                                TbActivityRegister temp = new TbActivityRegister();
                                temp = item.register;
                                // 寄發成功通知信
                                if (!temp.IsValid.HasValue)
                                {
                                    listSuccess.Add(temp.Id);
                                }

                                // 寄發備取通知信
                                if (temp.IsValid.HasValue && temp.IsValid.Value)
                                {
                                    listReserve.Add(temp.Id);
                                }
                                temp.IsValid = true;
                                temp.ModifyDate = dtnow;
                                temp.ModifyUser = userinfo.UserID;
                                activityRegister.Add(temp);
                            }
                        }

                        using (var transaction = _eventService.GetTransaction())
                        {
                            try
                            {
                                //編輯
                                await _eventService.UpdateRange(activityRegister, transaction);

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
                if (activityRegister != null && activityRegister.Any())
                {
                    string actMonth = activitySection.Day.Month.ToString();
                    string actDay = activitySection.Day.Day.ToString();

                    // 彙管承辦信箱
                    TbUserInfo Creater = _eventService.Lookup<TbUserInfo>(ref _message, x => x.UserId == activity.CreateUser).FirstOrDefault();

                    //-- 審查成功通知信
                    if (listSuccess != null && listSuccess.Any())
                    {
                        foreach (var itemSuccess in activityRegister.Where(x => listSuccess.Contains(x.Id)))
                        {
                            // 主旨
                            string sSubject = string.Concat("勞動部勞動力發展署桃竹苗分署-", actMonth, "月", actDay, "日", activity.Title, "-", activity.Subject, "報名成功通知信(本郵件由系統自動寄發，請問直接回覆此郵件)");

                            //內容
                            string sContent = string.Concat(itemSuccess.Name, "您好<br />");
                            sContent += string.Concat("您已成功報名", actMonth, "月", actDay, "日 「", activity.Title, "-", activity.Subject, "」，本活動資訊如下：<br />");
                            sContent += string.Concat("活動主題：「", activity.Subject, "」<br />");
                            sContent += string.Concat("活動參與方式：", activitySection.SectionType, "<br />");
                            sContent += string.Concat("活動時間：", actMonth, "月", actDay, "日", activitySection.StartTime, '-', activitySection.EndTime, "【活動將於開始前30分鐘開放報到】<br />");
                            sContent += string.Concat("活動地點：", activity.Place, "<br /><br />");
                            sContent += String.Format("※提醒您，活動當天請準時出席，若臨時有事不克參與請於活動開始前3天回信至{0}或來電取消報名<br /><br />", Creater.Email);
                            sContent += "敬祝 順心平安<br />";
                            sContent += "勞動部勞動力發展署桃竹苗分署<br />";
                            sContent += "桃竹苗區域運籌人力資源整合服務計畫_專案辦公室<br />";
                            sContent += "諮詢電話： 02-23660812 #164、#127 03-4855368#1905";

                            //寄送預約信件
                            await _mailService.ReserveSendEmail(new MailViewModel()
                            {
                                ToList = new List<MailAddressInfo>() { new MailAddressInfo(itemSuccess.Email) },
                                Subject = sSubject,
                                Body = sContent
                            }, userinfo.UserID, DateTime.Now, "ActivityPass");
                        }
                    }

                    //-- 審查備取成功通知信
                    if (listReserve != null && listReserve.Any())
                    {
                        foreach (var itemReserve in activityRegister.Where(x => listReserve.Contains(x.Id)))
                        {
                            // 主旨
                            string sSubject = string.Concat("勞動部勞動力發展署桃竹苗分署-", actMonth, "月", actDay, "日", activity.Title, "-", activity.Subject, "報名備取成功通知信");

                            //內容
                            string sContent = string.Concat(itemReserve.Name, "您好<br />");
                            sContent += string.Concat("感謝您報名", actMonth, "月", actDay, "日 「", activity.Title, "-", activity.Subject, "」，因活動名額釋出，在此通知您備取成功，");
                            sContent += "敬請於活動當天準時出席，本活動資訊如下：<br />";
                            sContent += string.Concat("活動主題：「", activity.Subject, "」<br />");
                            sContent += string.Concat("活動參與方式：", activitySection.SectionType, "<br />");
                            sContent += string.Concat("活動時間：", actMonth, "月", actDay, "日", activitySection.StartTime, '-', activitySection.EndTime, "【活動將於開始前30分鐘開放報到】<br />");
                            sContent += string.Concat("活動地點：", activity.Place, "<br /><br />");

                            sContent += "敬祝 順心平安<br />";
                            sContent += "勞動部勞動力發展署桃竹苗分署<br />";
                            sContent += "桃竹苗區域運籌人力資源整合服務計畫_專案辦公室<br />";
                            sContent += "諮詢電話： 02-23660812 #164、#127 03-4855368#1905";

                            //寄送預約信件
                            await _mailService.ReserveSendEmail(new MailViewModel()
                            {
                                ToList = new List<MailAddressInfo>() { new MailAddressInfo(itemReserve.Email) },
                                Subject = sSubject,
                                Body = sContent
                            }, userinfo.UserID, DateTime.Now, "ActivityReserve");
                        }
                    }
                }

                result.alert_type = "success";
                result.Message = "全數審核成功";
            }
            else
            {
                result.alert_type = "error";
                result.Message = result.Message ?? "編輯失敗";

                if (unCaughtError)
                {
                    await _allCommonService.Error_Record("Backend", Feature + "-" + Action, _message);
                }
            }

            //操作紀錄
            string response = result.Message + "\r\n" + result.MessageDetail;
            await _commonService.OperateLog(userinfo.UserID, Feature, Action, id, id, _message, response, isSuccess);

            return Json(result);
        }

        /// <summary>
        /// 批次儲存審核結果
        /// </summary>
        /// <returns></returns>
        [BackendCheckLogin("Menu000016", "MODIFY")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeVerifyStatus(List<string> arrRegistration, List<string> verifyStatus,string activityID, string sectionID)
        {
            JsonResponse<TbActivityRegister> result = new JsonResponse<TbActivityRegister>();
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "活動報名名單管理", Action = "批次審核";
            
            DateTime dtnow = DateTime.Now;

            // 最終動作成功與否
            bool isSuccess = false;

            // 例外錯誤發生，特別記錄至 TbLog
            bool unCaughtError = false;

            TbActivity? activity = new TbActivity();
            TbActivitySection? activitySection = new TbActivitySection();
            List<TbActivityRegister>? activityRegister = new List<TbActivityRegister>();
            List<long>? listSuccess = new List<long>();
            List<long>? listFail = new List<long>();
            List<long>? listReserve = new List<long>();

            try
            {
                if (arrRegistration == null || arrRegistration == null)
                {
                    result.MessageDetail = "金鑰逾時！請重新再操作一次！";
                }
                else
                {
                    // 活動主表
                    activity = _eventService.Lookup<TbActivity>(ref _message, x => x.Id == activityID).FirstOrDefault();
                    // 活動時段表
                    long longSection = Convert.ToInt64(sectionID);
                    activitySection = _eventService.Lookup<TbActivitySection>(ref _message, x => x.Id == longSection).FirstOrDefault();

                    //取資料
                    List<TbActivityRegister>? dataList = _eventService.Lookup<TbActivityRegister>(ref _message, x => arrRegistration.Contains(x.Id.ToString())).ToList();

                    if (dataList == null)
                    {
                        TempData["TempMsgDetail"] = "查無指定項目！";
                    }
                    else
                    {
                        for (int i = 0; i < arrRegistration.Count; i++)
                        {
                            bool? changeStatus = verifyStatus[i] != "-1" ? (verifyStatus[i] == "1" ? true : false) : null;
                            TbActivityRegister temp = dataList.Where(x => x.Id.ToString() == arrRegistration[i]).FirstOrDefault();
                            if (temp.IsValid != changeStatus)
                            {
                                // 寄發成功通知信
                                if (!temp.IsValid.HasValue && (changeStatus.HasValue && changeStatus.Value))
                                {
                                    listSuccess.Add(temp.Id);
                                }

                                // 寄發失敗通知信
                                if (!temp.IsValid.HasValue && (changeStatus.HasValue && !changeStatus.Value))
                                {
                                    listFail.Add(temp.Id);
                                }

                                // 寄發備取通知信
                                if ((temp.IsValid.HasValue && !temp.IsValid.Value) && (changeStatus.HasValue && changeStatus.Value))
                                {
                                    listReserve.Add(temp.Id);
                                }

                                temp.IsValid = changeStatus;
                                temp.ModifyDate = dtnow;
                                temp.ModifyUser = userinfo.UserID;
                                activityRegister.Add(temp);
                            }
                            else {
                                continue;
                            }
                        }

                        using (var transaction = _eventService.GetTransaction())
                        {
                            try
                            {
                                if (activityRegister != null && activityRegister.Any())
                                {
                                    //編輯
                                    await _eventService.UpdateRange(activityRegister, transaction);

                                    transaction.Commit();
                                    isSuccess = true;
                                }
                                else
                                {
                                    result.alert_type = "error";
                                    result.Message = "無審核狀態變更";
                                }

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
                
                if (activityRegister != null && activityRegister.Any())
                {
                    string actMonth = activitySection.Day.Month.ToString();
                    string actDay = activitySection.Day.Day.ToString();

                    //-- 審查成功通知信
                    if (listSuccess != null && listSuccess.Any())
                    {
                        foreach (var itemSuccess in activityRegister.Where(x=> listSuccess.Contains(x.Id)))
                        {
                            // 主旨
                            string sSubject = string.Concat("勞動部勞動力發展署桃竹苗分署-", actMonth,"月", actDay,"日",activity.Title,"-",activity.Subject,"報名成功通知信(本郵件由系統自動寄發，請問直接回覆此郵件)");

                            //內容
                            string sContent = string.Concat(itemSuccess.Name, "您好<br />");
                            sContent += string.Concat("您已成功報名", actMonth, "月", actDay, "日 「", activity.Title, "-", activity.Subject, "」，本活動資訊如下：<br />");
                            sContent += string.Concat("活動主題：「", activity.Subject , "」<br />");
                            sContent += string.Concat("活動參與方式：", activitySection.SectionType , "<br />");
                            sContent += string.Concat("活動時間：", actMonth, "月", actDay, "日", activitySection.StartTime ,'-', activitySection.EndTime, "【活動將於開始前30分鐘開放報到】<br />");
                            sContent += string.Concat("活動地點：", activity.Place, "<br /><br />");
                            sContent += "※提醒您，活動當天請準時出席，若臨時有事不克參與請於活動開始前3天回信至000@wda.gov.tw或來電取消報名<br /><br />";
                            sContent += "敬祝 順心平安<br />";
                            sContent += "勞動部勞動力發展署桃竹苗分署<br />";
                            sContent += "桃竹苗區域運籌人力資源整合服務計畫_專案辦公室<br />";
                            sContent += "諮詢電話： 02-23660812 #164、#127 03-4855368#1905";

                            //寄送預約信件
                            await _mailService.ReserveSendEmail(new MailViewModel()
                            {
                                ToList = new List<MailAddressInfo>() { new MailAddressInfo(itemSuccess.Email) },
                                Subject = sSubject,
                                Body = sContent
                            }, userinfo.UserID, DateTime.Now, "ActivityPass");
                        }
                    }

                    //-- 審查未通過通知信
                    if (listFail != null && listFail.Any())
                    {
                        foreach (var itemFail in activityRegister.Where(x => listFail.Contains(x.Id)))
                        {
                            // 主旨
                            string sSubject = string.Concat("勞動部勞動力發展署桃竹苗分署-", actMonth, "月", actDay, "日", activity.Title, "-", activity.Subject, "報名未錄取通知信");

                            //內容
                            string sContent = string.Concat(itemFail.Name, "您好<br />");
                            sContent += string.Concat("感謝您報名", actMonth, "月", actDay, "日 「", activity.Title, "-", activity.Subject, "」，因活動名額有限，您目前為本活動候補者。");
                            sContent += "若後續有名額釋出，我們將會與您聯繫確認出席意願，如您有任何問題皆歡迎來電詢問，謝謝。<br /><br />";
                            sContent += "敬祝 順心平安<br />";
                            sContent += "勞動部勞動力發展署桃竹苗分署<br />";
                            sContent += "桃竹苗區域運籌人力資源整合服務計畫_專案辦公室<br />";
                            sContent += "諮詢電話： 02-23660812 #164、#127 03-4855368#1905";

                            //寄送預約信件
                            await _mailService.ReserveSendEmail(new MailViewModel()
                            {
                                ToList = new List<MailAddressInfo>() { new MailAddressInfo(itemFail.Email) },
                                Subject = sSubject,
                                Body = sContent
                            }, userinfo.UserID, DateTime.Now, "ActivityFail");
                        }
                    }

                    //-- 審查備取成功通知信
                    if (listReserve != null && listReserve.Any())
                    {
                        foreach (var itemReserve in activityRegister.Where(x => listReserve.Contains(x.Id)))
                        {
                            // 主旨
                            string sSubject = string.Concat("勞動部勞動力發展署桃竹苗分署-", actMonth, "月", actDay, "日", activity.Title, "-", activity.Subject, "報名備取成功通知信");

                            //內容
                            string sContent = string.Concat(itemReserve.Name, "您好<br />");
                            sContent += string.Concat("感謝您報名", actMonth, "月", actDay, "日 「", activity.Title, "-", activity.Subject, "」，因活動名額釋出，在此通知您備取成功，");
                            sContent += "敬請於活動當天準時出席，本活動資訊如下：<br />";
                            sContent += string.Concat("活動主題：「", activity.Subject, "」<br />");
                            sContent += string.Concat("活動參與方式：", activitySection.SectionType, "<br />");
                            sContent += string.Concat("活動時間：", actMonth, "月", actDay, "日", activitySection.StartTime, '-', activitySection.EndTime, "【活動將於開始前30分鐘開放報到】<br />");
                            sContent += string.Concat("活動地點：", activity.Place, "<br /><br />");

                            sContent += "敬祝 順心平安<br />";
                            sContent += "勞動部勞動力發展署桃竹苗分署<br />";
                            sContent += "桃竹苗區域運籌人力資源整合服務計畫_專案辦公室<br />";
                            sContent += "諮詢電話： 02-23660812 #164、#127 03-4855368#1905";

                            //寄送預約信件
                            await _mailService.ReserveSendEmail(new MailViewModel()
                            {
                                ToList = new List<MailAddressInfo>() { new MailAddressInfo(itemReserve.Email) },
                                Subject = sSubject,
                                Body = sContent
                            }, userinfo.UserID, DateTime.Now, "ActivityReserve");
                        }
                    }

                }

                result.alert_type = "success";
                result.Message = "審核成功";
            }
            else
            {
                result.alert_type = "error";
                result.Message = result.Message ?? "編輯失敗";

                if (unCaughtError)
                {
                    await _allCommonService.Error_Record("Backend", Feature + "-" + Action, _message);
                }
            }

            //操作紀錄
            string response = result.Message + "\r\n" + result.MessageDetail;
            await _commonService.OperateLog(userinfo.UserID, Feature, Action, null, null);

            return Json(result);
        }

        /// <summary>
        /// 簽到
        /// </summary>
        /// <returns></returns>
        [BackendCheckLogin("Menu000016", "MODIFY")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivitySignin(string registerSectionID,string type)
        {
            JsonResponse<TbActivityRegister> result = new JsonResponse<TbActivityRegister>();
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "活動報名名單管理", Action = "簽到";

            DateTime dtnow = DateTime.Now;

            // 最終動作成功與否
            bool isSuccess = false;

            // 例外錯誤發生，特別記錄至 TbLog
            bool unCaughtError = false;

            TbActivity? activity = new TbActivity();
            TbActivitySection? activitySection = new TbActivitySection();
            TbActivityRegister? register = new TbActivityRegister();
            TbActivityRegisterSection? item = new TbActivityRegisterSection();
            // 是否要寄送滿意度問卷調查
            bool isSend = false;

            // 寄件附檔
            List<Attachment>? listAttachments = new List<Attachment>();
            try
            {
                if (string.IsNullOrEmpty(registerSectionID))
                {
                    result.MessageDetail = "金鑰逾時！請重新再操作一次！";
                }
                else
                {
                    //取資料
                    long RSId = Convert.ToInt64(registerSectionID);
                    TbActivityRegisterSection? data = _eventService.Lookup<TbActivityRegisterSection>(ref _message, x => x.Id == RSId).FirstOrDefault();

                    // 活動主表
                    activity = _eventService.Lookup<TbActivity>(ref _message, x => x.Id == data.ActivityId).FirstOrDefault();

                    // 活動報名者資料
                    register = _eventService.Lookup<TbActivityRegister>(ref _message, x => x.Id == data.RegisterId).FirstOrDefault();

                    // 活動報名時段資料
                    long longSection = Convert.ToInt64(data.RegisterSectionId);
                    activitySection = _eventService.Lookup<TbActivitySection>(ref _message, x => x.Id == longSection).FirstOrDefault();


                    if (data == null)
                    {
                        TempData["TempMsgDetail"] = "查無指定項目！";
                    }
                    else
                    {
                        item = data;
                        // 如果簽到過 = 寄滿意度調查
                        if (!item.IsSigninAm && !item.IsSigninPm)
                            isSend = true;
                        if (type == "AM")
                            item.IsSigninAm = !item.IsSigninAm;
                        else
                            item.IsSigninPm = !item.IsSigninPm;
                        item.ModifyDate = dtnow;
                        item.ModifyUser = userinfo.UserID;

                        using (var transaction = _eventService.GetTransaction())
                        {
                            try
                            {
                                //編輯
                                await _eventService.Update(item, transaction);

                                transaction.Commit();
                                isSuccess = true;

                                // 後臺簽到寄送滿意度調查與學員出席證明
                                if (isSend)
                                {
                                    HttpRequest httpRequest = _contextAccessor.HttpContext.Request;
                                    string webSiteDomain = new StringBuilder().Append(httpRequest.Scheme).Append("://").Append(httpRequest.Host).ToString();

                                    string actMonth = activitySection.Day.Month.ToString();
                                    string actDay = activitySection.Day.Day.ToString();

                                    //主旨
                                    string sSubject = string.Format("【活動滿意度問卷】桃竹苗分署{0}月{1}日{2}-{3}", actMonth, actDay, activity.Title, activity.Subject);

                                    //內容
                                    string sContent = "敬愛的學員，您好： <br />";
                                    sContent += string.Format("感謝您於{0}月{1}日參加勞動部勞動力發展署桃竹苗分署主辦之{2}–{3}。<br />", actMonth, actDay, activity.Title, activity.Subject);
                                    sContent += "為優化活動品質，桃竹苗分署邀請您填寫本次活動滿意度調查問卷，作為未來活動規劃與改善之參考，敬請協助填寫，謝謝您！<br /><br />";
                                    sContent += string.Format("{0}月{1}日滿意度問卷調查連結：<br /><a href='{2}/Frontend/Activity/Quiz/{3}' target='_blank'>網址連結</a><br /><br />", actMonth, actDay, webSiteDomain, EncryptService.AES.RandomizedEncrypt(register.Id.ToString()));
                                    sContent += "附件：課程講義、其他服務資源資訊<br /><br />";
                                    sContent += "敬祝 順心平安<br />";
                                    sContent += "勞動部勞動力發展署桃竹苗分署<br />";
                                    sContent += "桃竹苗區域運籌人力資源整合服務計畫_專案辦公室<br />";
                                    sContent += "諮詢電話： 02-23660812 #164、#127 03-4855368#1905";

                                    // 取出講義
                                    string HandoutFilePath = "";
                                    TbFileInfo HandoutFile = _eventService.Lookup<TbFileInfo>(ref _message).Where(x => x.FileId == activity.HandoutFile).FirstOrDefault();
                                    if (HandoutFile != null)
                                    {
                                        HandoutFilePath = _fileService.MapPath(HandoutFile.FilePath);
                                        Attachment attachmentHandout = new Attachment(HandoutFilePath);
                                        string strExtend = HandoutFile.FileName.Split('.')[1];
                                        attachmentHandout.Name = "講義_" + dtnow.Millisecond.ToString() + "." + strExtend;  // set name here
                                        listAttachments.Add(attachmentHandout);
                                    }
                                    

                                    // 製作學員出席證明
                                    ProofExportExtend itemProof = new ProofExportExtend()
                                    {
                                        ActivityTitle = activity.Title,
                                        ActivitySubject = activity.Subject,
                                        StudentName = register.Name,
                                        Year = (activitySection.Day.Year - 1911).ToString(),
                                        Month = activitySection.Day.Month.ToString(),
                                        Day = activitySection.Day.Day.ToString(),
                                        Week = Week(activitySection.Day),
                                        Time = activitySection.StartTime.ToString("hh\\:mm") + "~" + activitySection.EndTime.ToString("hh\\:mm")
                                    };

                                    // 預設的存檔路徑
                                    string fileUploadRoot = _config["Site:FileUploadRoot"];
                                    // 檔案存放路徑 (注意：檔案只能存在wwwroot底下)
                                    string folderPath = _fileService.MapPath(fileUploadRoot, "ProofOfStudent/" + register.Id);

                                    // 建立目錄
                                    if (!Directory.Exists(folderPath))
                                        Directory.CreateDirectory(folderPath);

                                    // 存檔路徑
                                    string filePath = _fileService.MapPath(fileUploadRoot, "ProofOfStudent/" + register.Id, "學員" + register.Name + "出席證明"+ ".docx");
                                    
                                    // 產生學生出席證明word 
                                    var GenerateProof = _exportService.ProofWord(itemProof, filePath);
                                    Attachment attachmentProof = new Attachment(filePath);
                                    attachmentProof.Name = "學員" + register.Name + "出席證明_" + dtnow.Millisecond.ToString() + ".docx";  // set name here
                                    listAttachments.Add(attachmentProof);

                                    //直接測試寄信
                                    await _mailService.SendEmail(new MailViewModel()
                                    {
                                        ToList = new List<MailAddressInfo>() { new MailAddressInfo(register.Email) },
                                        Subject = sSubject,
                                        Body = sContent,
                                        AttachmentList = listAttachments
                                    });
                                }
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
                result.Message = "簽到成功";
            }
            else
            {
                result.alert_type = "error";
                result.Message = result.Message ?? "編輯失敗";

                if (unCaughtError)
                {
                    await _allCommonService.Error_Record("Backend", Feature + "-" + Action, _message);
                }
            }

            //操作紀錄
            string response = result.Message + "\r\n" + result.MessageDetail;
            await _commonService.OperateLog(userinfo.UserID, Feature, Action, null, null);

            return Json(result);
        }


        /// <summary>
        /// 匯出報名名單列表
        /// </summary>
        /// <returns></returns>
        [BackendCheckLogin("Menu000016", "DOWNLOAD")]
        [ValidateAntiForgeryToken]
        [RequestFormLimits(ValueCountLimit = int.MaxValue)] // post form data 大小限制 
        [HttpPost]
        public async Task<IActionResult> EventRegistrationExport(VM_Event data)
        {
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "活動報名名單", Action = "匯出";

            // 最終動作成功與否
            bool isSuccess = false;

            // 例外錯誤發生，特別記錄至 TbLog
            bool unCaughtError = false;

            ActionResultModel<MemoryStream> result = new ActionResultModel<MemoryStream>();
            string FileName = "活動報名名單_" + DateTime.Today.ToString("yyyyMMdd");

            try
            {
                data.Search.activityId = data.ActivityItem.Id;
                //取資料
                List<RegistrationExtend>? dataList = _eventService.GetRegistrationExtendList(ref _message, data.Search);

                result = _exportService.EventInfoRegistrationExcel(dataList.AsQueryable());

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
        /// 活動報名簽到表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [BackendCheckLogin("Menu000016", "DOWNLOAD")]
        public async Task<IActionResult> EventSigninExport(VM_Event datapost)
        {
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "活動報名簽到表", Action = "匯出";

            // 最終動作成功與否
            bool isSuccess = false;

            // 例外錯誤發生，特別記錄至 TbLog
            bool unCaughtError = false;

            ActionResultModel<MemoryStream> result = new ActionResultModel<MemoryStream>();
            string FileName = "活動報名簽到表_" + DateTime.Today.ToString("yyyyMMdd");

            try
            {
                // 查詢條件綁定-活動主表ID
                datapost.Search.activityId = datapost.ActivityItem.Id;

                //取資料
                List<RegistrationExtend>? dataList = _eventService.GetRegistrationExtendList(ref _message, datapost.Search).Where(x=>x.register.IsValid.HasValue && x.register.IsValid.Value).OrderBy(x=>x.register.Name).ToList();

                // 取得活動日期與時段
                long sectionId = Convert.ToInt64(datapost.Search.sSection);
                TbActivitySection ActivitySection = _eventService.Lookup<TbActivitySection>(ref _message, x => x.Id == sectionId && x.ActivityId == datapost.Search.activityId).FirstOrDefault();

                if (dataList == null )
                {
                    TempData["TempMsg"] = "資料回傳有誤，請重新操作！";
                }
                else
                {
                    int chYear = ActivitySection.Day.Year - 1911;
                    string ActMonth = ActivitySection.Day.Month.ToString().PadLeft(2, '0');
                    string ActDay = ActivitySection.Day.Day.ToString().PadLeft(2, '0');
                    string ActWeek = Week(ActivitySection.Day);
                    string ActStartTime = ActivitySection.StartTime.ToString();
                    string ActEndTime = ActivitySection.EndTime.ToString();
                    string ActDate = string.Concat(chYear.ToString(),"年", ActMonth,"月", ActDay,"日 (",ActWeek,")", ActStartTime,"-", ActEndTime);

                    RegistrationExportExtend item = new RegistrationExportExtend()
                    {
                        ActivityTitle = String.Concat(datapost.ActivityItem.Title,datapost.ActivityItem.Subject),
                        ActivityDate = ActDate,
                        ActivityPlace = datapost.ActivityItem.Place,
                        listData = dataList
                    };

                    //產生Word檔
                    result = _exportService.EventSigninWord(item);

                    if (!result.IsSuccess)
                    {
                        TempData["TempMsgDetail"] = "發生技術性錯誤，請聯絡技術人員或稍後再試一次";
                        _message += result.Message;
                        unCaughtError = true;
                    }

                    isSuccess = result.IsSuccess;
                }
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

            if (isSuccess && result.Data != null)
            {
                return File(result.Data.ToArray(), "application/vnd.ms-word.document.12", FileName + ".docx");
            }
            else
            {
                return RedirectToAction("EventInfoManage");
            }
        }
        public string Week(DateTime ActDate)
        {
            string[] weekdays = { "日", "一", "二", "三", "四", "五", "六" };
            string week = weekdays[Convert.ToInt32(ActDate.DayOfWeek)];

            return week;
        }

        /// <summary>
        /// 匯出QRCODE
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [BackendCheckLogin("Menu000016", "DOWNLOAD")]
        public async Task<IActionResult> GenerateQRcode(VM_Event datapost)
        {
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "匯出活動報名QRcode", Action = "匯出";

            // 最終動作成功與否
            bool isSuccess = false;

            // 例外錯誤發生，特別記錄至 TbLog
            bool unCaughtError = false;

            ActionResultModel<MemoryStream> result = new ActionResultModel<MemoryStream>();

            HttpRequest httpRequest = _contextAccessor.HttpContext.Request;
            string webSiteDomain = new StringBuilder().Append(httpRequest.Scheme).Append("://").Append(httpRequest.Host).ToString();

            DateTime dT = DateTime.Now;

            // 前台簽到網址
            string SigninUrl = "";
            string ReturnFilePath = "";
            try
            {
                if (!string.IsNullOrEmpty(datapost.ActivityItem.Id) && !string.IsNullOrEmpty(datapost.Search.sSection))
                {
                    // 前台簽到網址
                    SigninUrl = string.Format("{0}/Frontend/Activity/Checkin?aid={1}%26sid={2}", webSiteDomain, EncryptService.AES.RandomizedEncrypt(datapost.ActivityItem.Id), EncryptService.AES.RandomizedEncrypt(datapost.Search.sSection));

                    // QRCODE URL
                    var url = string.Format("http://chart.apis.google.com/chart?cht=qr&chs={1}x{2}&chl={0}", SigninUrl, 500, 500);
                    WebResponse webResponse = default(WebResponse);
                    Stream remoteStream = default(Stream);
                    StreamReader readStream = default(StreamReader);
                    WebRequest request = WebRequest.Create(url);
                    webResponse = request.GetResponse();
                    remoteStream = webResponse.GetResponseStream();
                    readStream = new StreamReader(remoteStream);
                    System.Drawing.Image img = System.Drawing.Image.FromStream(remoteStream);

                    // 預設的存檔路徑
                    string fileUploadRoot = _config["Site:FileUploadRoot"];
                    // 檔案存放路徑 (注意：檔案只能存在wwwroot底下)
                    string folderPath = _fileService.MapPath(fileUploadRoot, "EventQRcode");

                    // 建立目錄
                    if (!Directory.Exists(folderPath))
                        Directory.CreateDirectory(folderPath);

                    // 存檔路徑
                    ReturnFilePath = _fileService.MapPath(fileUploadRoot, "EventQRcode","簽到QRCODE" + dT.ToString("yyyyMMddHHmmss") + ".png");
                    img.Save(ReturnFilePath);

                    webResponse.Close();
                    remoteStream.Close();
                    readStream.Close();

                    isSuccess = true;
                }
                else
                {
                    TempData["TempMsgType"] = MsgTypeEnum.error;
                    TempData["TempMsg"] = "參數錯誤請重新確認！";

                    isSuccess = false;
                }
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
                return File(System.IO.File.ReadAllBytes(ReturnFilePath), "image/png", System.IO.Path.GetFileName(ReturnFilePath));
            }
            else
            {
                return RedirectToAction("EventInfoManage");
            }
        }

        #endregion
    }
    
}
