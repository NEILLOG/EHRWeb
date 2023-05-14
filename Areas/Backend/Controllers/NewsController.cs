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
    public class NewsController : BaseController
    {
        private readonly AllCommonService _allCommonService;
        private readonly CommonService _commonService;
        private readonly NewsService _newsService;
        private readonly FileService _fileService;

        public NewsController(AllCommonService allCommonService,
            FileService fileService,
            CommonService commonService,
            NewsService newsService)
        {
            _allCommonService = allCommonService;
            _fileService = fileService;
            _commonService = commonService;
            _newsService = newsService;
        }

        /// <summary>
        /// NewsList列表
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [BackendCheckLogin("Menu000020", "ENABLED")]
        public async Task<IActionResult> NewsList(VM_News data)
        {
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "最新消息管理", Action = "檢視";

            try
            {
                string sYear = DateTime.Now.Year.ToString();
                string eTear = (DateTime.Now.Year + 1).ToString();

                //取資料
                IQueryable<NewsExtend>? dataList = _newsService.GetNewsExtendList(ref _message);

                if (dataList != null && !string.IsNullOrEmpty(data.Search.sCategory))
                { dataList = dataList.Where(x => x.News.Category == data.Search.sCategory); }

                if (dataList != null && !string.IsNullOrEmpty(data.Search.sTitle))
                { dataList = dataList.Where(x => x.News.Title.Contains(data.Search.sTitle)); }

                if (dataList != null && !string.IsNullOrEmpty(data.Search.sTime))
                { dataList = dataList.Where(x => x.News.DisplayDate >= Convert.ToDateTime(data.Search.sTime)); }

                if (dataList != null && !string.IsNullOrEmpty(data.Search.eTime))
                { dataList = dataList.Where(x => x.News.DisplayDate <= Convert.ToDateTime(data.Search.eTime)); }

                //分頁
                if (dataList != null)
                    data.NewsExtendList = await PagerInfoService.GetRange(dataList.OrderByDescending(x => x.News.IsKeepTop).ThenByDescending(x => x.News.DisplayDate), data.Search.PagerInfo);

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
        /// 新增News
        /// </summary>
        /// <returns></returns>
        [BackendCheckLogin("Menu000021", "ADD")]
        public async Task<IActionResult> NewsAdd()
        {
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "新增最新消息", Action = "新增";

            VM_News data = new VM_News();
            data.TopId = _newsService.GetTopId(ref _message); //取得有設置頂的ID
            await _commonService.OperateLog(userinfo.UserID, Feature, Action);
            return View(data);
        }

        /// <summary>
        /// 新增Newstise存檔
        /// </summary>
        /// <param name="datapost"></param>
        /// <returns></returns>
        [BackendCheckLogin("Menu000021", "ADD")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NewsAdd(VM_News datapost)
        {
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "最新消息新增", Action = "新增";


            // 最終動作成功與否
            bool isSuccess = false;

            // 例外錯誤發生，特別記錄至 TbLog
            bool unCaughtError = false;

            DateTime dtnow = DateTime.Now;
            TbNews? item = new TbNews();
            string? target_id = null;

            try
            {
                if (datapost.NewsExtendItem != null)
                {
                    item.Id = await _allCommonService.IDGenerator<TbNews>();
                    item.Title = datapost.NewsExtendItem.News.Title;
                    item.Category = datapost.NewsExtendItem.News.Category;
                    item.DisplayDate = datapost.NewsExtendItem.News.DisplayDate;
                    item.Contents = datapost.NewsExtendItem.News.Contents;
                    item.IsDelete = false;
                    if (datapost.isPublish == "上架")
                    { item.IsPublish = true; }
                    else
                    { item.IsPublish = false; }
                    item.IsKeepTop = datapost.NewsExtendItem.News.IsKeepTop;
                    item.CreateUser = userinfo.UserID;
                    item.CreateDate = dtnow;

                    using (var transaction = _newsService.GetTransaction())
                    {
                        try
                        {
                            //相關檔案
                            if (datapost.NewsFile != null)
                            {
                                var photo_upload = await _fileService.FileUploadAsync(datapost.NewsFile, "NewsFiles/" + item.Id, "NewsFiles", item.FileId, null, transaction);
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
                            await _newsService.Insert(item, transaction);

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
                return RedirectToAction("NewsList");
            }
            else
            {
                if (item == null)
                {
                    return RedirectToAction("NewsList");
                }
                else
                {
                    /* 失敗回原頁 */
                    return View(datapost);
                }
            }
        }

        /// <summary>
        /// 編輯News
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [BackendCheckLogin("Menu000022", "MODIFY")]
        public async Task<IActionResult> NewsEdit(string id)
        {
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "最新消息編輯", Action = "檢視";

            VM_News data = new VM_News();

            // 最終動作成功與否
            bool isSuccess = false;

            // 例外錯誤發生，特別記錄至 TbLog
            bool unCaughtError = false;

            string decrypt_id = id;

            try
            {

                if (string.IsNullOrEmpty(decrypt_id))
                {
                    TempData["TempMsgDetail"] = "金鑰逾時！請重新再操作一次！";
                }
                else
                {
                    IQueryable<NewsExtend>? dataList = _newsService.GetNewsExtendItem(ref _message, id);
                    data.TopId = _newsService.GetTopId(ref _message); //取得有設置頂的ID
                    if (dataList != null)
                    {
                        data.NewsExtendItem = dataList.FirstOrDefault();
                        if (data.NewsExtendItem.News.IsPublish)
                        { data.isPublish = "上架"; }
                        else
                        { data.isPublish = "下架"; }
                    }

                    if (data.NewsExtendItem == null)
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

                return RedirectToAction("NewsList");
            }
        }

        /// <summary>
        /// 編輯News_存檔
        /// </summary>
        /// <param name="id"></param>
        /// <param name="datapost"></param>
        /// <returns></returns>
        [BackendCheckLogin("Menu000022", "MODIFY")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NewsEdit(string id, VM_News datapost)
        {
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "最新消息編輯", Action = "編輯";

            // 最終動作成功與否
            bool isSuccess = false;

            // 例外錯誤發生，特別記錄至 TbLog
            bool unCaughtError = false;

            DateTime dtnow = DateTime.Now;

            TbNews? item = null;

            string decrypt_id = id;

            try
            {
                if (string.IsNullOrEmpty(decrypt_id))
                {
                    TempData["TempMsgDetail"] = "金鑰逾時！請重新再操作一次！";
                }
                else
                {
                    IQueryable<TbNews>? temp = _newsService.Lookup<TbNews>(ref _message, x => x.Id == id && x.IsDelete == false);
                    if (temp != null)
                        item = temp.FirstOrDefault();

                    if (item == null)
                    {
                        TempData["TempMsgDetail"] = "查無指定項目！";
                    }
                    else if (datapost.NewsExtendItem == null)
                    {
                        TempData["TempMsgDetail"] = "資料回傳有誤，請重新操作！";
                    }
                    else
                    {
                        item.Id = id;
                        item.Title = datapost.NewsExtendItem.News.Title;
                        item.Category = datapost.NewsExtendItem.News.Category;
                        item.DisplayDate = datapost.NewsExtendItem.News.DisplayDate;
                        item.Contents = datapost.NewsExtendItem.News.Contents;
                        if (datapost.isPublish == "上架")
                        { item.IsPublish = true; }
                        else
                        { item.IsPublish = false; }
                        item.IsKeepTop = datapost.NewsExtendItem.News.IsKeepTop;
                        item.ModifyUser = userinfo.UserID;
                        item.ModifyDate = dtnow;

                        using (var transaction = _newsService.GetTransaction())
                        {
                            try
                            {
                                //相關檔案
                                if (datapost.NewsFile != null)
                                {
                                    var photo_upload = await _fileService.FileUploadAsync(datapost.NewsFile, "NewsFiles/" + item.Id, "NewsFiles", item.FileId, null, transaction);
                                    if (photo_upload.IsSuccess == true && !string.IsNullOrEmpty(photo_upload.FileID))
                                    {
                                        item.FileId = photo_upload.FileID;
                                    }
                                    else
                                    {
                                        _message += photo_upload.Message;
                                    }
                                }
                                else
                                {
                                    //檔案(刪除)
                                    if (datapost.DelFileList != null && datapost.DelFileList.Count > 0)
                                    {
                                        item.FileId = "";
                                    }
                                }

                                //編輯
                                await _newsService.Update(item, transaction);

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
                return RedirectToAction("NewsList");
            }
            else
            {
                if (item == null)
                {
                    return RedirectToAction("NewsList");
                }
                else
                {
                    /* 失敗回原頁 */
                    return View(datapost);
                }
            }
        }

        /// <summary>
        /// 刪除News
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [BackendCheckLogin("Menu000020", "DELETE")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NewsDelete(string id)
        {
            JsonResponse<TbNews> result = new JsonResponse<TbNews>();
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "最新消息管理", Action = "刪除";

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
                    TbNews? item = null;
                    IQueryable<TbNews>? temp = _newsService.Lookup<TbNews>(ref _message, x => x.Id == id && x.IsDelete == false);
                    if (temp != null)
                        item = temp.FirstOrDefault();

                    if (item == null)
                    {
                        TempData["TempMsgDetail"] = "查無指定項目！";
                    }
                    else
                    {
                        item.IsDelete = true;
                        item.ModifyUser = userinfo.UserID;
                        item.ModifyDate = dtnow;

                        using (var transaction = _newsService.GetTransaction())
                        {
                            try
                            {
                                //編輯
                                await _newsService.Update(item, transaction);

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
            await _commonService.OperateLog(userinfo.UserID, Feature, Action, decrypt_id, id.ToString(), _message, response, isSuccess);

            return Json(result);
        }

        /// <summary>
        /// News上下架狀態變更
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [BackendCheckLogin("Menu000020", "MODIFY")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NewsPublishChange(string id)
        {
            JsonResponse<TbNews> result = new JsonResponse<TbNews>();
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "最新消息管理", Action = "編輯";

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
                    TbNews? item = null;
                    IQueryable<TbNews>? temp = _newsService.Lookup<TbNews>(ref _message, x => x.Id == id && x.IsDelete == false);
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

                        using (var transaction = _newsService.GetTransaction())
                        {
                            try
                            {
                                //編輯
                                await _newsService.Update(item, transaction);

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
    }
}
