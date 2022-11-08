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

namespace BASE.Areas.Backend.Controllers
{
    public class RelationLinkController : BaseController
    {
        private readonly AllCommonService _allCommonService;
        private readonly CommonService _commonService;
        private readonly RelationLinkService _relService;
        private readonly FileService _fileService;

        public RelationLinkController(AllCommonService allCommonService,
            FileService fileService,
            CommonService commonService,
            RelationLinkService relService)
        {
            _allCommonService = allCommonService;
            _fileService = fileService;
            _commonService = commonService;
            _relService = relService;
        }

        /// <summary>
        /// RelationLinkList列表
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [BackendCheckLogin("Menu000010", "ENABLED")]
        public async Task<IActionResult> RelationLinkList(VM_RelationLink data)
        {
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "相關連結管理", Action = "檢視";

            try
            {
                //取資料
                IQueryable<RelationLinkExtend>? dataList = _relService.GetRelationLinkExtendList(ref _message);

                //分頁
                if (dataList != null)
                    data.RelationLinkExtendList = await PagerInfoService.GetRange(dataList.OrderBy(x => x.RelationLink.Sort).ThenByDescending(x => x.RelationLink.CreateDate), data.Search.PagerInfo);

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
        /// 新增RelationLink
        /// </summary>
        /// <returns></returns>
        [BackendCheckLogin("Menu000011", "ADD")]
        public async Task<IActionResult> RelationLinkAdd()
        {
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "相關連結新增", Action = "新增";

            VM_RelationLink data = new VM_RelationLink();

            await _commonService.OperateLog(userinfo.UserID, Feature, Action);
            return View(data);
        }

        /// <summary>
        /// 新增RelationLink存檔
        /// </summary>
        /// <param name="datapost"></param>
        /// <returns></returns>
        [BackendCheckLogin("Menu000011", "ADD")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RelationLinkAdd(VM_RelationLink datapost)
        {
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "相關連結新增", Action = "新增";


            // 最終動作成功與否
            bool isSuccess = false;

            // 例外錯誤發生，特別記錄至 TbLog
            bool unCaughtError = false;

            DateTime dtnow = DateTime.Now;
            TbRelationLink? item = new TbRelationLink();
            string? target_id = null;

            try
            {
                if (datapost.RelationLinkExtendItem != null)
                {
                    item.Title = datapost.RelationLinkExtendItem.RelationLink.Title;
                    item.Link = datapost.RelationLinkExtendItem.RelationLink.Link;
                    item.IsDelete = false;
                    if (datapost.Search.sPublish == "上架")
                    { item.IsPublish = true; }
                    else
                    { item.IsPublish = false; }
                    item.CreateUser = userinfo.UserID;
                    item.CreateDate = dtnow;
                    item.Sort = (_relService.GetRelationLinkMAXSort(ref _message) ?? 0) + 1;

                    using (var transaction = _relService.GetTransaction())
                    {
                        try
                        {
                            //圖片
                            if (datapost.RelationLinkImageFile != null)
                            {
                                var photo_upload = await _fileService.FileUploadAsync(datapost.RelationLinkImageFile, "RelationLinkImageFiles/" + item.Id, "RelationLinkImageFils", item.FileId, null, transaction);
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
                            await _relService.Insert(item, transaction);

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
                return RedirectToAction("RelationLinkList");
            }
            else
            {
                if (item == null)
                {
                    return RedirectToAction("RelationLinkList");
                }
                else
                {
                    /* 失敗回原頁 */
                    return View(datapost);
                }
            }
        }

        /// <summary>
        /// 編輯RelationLink
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [BackendCheckLogin("Menu000012", "MODIFY")]
        public async Task<IActionResult> RelationLinkEdit(int id)
        {
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "相關連結編輯", Action = "檢視";

            VM_RelationLink data = new VM_RelationLink();

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
                    IQueryable<RelationLinkExtend>? dataList = _relService.GetRelationLinkExtendItem(ref _message, id);
                    if (dataList != null)
                        data.RelationLinkExtendItem = dataList.FirstOrDefault();

                    if (data.RelationLinkExtendItem == null)
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

                return RedirectToAction("RelationLinkList");
            }
        }

        /// <summary>
        /// 編輯RelationLink_存檔
        /// </summary>
        /// <param name="id"></param>
        /// <param name="datapost"></param>
        /// <returns></returns>
        [BackendCheckLogin("Menu000012", "MODIFY")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RelationLinkEdit(int id, VM_RelationLink datapost)
        {
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "相關連結編輯", Action = "編輯";

            // 最終動作成功與否
            bool isSuccess = false;

            // 例外錯誤發生，特別記錄至 TbLog
            bool unCaughtError = false;

            DateTime dtnow = DateTime.Now;

            TbRelationLink? item = null;

            string decrypt_id = id.ToString();

            try
            {
                if (string.IsNullOrEmpty(decrypt_id))
                {
                    TempData["TempMsgDetail"] = "金鑰逾時！請重新再操作一次！";
                }
                else
                {
                    IQueryable<TbRelationLink>? temp = _relService.Lookup<TbRelationLink>(ref _message, x => x.Id == id && x.IsDelete == false);
                    if (temp != null)
                        item = temp.FirstOrDefault();

                    if (item == null)
                    {
                        TempData["TempMsgDetail"] = "查無指定項目！";
                    }
                    else if (datapost.RelationLinkExtendItem == null)
                    {
                        TempData["TempMsgDetail"] = "資料回傳有誤，請重新操作！";
                    }
                    else
                    {
                        item.Title = datapost.RelationLinkExtendItem.RelationLink.Title;
                        item.Link = datapost.RelationLinkExtendItem.RelationLink.Link;
                        if (datapost.Search.sPublish == "上架")
                        { item.IsPublish = true; }
                        else
                        { item.IsPublish = false; }
                        item.ModifyUser = userinfo.UserID;
                        item.ModifyDate = dtnow;

                        using (var transaction = _relService.GetTransaction())
                        {
                            try
                            {
                                //圖片
                                if (datapost.RelationLinkImageFile != null)
                                {
                                    var photo_upload = await _fileService.FileUploadAsync(datapost.RelationLinkImageFile, "RelationLinkImageFiles/" + item.Id, "RelationLinkImageFils", item.FileId, null, transaction);
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
                                await _relService.Update(item, transaction);

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
                return RedirectToAction("RelationLinkList");
            }
            else
            {
                if (item == null)
                {
                    return RedirectToAction("RelationLinkList");
                }
                else
                {
                    /* 失敗回原頁 */
                    return View(datapost);
                }
            }
        }

        /// <summary>
        /// 刪除RelationLink
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [BackendCheckLogin("Menu000010", "DELETE")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RelationLinkDelete(int id)
        {
            JsonResponse<TbRelationLink> result = new JsonResponse<TbRelationLink>();
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "相關連結管理", Action = "刪除";

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
                    TbRelationLink? item = null;
                    IQueryable<TbRelationLink>? temp = _relService.Lookup<TbRelationLink>(ref _message, x => x.Id == id && x.IsDelete == false);
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

                        using (var transaction = _relService.GetTransaction())
                        {
                            try
                            {
                                //編輯
                                await _relService.Update(item, transaction);

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
                    List<TbRelationLink>? itemReSort = null;
                    IQueryable<TbRelationLink>? tempReSort = _relService.Lookup<TbRelationLink>(ref _message, x => x.IsDelete == false).OrderBy(x => x.Sort);
                    if (tempReSort != null)
                        itemReSort = tempReSort.ToList();

                    int ReSortIndex = 1;

                    if (itemReSort.Any())
                    {
                        foreach (var itemRe in itemReSort)
                        {
                            itemRe.Sort = ReSortIndex;

                            using (var transaction = _relService.GetTransaction())
                            {
                                try
                                {
                                    //編輯
                                    await _relService.Update(itemRe, transaction);

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
        /// RelationLink上下架狀態變更
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [BackendCheckLogin("Menu000010", "MODIFY")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RelationLinkPublishChange(long id)
        {
            JsonResponse<TbRelationLink> result = new JsonResponse<TbRelationLink>();
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "相關連結管理", Action = "編輯";

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
                    TbRelationLink? item = null;
                    IQueryable<TbRelationLink>? temp = _relService.Lookup<TbRelationLink>(ref _message, x => x.Id == id && x.IsDelete == false);
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

                        using (var transaction = _relService.GetTransaction())
                        {
                            try
                            {
                                //編輯
                                await _relService.Update(item, transaction);

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
        /// 上移RelationLink
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [BackendCheckLogin("Menu000010", "MODIFY")]
        public async Task<IActionResult> RelationLinkMoveUp(int id)
        {
            JsonResponse<TbRelationLink> result = new JsonResponse<TbRelationLink>();
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "相關連結管理", Action = "編輯";

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
                    TbRelationLink? item = null;
                    TbRelationLink? itemchange = null;

                    IQueryable<TbRelationLink>? temp = _relService.Lookup<TbRelationLink>(ref _message, x => x.Sort == id && x.IsDelete == false);
                    IQueryable<TbRelationLink>? tempchange = _relService.Lookup<TbRelationLink>(ref _message, x => x.Sort == id - 1 && x.IsDelete == false);

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

                        using (var transaction = _relService.GetTransaction())
                        {
                            try
                            {
                                //編輯
                                await _relService.Update(item, transaction);
                                await _relService.Update(itemchange, transaction);

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

            return RedirectToAction("RelationLinkList");
        }

        /// <summary>
        /// 上移RelationLink
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [BackendCheckLogin("Menu000010", "MODIFY")]
        public async Task<IActionResult> RelationLinkMoveDown(int id)
        {
            JsonResponse<TbRelationLink> result = new JsonResponse<TbRelationLink>();
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "相關連結管理", Action = "編輯";

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
                    TbRelationLink? item = null;
                    TbRelationLink? itemchange = null;

                    IQueryable<TbRelationLink>? temp = _relService.Lookup<TbRelationLink>(ref _message, x => x.Sort == id && x.IsDelete == false);
                    IQueryable<TbRelationLink>? tempchange = _relService.Lookup<TbRelationLink>(ref _message, x => x.Sort == id + 1 && x.IsDelete == false);

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

                        using (var transaction = _relService.GetTransaction())
                        {
                            try
                            {
                                //編輯
                                await _relService.Update(item, transaction);
                                await _relService.Update(itemchange, transaction);

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

            return RedirectToAction("RelationLinkList");
        }
    }
}
