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
    public class AdvertiseController : BaseController
    {
        private readonly AllCommonService _allCommonService;
        private readonly CommonService _commonService;
        private readonly AdvertiseService _advService;
        private readonly FileService _fileService;

        public AdvertiseController(AllCommonService allCommonService,
            FileService fileService,
            CommonService commonService,
            AdvertiseService advService)
        {
            _allCommonService = allCommonService;
            _fileService = fileService;
            _commonService = commonService;
            _advService = advService;
        }

        /// <summary>
        /// AdvertiseList列表
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [BackendCheckLogin("Menu000006", "ENABLED")]
        public async Task<IActionResult> AdvertiseList(VM_Advertise data)
        {
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "輪播廣告管理", Action = "檢視";

            try
            {
                //取資料
                IQueryable<AdvertiseExtend>? dataList = _advService.GetAdvertiseExtendList(ref _message);

                //分頁
                if (dataList != null)
                    data.AdvertiseExtendList = await PagerInfoService.GetRange(dataList.OrderBy(x => x.Advertise.Sort).ThenByDescending(x => x.Advertise.CreateDate), data.Search.PagerInfo);

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
        /// 新增Advertise
        /// </summary>
        /// <returns></returns>
        [BackendCheckLogin("Menu000008", "ADD")]
        public async Task<IActionResult> AdvertiseAdd()
        {
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "輪播廣告新增", Action = "新增";

            VM_Advertise data = new VM_Advertise();

            await _commonService.OperateLog(userinfo.UserID, Feature, Action);
            return View(data);
        }

        /// <summary>
        /// 新增Advertise存檔
        /// </summary>
        /// <param name="datapost"></param>
        /// <returns></returns>
        [BackendCheckLogin("Menu000008", "ADD")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdvertiseAdd(VM_Advertise datapost)
        {
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "輪播廣告新增", Action = "新增";


            // 最終動作成功與否
            bool isSuccess = false;

            // 例外錯誤發生，特別記錄至 TbLog
            bool unCaughtError = false;

            DateTime dtnow = DateTime.Now;
            TbAdvertise? item = new TbAdvertise();
            string? target_id = null;

            try
            {
                if (datapost.AdvertiseExtendItem != null)
                {
                    item.Title = datapost.AdvertiseExtendItem.Advertise.Title;
                    item.Link = datapost.AdvertiseExtendItem.Advertise.Link;
                    item.IsDelete = false;
                    if (datapost.Search.sPublish == "上架")
                    { item.IsPublish = true; }
                    else
                    { item.IsPublish = false; }
                    item.CreateUser = userinfo.UserID;
                    item.CreateDate = dtnow;
                    item.Sort = (_advService.GetAdvertisMAXSort(ref _message) ?? 0) + 1;

                    using (var transaction = _advService.GetTransaction())
                    {
                        try
                        {
                            //輪播圖片
                            if (datapost.AdvertiseImageFile != null)
                            {
                                var photo_upload = await _fileService.FileUploadAsync(datapost.AdvertiseImageFile, "AdvertiseImageFiles/" + item.Id, "AdvertiseImageFils", item.FileId, null, transaction);
                                if (photo_upload.IsSuccess == true && !string.IsNullOrEmpty(photo_upload.FileID))
                                {
                                    item.FileId = photo_upload.FileID;
                                }
                                else
                                {
                                    _message += photo_upload.Message;
                                }
                            }
                            //新增
                            await _advService.Insert(item, transaction);

                            transaction.Commit();
                            isSuccess = true;
                            target_id = item.Id.ToString();
                        }
                        catch (Exception ex)
                        {
                            _message += ex.ToString();
                            TempData["TempMsgDetail"] = "發生技術性錯誤，請聯絡技術人員或稍後再試一次";
                            unCaughtError = true;
                        }
                    }
                }
                else
                {
                    TempData["TempMsg"] = "資料回傳有誤，請重新操作！";
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
            await _commonService.OperateLog(userinfo.UserID, Feature, Action, target_id, datapost, _message, response, isSuccess);

            if (isSuccess)
            {
                return RedirectToAction("AdvertiseList");
            }
            else
            {
                if (item == null)
                {
                    return RedirectToAction("AdvertiseList");
                }
                else
                {
                    /* 失敗回原頁 */
                    return View(datapost);
                }
            }
        }

        /// <summary>
        /// 編輯Advertise
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [BackendCheckLogin("Menu000009", "MODIFY")]
        public async Task<IActionResult> AdvertiseEdit(int id)
        {
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "輪播廣告編輯", Action = "檢視";

            VM_Advertise data = new VM_Advertise();

            // 最終動作成功與否
            bool isSuccess = false;

            // 例外錯誤發生，特別記錄至 TbLog
            bool unCaughtError = false;

            string decrypt_id = id.ToString();

            try
            {

                if (string.IsNullOrEmpty(decrypt_id))
                {
                    TempData["TempMsgDetail"] = "金鑰逾時！請重新再操作一次！";
                }
                else
                {
                    IQueryable<AdvertiseExtend>? dataList = _advService.GetAdvertisExtendItem(ref _message, id);
                    if (dataList != null)
                        data.AdvertiseExtendItem = dataList.FirstOrDefault();

                    if (data.AdvertiseExtendItem == null)
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
                await _commonService.OperateLog(userinfo.UserID, Feature, Action, decrypt_id, id.ToString());
                return View(data);
            }
            else
            {
                TempData["TempMsgType"] = MsgTypeEnum.error;
                TempData["TempMsg"] = TempData["TempMsg"] ?? "操作失敗";

                //操作紀錄
                string response = (TempData["TempMsg"] == null ? "" : TempData["TempMsg"].ToString()) + "\r\n" + (TempData["TempMsgDetail"] == null ? "" : TempData["TempMsgDetail"].ToString());
                await _commonService.OperateLog(userinfo.UserID, Feature, Action, decrypt_id, id.ToString(), _message, response, isSuccess);

                if (unCaughtError)
                {
                    await _allCommonService.Error_Record("Backend", Feature + "-" + Action, _message);
                }

                return RedirectToAction("AdvertiseList");
            }
        }

        /// <summary>
        /// 編輯Advertise_存檔
        /// </summary>
        /// <param name="id"></param>
        /// <param name="datapost"></param>
        /// <returns></returns>
        [BackendCheckLogin("Menu000009", "MODIFY")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdvertiseEdit(long id, VM_Advertise datapost)
        {
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "輪播廣告編輯", Action = "編輯";

            // 最終動作成功與否
            bool isSuccess = false;

            // 例外錯誤發生，特別記錄至 TbLog
            bool unCaughtError = false;

            DateTime dtnow = DateTime.Now;

            TbAdvertise? item = null;

            string decrypt_id = id.ToString();

            try
            {
                if (string.IsNullOrEmpty(decrypt_id))
                {
                    TempData["TempMsgDetail"] = "金鑰逾時！請重新再操作一次！";
                }
                else
                {
                    IQueryable<TbAdvertise>? temp = _advService.Lookup<TbAdvertise>(ref _message, x => x.Id == id && x.IsDelete == false);
                    if (temp != null)
                        item = temp.FirstOrDefault();

                    if (item == null)
                    {
                        TempData["TempMsgDetail"] = "查無指定項目！";
                    }
                    else if (datapost.AdvertiseExtendItem == null)
                    {
                        TempData["TempMsgDetail"] = "資料回傳有誤，請重新操作！";
                    }
                    else
                    {
                        item.Title = datapost.AdvertiseExtendItem.Advertise.Title;
                        item.Link = datapost.AdvertiseExtendItem.Advertise.Link;
                        if (datapost.Search.sPublish == "上架")
                        { item.IsPublish = true; }
                        else
                        { item.IsPublish = false; }
                        item.ModifyUser = userinfo.UserID;
                        item.ModifyDate = dtnow;

                        using (var transaction = _advService.GetTransaction())
                        {
                            try
                            {
                                //輪播圖片
                                if (datapost.AdvertiseImageFile != null)
                                {
                                    var photo_upload = await _fileService.FileUploadAsync(datapost.AdvertiseImageFile, "AdvertiseImageFiles/" + item.Id, "AdvertiseImageFils", item.FileId, null, transaction);
                                    if (photo_upload.IsSuccess == true && !string.IsNullOrEmpty(photo_upload.FileID))
                                    {
                                        item.FileId = photo_upload.FileID;
                                    }
                                    else
                                    {
                                        _message += photo_upload.Message;
                                    }
                                }

                                //編輯
                                await _advService.Update(item, transaction);

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
                return RedirectToAction("AdvertiseList");
            }
            else
            {
                if (item == null)
                {
                    return RedirectToAction("AdvertiseList");
                }
                else
                {
                    /* 失敗回原頁 */
                    return View(datapost);
                }
            }
        }

        /// <summary>
        /// 刪除Advertise
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [BackendCheckLogin("Menu000009", "DELETE")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdvertiseDelete(long id)
        {
            JsonResponse<TbAdvertise> result = new JsonResponse<TbAdvertise>();
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "輪播廣告管理", Action = "刪除";

            DateTime dtnow = DateTime.Now;

            // 最終動作成功與否
            bool isSuccess = false;

            // 例外錯誤發生，特別記錄至 TbLog
            bool unCaughtError = false;

            string decrypt_id = id.ToString();

            try
            {
                if (string.IsNullOrEmpty(decrypt_id))
                {
                    result.MessageDetail = "金鑰逾時！請重新再操作一次！";
                }
                else
                {
                    #region Delete
                    TbAdvertise? item = null;
                    IQueryable<TbAdvertise>? temp = _advService.Lookup<TbAdvertise>(ref _message, x => x.Id == id && x.IsDelete == false);
                    if (temp != null)
                        item = temp.FirstOrDefault();

                    if (item == null)
                    {
                        TempData["TempMsgDetail"] = "查無指定項目！";
                    }
                    else
                    {
                        item.Sort = 0;
                        item.IsDelete = true;
                        item.ModifyUser = userinfo.UserID;
                        item.ModifyDate = dtnow;

                        using (var transaction = _advService.GetTransaction())
                        {
                            try
                            {
                                //編輯
                                await _advService.Update(item, transaction);

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
                    #endregion

                    #region Sort重新排序
                    List<TbAdvertise>? itemReSort = null;
                    IQueryable<TbAdvertise>? tempReSort = _advService.Lookup<TbAdvertise>(ref _message, x => x.IsDelete == false).OrderBy(x => x.Sort);
                    if (tempReSort != null)
                        itemReSort = tempReSort.ToList();

                    int ReSortIndex = 1;

                    if (itemReSort.Any())
                    {
                        foreach (var itemRe in itemReSort)
                        {
                            itemRe.Sort = ReSortIndex;

                            using (var transaction = _advService.GetTransaction())
                            {
                                try
                                {
                                    //編輯
                                    await _advService.Update(itemRe, transaction);

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
                            ReSortIndex++;
                        }
                    }
                    #endregion
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
            await _commonService.OperateLog(userinfo.UserID, Feature, Action, decrypt_id, id.ToString(), _message, response, isSuccess);

            return Json(result);
        }

        /// <summary>
        /// Advertise上下架狀態變更
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [BackendCheckLogin("Menu000009", "MODIFY")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdvertisePublishChange(long id)
        {
            JsonResponse<TbAdvertise> result = new JsonResponse<TbAdvertise>();
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "輪播廣告管理", Action = "編輯";

            DateTime dtnow = DateTime.Now;

            // 最終動作成功與否
            bool isSuccess = false;

            // 例外錯誤發生，特別記錄至 TbLog
            bool unCaughtError = false;

            string decrypt_id = id.ToString();

            try
            {
                if (string.IsNullOrEmpty(decrypt_id))
                {
                    result.MessageDetail = "金鑰逾時！請重新再操作一次！";
                }
                else
                {
                    TbAdvertise? item = null;
                    IQueryable<TbAdvertise>? temp = _advService.Lookup<TbAdvertise>(ref _message, x => x.Id == id && x.IsDelete == false);
                    if (temp != null)
                        item = temp.FirstOrDefault();

                    if (item == null)
                    {
                        TempData["TempMsgDetail"] = "查無指定項目！";
                    }
                    else
                    {
                        if (item.IsPublish)
                        { item.IsPublish = false; }
                        else
                        { item.IsPublish = true; }
                        item.ModifyUser = userinfo.UserID;
                        item.ModifyDate = dtnow;

                        using (var transaction = _advService.GetTransaction())
                        {
                            try
                            {
                                //編輯
                                await _advService.Update(item, transaction);

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
                result.Message = "狀態變更成功";
            }
            else
            {
                result.alert_type = "error";
                result.Message = result.Message ?? "狀態變更失敗";

                if (unCaughtError)
                {
                    await _allCommonService.Error_Record("Backend", Feature + "-" + Action, _message);
                }
            }

            //操作紀錄
            string response = result.Message + "\r\n" + result.MessageDetail;
            await _commonService.OperateLog(userinfo.UserID, Feature, Action, decrypt_id, id.ToString(), _message, response, isSuccess);

            return Json(result);
        }

        /// <summary>
        /// 上移Advertise
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [BackendCheckLogin("Menu000009", "MODIFY")]
        public async Task<IActionResult> AdvertiseMoveUp(int id)
        {
            JsonResponse<TbAdvertise> result = new JsonResponse<TbAdvertise>();
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "輪播廣告管理", Action = "編輯";

            DateTime dtnow = DateTime.Now;

            // 最終動作成功與否
            bool isSuccess = false;

            // 例外錯誤發生，特別記錄至 TbLog
            bool unCaughtError = false;

            string decrypt_id = id.ToString();

            try
            {
                if (string.IsNullOrEmpty(decrypt_id))
                {
                    result.MessageDetail = "金鑰逾時！請重新再操作一次！";
                }
                else
                {
                    TbAdvertise? item = null;
                    TbAdvertise? itemchange = null;

                    IQueryable<TbAdvertise>? temp = _advService.Lookup<TbAdvertise>(ref _message, x => x.Sort == id && x.IsDelete == false);
                    IQueryable<TbAdvertise>? tempchange = _advService.Lookup<TbAdvertise>(ref _message, x => x.Sort == id - 1 && x.IsDelete == false);

                    if (temp != null && tempchange != null)
                    {
                        item = temp.FirstOrDefault();
                        itemchange = tempchange.FirstOrDefault();
                    }

                    //if (item == null)
                    //{
                    //    TempData["TempMsgDetail"] = "查無指定項目！";
                    //}
                    if (item != null && itemchange != null)
                    {
                        item.Sort = id - 1;
                        item.ModifyUser = userinfo.UserID;
                        item.ModifyDate = dtnow;

                        itemchange.Sort = id;
                        itemchange.ModifyUser = userinfo.UserID;
                        itemchange.ModifyDate = dtnow;

                        using (var transaction = _advService.GetTransaction())
                        {
                            try
                            {
                                //編輯
                                await _advService.Update(item, transaction);
                                await _advService.Update(itemchange, transaction);

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
                result.Message = "狀態變更成功";
            }
            else
            {
                result.alert_type = "error";
                result.Message = result.Message ?? "狀態變更失敗";

                if (unCaughtError)
                {
                    await _allCommonService.Error_Record("Backend", Feature + "-" + Action, _message);
                }
            }

            //操作紀錄
            string response = result.Message + "\r\n" + result.MessageDetail;
            await _commonService.OperateLog(userinfo.UserID, Feature, Action, decrypt_id, id.ToString(), _message, response, isSuccess);

            return RedirectToAction("AdvertiseList");
        }

        /// <summary>
        /// 上移Advertise
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [BackendCheckLogin("Menu000009", "MODIFY")]
        public async Task<IActionResult> AdvertiseMoveDown(int id)
        {
            JsonResponse<TbAdvertise> result = new JsonResponse<TbAdvertise>();
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "輪播廣告管理", Action = "編輯";

            DateTime dtnow = DateTime.Now;

            // 最終動作成功與否
            bool isSuccess = false;

            // 例外錯誤發生，特別記錄至 TbLog
            bool unCaughtError = false;

            string decrypt_id = id.ToString();

            try
            {
                if (string.IsNullOrEmpty(decrypt_id))
                {
                    result.MessageDetail = "金鑰逾時！請重新再操作一次！";
                }
                else
                {
                    TbAdvertise? item = null;
                    TbAdvertise? itemchange = null;

                    IQueryable<TbAdvertise>? temp = _advService.Lookup<TbAdvertise>(ref _message, x => x.Sort == id && x.IsDelete == false);
                    IQueryable<TbAdvertise>? tempchange = _advService.Lookup<TbAdvertise>(ref _message, x => x.Sort == id + 1 && x.IsDelete == false);

                    if (temp != null && tempchange != null)
                    {
                        item = temp.FirstOrDefault();
                        itemchange = tempchange.FirstOrDefault();
                    }

                    //if (item == null)
                    //{
                    //    TempData["TempMsgDetail"] = "查無指定項目！";
                    //}
                    if (item != null && itemchange != null)
                    {
                        item.Sort = id + 1;
                        item.ModifyUser = userinfo.UserID;
                        item.ModifyDate = dtnow;

                        itemchange.Sort = id;
                        itemchange.ModifyUser = userinfo.UserID;
                        itemchange.ModifyDate = dtnow;

                        using (var transaction = _advService.GetTransaction())
                        {
                            try
                            {
                                //編輯
                                await _advService.Update(item, transaction);
                                await _advService.Update(itemchange, transaction);

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
                result.Message = "狀態變更成功";
            }
            else
            {
                result.alert_type = "error";
                result.Message = result.Message ?? "狀態變更失敗";

                if (unCaughtError)
                {
                    await _allCommonService.Error_Record("Backend", Feature + "-" + Action, _message);
                }
            }

            //操作紀錄
            string response = result.Message + "\r\n" + result.MessageDetail;
            await _commonService.OperateLog(userinfo.UserID, Feature, Action, decrypt_id, id.ToString(), _message, response, isSuccess);

            return RedirectToAction("AdvertiseList");
        }
    }
}
