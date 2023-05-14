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
    public class AlbumController : BaseController
    {
        private readonly AllCommonService _allCommonService;
        private readonly CommonService _commonService;
        private readonly AlbumService _ablService;
        private readonly FileService _fileService;

        public AlbumController(AllCommonService allCommonService,
            FileService fileService,
            CommonService commonService,
            AlbumService advService)
        {
            _allCommonService = allCommonService;
            _fileService = fileService;
            _commonService = commonService;
            _ablService = advService;
        }

        /// <summary>
        /// AlbumList列表
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [BackendCheckLogin("Menu000026", "ENABLED")]
        public async Task<IActionResult> AlbumList(VM_Album data)
        {
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "活動花絮管理", Action = "檢視";

            try
            {
                //取資料
                IQueryable<AlbumExtend>? dataList = _ablService.GetAlbumExtendList(ref _message);

                //分頁
                if (dataList != null)
                    data.AlbumExtendList = await PagerInfoService.GetRange(dataList.OrderByDescending(x => x.Album.DisplayDate), data.Search.PagerInfo);

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
        /// 新增Album
        /// </summary>
        /// <returns></returns>
        [BackendCheckLogin("Menu000027", "ADD")]
        public async Task<IActionResult> AlbumAdd()
        {
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "活動花絮新增", Action = "新增";

            VM_Album data = new VM_Album();

            await _commonService.OperateLog(userinfo.UserID, Feature, Action);
            return View(data);
        }

        /// <summary>
        /// 新增Album存檔
        /// </summary>
        /// <param name="datapost"></param>
        /// <returns></returns>
        [BackendCheckLogin("Menu000027", "ADD")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AlbumAdd(VM_Album datapost)
        {
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "活動花絮新增", Action = "新增";


            // 最終動作成功與否
            bool isSuccess = false;

            // 例外錯誤發生，特別記錄至 TbLog
            bool unCaughtError = false;

            DateTime dtnow = DateTime.Now;
            TbAlbum? item = new TbAlbum();
            string? target_id = null;

            try
            {
                if (datapost.AlbumExtendItem != null)
                {
                    item.Id = await _allCommonService.IDGenerator<TbNews>();
                    item.Title = datapost.AlbumExtendItem.Album.Title;
                    item.DisplayDate = datapost.AlbumExtendItem.Album.DisplayDate;
                    item.IsDelete = false;
                    item.CreateUser = userinfo.UserID;
                    item.CreateDate = dtnow;

                    using (var transaction = _ablService.GetTransaction())
                    {
                        try
                        {
                            //輪播圖片
                            if (datapost.AlbumImageFile != null)
                            {
                                var photo_upload = await _fileService.FileUploadAsync(datapost.AlbumImageFile, "AlbumImageFiles/" + item.Id, "AlbumImageFils", item.FileId, null, transaction);
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
                            await _ablService.Insert(item, transaction);

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
                return RedirectToAction("AlbumList");
            }
            else
            {
                if (item == null)
                {
                    return RedirectToAction("AlbumList");
                }
                else
                {
                    /* 失敗回原頁 */
                    return View(datapost);
                }
            }
        }

        /// <summary>
        /// 編輯Album
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [BackendCheckLogin("Menu000028", "MODIFY")]
        public async Task<IActionResult> AlbumEdit(string id)
        {
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "活動花絮編輯", Action = "檢視";

            VM_Album data = new VM_Album();

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
                    IQueryable<AlbumExtend>? dataList = _ablService.GetAlbumxtendItem(ref _message, id);
                    if (dataList != null)
                        data.AlbumExtendItem = dataList.FirstOrDefault();

                    if (data.AlbumExtendItem == null)
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

                return RedirectToAction("AlbumList");
            }
        }

        /// <summary>
        /// 編輯Album_存檔
        /// </summary>
        /// <param name="id"></param>
        /// <param name="datapost"></param>
        /// <returns></returns>
        [BackendCheckLogin("Menu000028", "MODIFY")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AlbumEdit(string id, VM_Album datapost)
        {
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "活動花絮編輯", Action = "編輯";

            // 最終動作成功與否
            bool isSuccess = false;

            // 例外錯誤發生，特別記錄至 TbLog
            bool unCaughtError = false;

            DateTime dtnow = DateTime.Now;

            TbAlbum? item = null;

            string decrypt_id = id;

            try
            {
                if (string.IsNullOrEmpty(decrypt_id))
                {
                    TempData["TempMsgDetail"] = "金鑰逾時！請重新再操作一次！";
                }
                else
                {
                    IQueryable<TbAlbum>? temp = _ablService.Lookup<TbAlbum>(ref _message, x => x.Id == id && x.IsDelete == false);
                    if (temp != null)
                        item = temp.FirstOrDefault();

                    if (item == null)
                    {
                        TempData["TempMsgDetail"] = "查無指定項目！";
                    }
                    else if (datapost.AlbumExtendItem == null)
                    {
                        TempData["TempMsgDetail"] = "資料回傳有誤，請重新操作！";
                    }
                    else
                    {
                        item.Title = datapost.AlbumExtendItem.Album.Title;
                        item.DisplayDate = datapost.AlbumExtendItem.Album.DisplayDate;
                        item.ModifyUser = userinfo.UserID;
                        item.ModifyDate = dtnow;

                        using (var transaction = _ablService.GetTransaction())
                        {
                            try
                            {
                                //活動圖片
                                if (datapost.AlbumImageFile != null)
                                {
                                    var photo_upload = await _fileService.FileUploadAsync(datapost.AlbumImageFile, "AlbumImageFiles/" + item.Id, "AlbumImageFils", item.FileId, null, transaction);
                                    if (photo_upload.IsSuccess == true && !string.IsNullOrEmpty(photo_upload.FileID))
                                    {
                                        item.FileId = photo_upload.FileID;
                                    }
                                    else
                                    {
                                        _message += photo_upload.Message;
                                    }
                                }
                                //else
                                //{
                                //    item.FileId = "";
                                //}

                                //編輯
                                await _ablService.Update(item, transaction);

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
                return RedirectToAction("AlbumList");
            }
            else
            {
                if (item == null)
                {
                    return RedirectToAction("AlbumList");
                }
                else
                {
                    /* 失敗回原頁 */
                    return View(datapost);
                }
            }
        }

        /// <summary>
        /// 刪除Album
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [BackendCheckLogin("Menu000026", "DELETE")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AlbumDelete(string id)
        {
            JsonResponse<TbAlbum> result = new JsonResponse<TbAlbum>();
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "活動花絮管理", Action = "刪除";

            DateTime dtnow = DateTime.Now;

            // 最終動作成功與否
            bool isSuccess = false;

            // 例外錯誤發生，特別記錄至 TbLog
            bool unCaughtError = false;

            string decrypt_id = id;

            try
            {
                if (string.IsNullOrEmpty(decrypt_id))
                {
                    result.MessageDetail = "金鑰逾時！請重新再操作一次！";
                }
                else
                {
                    #region Delete
                    TbAlbum? item = null;
                    IQueryable<TbAlbum>? temp = _ablService.Lookup<TbAlbum>(ref _message, x => x.Id == id && x.IsDelete == false);
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

                        using (var transaction = _ablService.GetTransaction())
                        {
                            try
                            {
                                //編輯
                                await _ablService.Update(item, transaction);

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
    }
}
