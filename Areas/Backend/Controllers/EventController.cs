using BASE.Areas.Backend.Models;
using BASE.Areas.Backend.Models.Extend;
using BASE.Areas.Backend.Service;
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
    public class EventController : BaseController
    {
        private readonly AllCommonService _allCommonService;
        private readonly CommonService _commonService;
        private readonly EventService _eventService;
        private readonly FileService _fileService;
        private readonly IConfiguration _config;

        public EventController(AllCommonService allCommonService,
            FileService fileService,
            CommonService commonService,
            EventService eventService,
            IConfiguration configuration)
        {
            _allCommonService = allCommonService;
            _commonService = commonService;
            _fileService = fileService;
            _eventService = eventService;
            _config = configuration;
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
                    item.RegEndDate = datapost.EventInfoItem.activity.RegEndDate;
                    item.Title = datapost.EventInfoItem.activity.Title;
                    item.Subject = datapost.EventInfoItem.activity.Subject;
                    item.DateType = datapost.EventInfoItem.activity.DateType;
                    item.LecturerInfo = datapost.EventInfoItem.activity.LecturerInfo;
                    item.Description = datapost.EventInfoItem.activity.Description;
                    item.Quota = datapost.EventInfoItem.activity.Quota;
                    item.RegisterFor = datapost.EventInfoItem.activity.RegisterFor;
                    item.Place = datapost.EventInfoItem.activity.Place;
                    item.IsDelete = false;  // 預設關閉
                    item.IsPublish = true;  // 預設報名開關為on
                    item.IsValid = false;   // 預設未審核
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
                            item.RegEndDate = datapost.EventInfoItem.activity.RegEndDate;
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
                                foreach (var itemUserEXP in datapost.EventInfoItem.sectionExtendList)
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
                                    if (item.IsPublish)
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
            await _commonService.OperateLog(userinfo.UserID, Feature, Action, decrypt_id, id, _message, response, isSuccess);

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
                        item.IsValid = !item.IsValid;
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
                        data.RegistrationExtendItem = await temp.SingleOrDefaultAsync();

                    isSuccess = true;
                    //if (data.RegistrationExtendItem == null)
                    //{
                    //    TempData["TempMsgDetail"] = "查無指定項目！";
                    //}
                    //else
                    //{
                    //    data.RegistrationExtendItem = await PagerInfoService.GetRange(data.RegistrationExtendItem, data.Search.PagerInfo);

                    //    isSuccess = true;
                    //}
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
                    //IQueryable<RegistrationExtend>? temp = _eventService.GetRegistrationExtendItem(ref _message, decrypt_id);
                    //if (temp != null)
                    //    data.RegistrationExtendItem = await temp.SingleOrDefaultAsync();

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

            List<TbActivityRegister>? activityRegister = new List<TbActivityRegister>();

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
                            TbActivityRegister temp = new TbActivityRegister();
                            temp = item.register;
                            temp.IsValid = true;
                            temp.ModifyDate = dtnow;
                            temp.ModifyUser = userinfo.UserID;
                            activityRegister.Add(temp);
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
        public async Task<IActionResult> ChangeVerifyStatus(List<string> arrRegistration, List<string> verifyStatus)
        {
            JsonResponse<TbActivityRegister> result = new JsonResponse<TbActivityRegister>();
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "活動報名名單管理", Action = "批次審核";
            
            DateTime dtnow = DateTime.Now;

            // 最終動作成功與否
            bool isSuccess = false;

            // 例外錯誤發生，特別記錄至 TbLog
            bool unCaughtError = false;

            List<TbActivityRegister>? activityRegister = new List<TbActivityRegister>();

            try
            {
                if (arrRegistration == null || arrRegistration == null)
                {
                    result.MessageDetail = "金鑰逾時！請重新再操作一次！";
                }
                else
                {
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
                            TbActivityRegister temp = dataList.Where(x => x.Id.ToString() == arrRegistration[i]).FirstOrDefault();
                            temp.IsValid = verifyStatus[i] != "-1" ? (verifyStatus[i] == "1" ? true : false) : null;
                            temp.ModifyDate = dtnow;
                            temp.ModifyUser = userinfo.UserID;
                            activityRegister.Add(temp);
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

            TbActivityRegisterSection? item = new TbActivityRegisterSection();

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

                    if (data == null)
                    {
                        TempData["TempMsgDetail"] = "查無指定項目！";
                    }
                    else
                    {
                        item = data;
                        if(type == "AM")
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

        #endregion
    }
}
