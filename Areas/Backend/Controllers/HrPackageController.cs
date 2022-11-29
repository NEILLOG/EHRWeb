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
//using BASE.Areas.Frontend.Service;

namespace BASE.Areas.Backend.Controllers
{
    public class HrPackageController : BaseController
    {
        private readonly AllCommonService _allCommonService;
        private readonly CommonService _commonService;
        private readonly HrPackageService _HrPackageService;
        private readonly FileService _fileService;

        public HrPackageController(AllCommonService allCommonService,
            FileService fileService,
            CommonService commonService,
            HrPackageService HrPackageService)
        {
            _allCommonService = allCommonService;
            _fileService = fileService;
            _commonService = commonService;
            _HrPackageService = HrPackageService;
        }

        /// <summary>
        /// HrPackageList列表
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [BackendCheckLogin("Menu000020", "ENABLED")]
        public async Task<IActionResult> HrPackageList(VM_HrPackage data)
        {
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "HR材料包管理", Action = "檢視";

            try
            {
                string sYear = DateTime.Now.Year.ToString();
                string eTear = (DateTime.Now.Year + 1).ToString();

                //取資料
                IQueryable<HrPackageExtend>? dataList = _HrPackageService.GetHrPackageExtendList(ref _message);
                if (dataList != null && !string.IsNullOrEmpty(data.Search.sTitle))
                { dataList = dataList.Where(x => x.HrPackage.Title.Contains(data.Search.sTitle)); }


                //分頁
                if (dataList != null)
                    data.HrPackageExtendList = await PagerInfoService.GetRange(dataList.OrderBy(x => x.HrPackage.Sort).ThenByDescending(x => x.HrPackage.CreateDate), data.Search.PagerInfo);

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
        /// 編輯HrPackage
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [BackendCheckLogin("Menu000035", "MODIFY")]
        public async Task<IActionResult> HrManual()
        {
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "HR工具說明書", Action = "檢視";

            VM_HrPackage data = new VM_HrPackage();

            // 最終動作成功與否
            bool isSuccess = false;

            // 例外錯誤發生，特別記錄至 TbLog
            bool unCaughtError = false;
            string id = _HrPackageService.GetOnePageId(ref _message, "HR工具說明書");
            string decrypt_id = id;

            try
            {

                if (string.IsNullOrEmpty(decrypt_id))
                {
                    TempData["TempMsgDetail"] = "金鑰逾時！請重新再操作一次！";
                }
                else
                {
                    IQueryable<OnePageExtend>? dataList = _HrPackageService.GetOnePageExtendItem(ref _message, id);
                    if (dataList != null)
                    {
                        data.OnePageExtendItem = dataList.FirstOrDefault();
                    }

                    if (data.OnePageExtendItem == null)
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

                return RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// 編輯HrPackage_存檔
        /// </summary>
        /// <param name="id"></param>
        /// <param name="datapost"></param>
        /// <returns></returns>
        [BackendCheckLogin("Menu000035", "MODIFY")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HrManual(VM_HrPackage datapost)
        {
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "HR工具說明書", Action = "編輯";

            // 最終動作成功與否
            bool isSuccess = false;

            // 例外錯誤發生，特別記錄至 TbLog
            bool unCaughtError = false;

            DateTime dtnow = DateTime.Now;

            TbOnePage? item = null;
            string id = _HrPackageService.GetOnePageId(ref _message, "HR工具說明書");
            string decrypt_id = id;

            try
            {
                if (string.IsNullOrEmpty(decrypt_id))
                {
                    TempData["TempMsgDetail"] = "金鑰逾時！請重新再操作一次！";
                }
                else
                {
                    IQueryable<TbOnePage>? temp = _HrPackageService.Lookup<TbOnePage>(ref _message, x => x.Id == id);
                    if (temp != null)
                        item = temp.FirstOrDefault();

                    if (item == null)
                    {
                        TempData["TempMsgDetail"] = "查無指定項目！";
                    }
                    else if (datapost.OnePageExtendItem == null)
                    {
                        TempData["TempMsgDetail"] = "資料回傳有誤，請重新操作！";
                    }
                    else
                    {
                        item.Contents = datapost.OnePageExtendItem.OnePage.Contents;
                        item.ModifyUser = userinfo.UserID;
                        item.ModifyDate = dtnow;

                        using (var transaction = _HrPackageService.GetTransaction())
                        {
                            try
                            {
                                //編輯
                                await _HrPackageService.Update(item, transaction);

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
                return RedirectToAction("HrManual");
            }
            else
            {
                if (item == null)
                {
                    return RedirectToAction("HrManual");
                }
                else
                {
                    /* 失敗回原頁 */
                    return View(datapost);
                }
            }
        }

        /// <summary>
        /// 新增HrPackage
        /// </summary>
        /// <returns></returns>
        [BackendCheckLogin("Menu000021", "ADD")]
        public async Task<IActionResult> HrPackageAdd()
        {
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "新增HR材料包資料", Action = "新增";

            VM_HrPackage data = new VM_HrPackage();
            await _commonService.OperateLog(userinfo.UserID, Feature, Action);
            return View(data);
        }

        /// <summary>
        /// 新增HrPackagetise存檔
        /// </summary>
        /// <param name="datapost"></param>
        /// <returns></returns>
        [BackendCheckLogin("Menu000021", "ADD")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HrPackageAdd(VM_HrPackage datapost)
        {
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "新增HR材料包資料", Action = "新增";


            // 最終動作成功與否
            bool isSuccess = false;

            // 例外錯誤發生，特別記錄至 TbLog
            bool unCaughtError = false;

            DateTime dtnow = DateTime.Now;
            TbHrPackage? item = new TbHrPackage();
            string? target_id = null;

            try
            {
                if (datapost.HrPackageExtendItem != null)
                {
                    item.Id = await _allCommonService.IDGenerator<TbHrPackage>();
                    item.Title = datapost.HrPackageExtendItem.HrPackage.Title;
                    item.DisplayDate = datapost.HrPackageExtendItem.HrPackage.DisplayDate;
                    item.IsDelete = false;
                    item.IsPublish = true; 

                    item.Sort = (_HrPackageService.GetHrPackageMAXSort(ref _message) ?? 0) + 1;
                    item.CreateUser = userinfo.UserID;
                    item.CreateDate = dtnow;

                    using (var transaction = _HrPackageService.GetTransaction())
                    {
                        try
                        {
                            //相關檔案
                            if (datapost.RelatedFile != null)
                            {
                                var photo_upload = await _fileService.FileUploadAsync(datapost.RelatedFile, "HrPackageFiles/" + item.Id, "HrPackageFiles", item.FileId, null, transaction);
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
                            await _HrPackageService.Insert(item, transaction);

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
                return RedirectToAction("HrPackageList");
            }
            else
            {
                if (item == null)
                {
                    return RedirectToAction("HrPackageList");
                }
                else
                {
                    /* 失敗回原頁 */
                    return View(datapost);
                }
            }
        }

        /// <summary>
        /// 編輯HrPackage
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [BackendCheckLogin("Menu000022", "MODIFY")]
        public async Task<IActionResult> HrPackageEdit(string id)
        {
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "編輯HR材料包資料", Action = "檢視";

            VM_HrPackage data = new VM_HrPackage();

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
                    IQueryable<HrPackageExtend>? dataList = _HrPackageService.GetHrPackageExtendItem(ref _message, id);
                    if (dataList != null)
                        data.HrPackageExtendItem = dataList.FirstOrDefault();

                    if (data.HrPackageExtendItem == null)
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

                return RedirectToAction("HrPackageList");
            }
        }

        /// <summary>
        /// 編輯HrPackage_存檔
        /// </summary>
        /// <param name="id"></param>
        /// <param name="datapost"></param>
        /// <returns></returns>
        [BackendCheckLogin("Menu000022", "MODIFY")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HrPackageEdit(string id, VM_HrPackage datapost)
        {
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "編輯HR材料包資料", Action = "編輯";

            // 最終動作成功與否
            bool isSuccess = false;

            // 例外錯誤發生，特別記錄至 TbLog
            bool unCaughtError = false;

            DateTime dtnow = DateTime.Now;

            TbHrPackage? item = null;

            string decrypt_id = id;

            try
            {
                if (string.IsNullOrEmpty(decrypt_id))
                {
                    TempData["TempMsgDetail"] = "金鑰逾時！請重新再操作一次！";
                }
                else
                {
                    IQueryable<TbHrPackage>? temp = _HrPackageService.Lookup<TbHrPackage>(ref _message, x => x.Id == id && x.IsDelete == false);
                    if (temp != null)
                        item = temp.FirstOrDefault();

                    if (item == null)
                    {
                        TempData["TempMsgDetail"] = "查無指定項目！";
                    }
                    else if (datapost.HrPackageExtendItem == null)
                    {
                        TempData["TempMsgDetail"] = "資料回傳有誤，請重新操作！";
                    }
                    else
                    {
                        item.Id = id;
                        item.Title = datapost.HrPackageExtendItem.HrPackage.Title;
                        item.DisplayDate = datapost.HrPackageExtendItem.HrPackage.DisplayDate;
                        item.ModifyUser = userinfo.UserID;
                        item.ModifyDate = dtnow;

                        using (var transaction = _HrPackageService.GetTransaction())
                        {
                            try
                            {
                                //相關檔案
                                if (datapost.RelatedFile != null)
                                {
                                    var photo_upload = await _fileService.FileUploadAsync(datapost.RelatedFile, "HrPackageFiles/" + item.Id, "HrPackageFiles", item.FileId, null, transaction);
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
                                await _HrPackageService.Update(item, transaction);

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
                return RedirectToAction("HrPackageList");
            }
            else
            {
                if (item == null)
                {
                    return RedirectToAction("HrPackageList");
                }
                else
                {
                    /* 失敗回原頁 */
                    return View(datapost);
                }
            }
        }

        /// <summary>
        /// 刪除HrPackage
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [BackendCheckLogin("Menu000020", "DELETE")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HrPackageDelete(string id)
        {
            JsonResponse<TbHrPackage> result = new JsonResponse<TbHrPackage>();
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "HR材料包管理", Action = "刪除";

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
                    TbHrPackage? item = null;
                    IQueryable<TbHrPackage>? temp = _HrPackageService.Lookup<TbHrPackage>(ref _message, x => x.Id == id && x.IsDelete == false);
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

                        using (var transaction = _HrPackageService.GetTransaction())
                        {
                            try
                            {
                                //編輯
                                await _HrPackageService.Update(item, transaction);

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
                    List<TbHrPackage>? itemReSort = null;
                    IQueryable<TbHrPackage>? tempReSort = _HrPackageService.Lookup<TbHrPackage>(ref _message, x => x.IsDelete == false).OrderBy(x => x.Sort);
                    if (tempReSort != null)
                        itemReSort = tempReSort.ToList();

                    int ReSortIndex = 1;

                    if (itemReSort.Any())
                    {
                        foreach (var itemRe in itemReSort)
                        {
                            itemRe.Sort = ReSortIndex;

                            using (var transaction = _HrPackageService.GetTransaction())
                            {
                                try
                                {
                                    //編輯
                                    await _HrPackageService.Update(itemRe, transaction);

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
        /// HrPackage上下架狀態變更
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [BackendCheckLogin("Menu000020", "MODIFY")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HrPackagePublishChange(string id)
        {
            JsonResponse<TbHrPackage> result = new JsonResponse<TbHrPackage>();
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "HR材料包管理", Action = "編輯";

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
                    TbHrPackage? item = null;
                    IQueryable<TbHrPackage>? temp = _HrPackageService.Lookup<TbHrPackage>(ref _message, x => x.Id == id && x.IsDelete == false);
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

                        using (var transaction = _HrPackageService.GetTransaction())
                        {
                            try
                            {
                                //編輯
                                await _HrPackageService.Update(item, transaction);

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
        /// 上移HrPackage
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [BackendCheckLogin("Menu000009", "MODIFY")]
        public async Task<IActionResult> HrPackageMoveUp(int id)
        {
            JsonResponse<TbHrPackage> result = new JsonResponse<TbHrPackage>();
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "HR材料包管理", Action = "編輯";

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
                    TbHrPackage? item = null;
                    TbHrPackage? itemchange = null;

                    IQueryable<TbHrPackage>? temp = _HrPackageService.Lookup<TbHrPackage>(ref _message, x => x.Sort == id && x.IsDelete == false);
                    IQueryable<TbHrPackage>? tempchange = _HrPackageService.Lookup<TbHrPackage>(ref _message, x => x.Sort == id - 1 && x.IsDelete == false);

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

                        using (var transaction = _HrPackageService.GetTransaction())
                        {
                            try
                            {
                                //編輯
                                await _HrPackageService.Update(item, transaction);
                                await _HrPackageService.Update(itemchange, transaction);

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

            return RedirectToAction("HrPackageList");
        }

        /// <summary>
        /// 上移HrPackage
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [BackendCheckLogin("Menu000009", "MODIFY")]
        public async Task<IActionResult> HrPackageMoveDown(int id)
        {
            JsonResponse<TbHrPackage> result = new JsonResponse<TbHrPackage>();
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "HR材料包管理", Action = "編輯";

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
                    TbHrPackage? item = null;
                    TbHrPackage? itemchange = null;

                    IQueryable<TbHrPackage>? temp = _HrPackageService.Lookup<TbHrPackage>(ref _message, x => x.Sort == id && x.IsDelete == false);
                    IQueryable<TbHrPackage>? tempchange = _HrPackageService.Lookup<TbHrPackage>(ref _message, x => x.Sort == id + 1 && x.IsDelete == false);

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

                        using (var transaction = _HrPackageService.GetTransaction())
                        {
                            try
                            {
                                //編輯
                                await _HrPackageService.Update(item, transaction);
                                await _HrPackageService.Update(itemchange, transaction);

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

            return RedirectToAction("HrPackageList");
        }
    }
}
