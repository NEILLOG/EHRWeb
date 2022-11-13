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
using BASE.Areas.Frontend.Service;

namespace BASE.Areas.Backend.Controllers
{
    public class ProjectController : BaseController
    {
        private readonly AllCommonService _allCommonService;
        private readonly CommonService _commonService;
        private readonly B_ProjectService _ProjectService;
        private readonly FileService _fileService;

        public ProjectController(AllCommonService allCommonService,
            FileService fileService,
            CommonService commonService,
            B_ProjectService ProjectService)
        {
            _allCommonService = allCommonService;
            _fileService = fileService;
            _commonService = commonService;
            _ProjectService = ProjectService;
        }

        /// <summary>
        /// ProjectList列表
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [BackendCheckLogin("Menu000020", "ENABLED")]
        public async Task<IActionResult> ProjectList(VM_Project data)
        {
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "計畫管理", Action = "檢視";

            try
            {
                //取資料
                IQueryable<ProjectExtend>? dataList = _ProjectService.GetProjectExtendList(ref _message);

                if (dataList != null && !string.IsNullOrEmpty(data.Search.sName))
                { dataList = dataList.Where(x => x.Project.Name.Contains(data.Search.sName)); }

                if (dataList != null && !string.IsNullOrEmpty(data.Search.sCategory))
                { dataList = dataList.Where(x => x.Project.Category == data.Search.sCategory); }

                //分頁
                if (dataList != null)
                    data.ProjectExtendList = await PagerInfoService.GetRange(dataList.OrderByDescending(x => x.Project.CreateDate), data.Search.PagerInfo);

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
        /// 編輯Project
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [BackendCheckLogin("Menu000046", "MODIFY")]
        public async Task<IActionResult> ProjectMap()
        {
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "計畫資源地圖", Action = "檢視";

            VM_Project data = new VM_Project();

            // 最終動作成功與否
            bool isSuccess = false;

            // 例外錯誤發生，特別記錄至 TbLog
            bool unCaughtError = false;
            string id = _ProjectService.GetOnePageId(ref _message, "計畫資源地圖");
            string decrypt_id = id;

            try
            {

                if (string.IsNullOrEmpty(decrypt_id))
                {
                    TempData["TempMsgDetail"] = "金鑰逾時！請重新再操作一次！";
                }
                else
                {
                    IQueryable<OnePageExtend>? dataList = _ProjectService.GetOnePageExtendItem(ref _message, id);
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
        /// 編輯Project_存檔
        /// </summary>
        /// <param name="id"></param>
        /// <param name="datapost"></param>
        /// <returns></returns>
        [BackendCheckLogin("Menu000046", "MODIFY")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProjectMap(VM_Project datapost)
        {
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "計畫資源地圖", Action = "編輯";

            // 最終動作成功與否
            bool isSuccess = false;

            // 例外錯誤發生，特別記錄至 TbLog
            bool unCaughtError = false;

            DateTime dtnow = DateTime.Now;

            TbOnePage? item = null;
            string id = _ProjectService.GetOnePageId(ref _message, "計畫資源地圖");
            string decrypt_id = id;

            try
            {
                if (string.IsNullOrEmpty(decrypt_id))
                {
                    TempData["TempMsgDetail"] = "金鑰逾時！請重新再操作一次！";
                }
                else
                {
                    IQueryable<TbOnePage>? temp = _ProjectService.Lookup<TbOnePage>(ref _message, x => x.Id == id);
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

                        using (var transaction = _ProjectService.GetTransaction())
                        {
                            try
                            {
                                //編輯
                                await _ProjectService.Update(item, transaction);

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
                return RedirectToAction("Index", "Home");
            }
            else
            {
                if (item == null)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    /* 失敗回原頁 */
                    return View(datapost);
                }
            }
        }

        /// <summary>
        /// 新增Project
        /// </summary>
        /// <returns></returns>
        [BackendCheckLogin("Menu000021", "ADD")]
        public async Task<IActionResult> ProjectAdd()
        {
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "新增計畫", Action = "新增";

            VM_Project data = new VM_Project();
            data.ddlContact = _ProjectService.SetDDL_Contact();

            await _commonService.OperateLog(userinfo.UserID, Feature, Action);
            return View(data);
        }

        /// <summary>
        /// 新增Projecttise存檔
        /// </summary>
        /// <param name="datapost"></param>
        /// <returns></returns>
        [BackendCheckLogin("Menu000021", "ADD")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProjectAdd(VM_Project datapost)
        {
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "新增計畫", Action = "新增";


            // 最終動作成功與否
            bool isSuccess = false;

            // 例外錯誤發生，特別記錄至 TbLog
            bool unCaughtError = false;

            DateTime dtnow = DateTime.Now;
            TbProject? item = new TbProject();
            string? target_id = null;

            try
            {
                if (datapost.ProjectExtendItem != null)
                {
                    item.Id = await _allCommonService.IDGenerator<TbProject>();
                    item.Name = datapost.ProjectExtendItem.Project.Name;
                    item.Category = datapost.ProjectExtendItem.Project.Category;
                    item.Link = datapost.ProjectExtendItem.Project.Link;
                    item.Purpose = datapost.ProjectExtendItem.Project.Purpose;
                    item.Target = datapost.ProjectExtendItem.Project.Target;
                    item.Contents = datapost.ProjectExtendItem.Project.Contents;
                    item.Contact = datapost.ProjectExtendItem.Project.Contact;
                    item.IsDelete = false;
                    item.CreateUser = userinfo.UserID;
                    item.CreateDate = dtnow;

                    using (var transaction = _ProjectService.GetTransaction())
                    {
                        try
                        {

                            //新增
                            await _ProjectService.Insert(item, transaction);

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
                return RedirectToAction("ProjectList");
            }
            else
            {
                if (item == null)
                {
                    return RedirectToAction("ProjectList");
                }
                else
                {
                    /* 失敗回原頁 */
                    return View(datapost);
                }
            }
        }

        /// <summary>
        /// 編輯Project
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [BackendCheckLogin("Menu000022", "MODIFY")]
        public async Task<IActionResult> ProjectEdit(string id)
        {
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "編輯計畫", Action = "檢視";

            VM_Project data = new VM_Project();

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
                    IQueryable<ProjectExtend>? dataList = _ProjectService.GetProjectExtendItem(ref _message, id);
                    if (dataList != null)
                    {
                        data.ProjectExtendItem = dataList.FirstOrDefault();

                        data.ddlContact = _ProjectService.SetDDL_Contact();
                    }

                    if (data.ProjectExtendItem == null)
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

                return RedirectToAction("ProjectList");
            }
        }

        /// <summary>
        /// 編輯Project_存檔
        /// </summary>
        /// <param name="id"></param>
        /// <param name="datapost"></param>
        /// <returns></returns>
        [BackendCheckLogin("Menu000022", "MODIFY")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProjectEdit(string id, VM_Project datapost)
        {
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "編輯計畫", Action = "編輯";

            // 最終動作成功與否
            bool isSuccess = false;

            // 例外錯誤發生，特別記錄至 TbLog
            bool unCaughtError = false;

            DateTime dtnow = DateTime.Now;

            TbProject? item = null;

            string decrypt_id = id;

            try
            {
                if (string.IsNullOrEmpty(decrypt_id))
                {
                    TempData["TempMsgDetail"] = "金鑰逾時！請重新再操作一次！";
                }
                else
                {
                    IQueryable<TbProject>? temp = _ProjectService.Lookup<TbProject>(ref _message, x => x.Id == id && x.IsDelete == false);
                    if (temp != null)
                        item = temp.FirstOrDefault();

                    if (item == null)
                    {
                        TempData["TempMsgDetail"] = "查無指定項目！";
                    }
                    else if (datapost.ProjectExtendItem == null)
                    {
                        TempData["TempMsgDetail"] = "資料回傳有誤，請重新操作！";
                    }
                    else
                    {
                        item.Id = id;
                        item.Name = datapost.ProjectExtendItem.Project.Name;
                        item.Category = datapost.ProjectExtendItem.Project.Category;
                        item.Link = datapost.ProjectExtendItem.Project.Link;
                        item.Purpose = datapost.ProjectExtendItem.Project.Purpose;
                        item.Target = datapost.ProjectExtendItem.Project.Target;
                        item.Contents = datapost.ProjectExtendItem.Project.Contents;
                        item.Contact = datapost.ProjectExtendItem.Project.Contact;
                        item.ModifyUser = userinfo.UserID;
                        item.ModifyDate = dtnow;

                        using (var transaction = _ProjectService.GetTransaction())
                        {
                            try
                            {
                                //編輯
                                await _ProjectService.Update(item, transaction);

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
                return RedirectToAction("ProjectList");
            }
            else
            {
                if (item == null)
                {
                    return RedirectToAction("ProjectList");
                }
                else
                {
                    /* 失敗回原頁 */
                    return View(datapost);
                }
            }
        }

        /// <summary>
        /// 刪除Project
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [BackendCheckLogin("Menu000047", "DELETE")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProjectDelete(string id)
        {
            JsonResponse<TbProject> result = new JsonResponse<TbProject>();
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "計畫管理", Action = "刪除";

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
                    TbProject? item = null;
                    IQueryable<TbProject>? temp = _ProjectService.Lookup<TbProject>(ref _message, x => x.Id == id && x.IsDelete == false);
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

                        using (var transaction = _ProjectService.GetTransaction())
                        {
                            try
                            {
                                //編輯
                                await _ProjectService.Update(item, transaction);

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
        public async Task<IActionResult> ProjectContactChange(string id)
        {
            JsonResponse<TbUserInfo> result = new JsonResponse<TbUserInfo>();
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "計畫管理", Action = "編輯";

            DateTime dtnow = DateTime.Now;

            // 最終動作成功與否
            bool isSuccess = false;

            // 例外錯誤發生，特別記錄至 TbLog
            bool unCaughtError = false;

            string decrypt_id = (id ?? "").ToString();

            try
            {
                if (string.IsNullOrEmpty(decrypt_id))
                {
                    result.MessageDetail = "金鑰逾時！請重新再操作一次！";
                }
                else
                {
                    TbUserInfo? item = null;
                    IQueryable<TbUserInfo>? temp = _ProjectService.Lookup<TbUserInfo>(ref _message, x => x.UserId == id && x.IsDelete == false);
                    if (temp != null)
                        result.Datas = temp.FirstOrDefault();

                    if (result.Datas == null)
                    {
                        TempData["TempMsgDetail"] = "查無指定項目！";
                    }
                    else
                    {
                        isSuccess = true;
                        //result.Datas = new TbUserInfo();
                        //result.Datas.Phone = item.Phone;
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

            return Json(result.Datas.Phone);
        }

    }
}
