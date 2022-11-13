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
    public class ProjectModifyController : BaseController
    {
        private readonly AllCommonService _allCommonService;
        private readonly CommonService _commonService;
        private readonly B_ProjectModifyService _ProjectModifyService;
        private readonly FileService _fileService;

        public ProjectModifyController(AllCommonService allCommonService,
            FileService fileService,
            CommonService commonService,
            B_ProjectModifyService ProjectModifyService)
        {
            _allCommonService = allCommonService;
            _fileService = fileService;
            _commonService = commonService;
            _ProjectModifyService = ProjectModifyService;
        }

        /// <summary>
        /// ProjectModifyList列表
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [BackendCheckLogin("Menu000020", "ENABLED")]
        public async Task<IActionResult> ProjectModifyList(string id)
        {
            VM_ProjectModify data = new VM_ProjectModify();
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "課程變更管理", Action = "檢視";

            try
            {
                //取資料
                IQueryable<ProjectModifyExtend>? dataList = _ProjectModifyService.GetProjectModifyExtendList(ref _message, id);

                //分頁
                if (dataList != null)
                    data.ProjectModifyExtendList = await PagerInfoService.GetRange(dataList.OrderByDescending(x => x.ProjectModify.CreateDate), data.Search.PagerInfo);

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
        /// 編輯ProjectModify_存檔
        /// </summary>
        /// <param name="id"></param>
        /// <param name="datapost"></param>
        /// <returns></returns>
        [BackendCheckLogin("Menu000009", "MODIFY")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProjectModifyList(long id, VM_ProjectModify datapost)
        {
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "課程變更管理", Action = "編輯";

            // 最終動作成功與否
            bool isSuccess = false;

            // 例外錯誤發生，特別記錄至 TbLog
            bool unCaughtError = false;

            DateTime dtnow = DateTime.Now;

            TbProjectModify? item = null;

            string decrypt_id = id.ToString();

            try
            {
                if (string.IsNullOrEmpty(decrypt_id))
                {
                    TempData["TempMsgDetail"] = "金鑰逾時！請重新再操作一次！";
                }
                else
                {
                    IQueryable<TbProjectModify>? temp = _ProjectModifyService.Lookup<TbProjectModify>(ref _message, x => x.Id == id);
                    if (temp != null)
                        item = temp.FirstOrDefault();

                    if (item == null)
                    {
                        TempData["TempMsgDetail"] = "查無指定項目！";
                    }
                    else if (datapost.ProjectModifyExtendItem == null)
                    {
                        TempData["TempMsgDetail"] = "資料回傳有誤，請重新操作！";
                    }
                    else
                    {
                        if (datapost.IsApprove == "同意")
                        {
                            item.IsApprove = true;
                        }
                        else if (datapost.IsApprove == "不同意")
                        {
                            item.IsApprove = false;
                        }
                        //item.ModifyUser = userinfo.UserID;
                        item.ModifyDate = dtnow;

                        using (var transaction = _ProjectModifyService.GetTransaction())
                        {
                            try
                            {
                                //編輯
                                await _ProjectModifyService.Update(item, transaction);

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
                return RedirectToAction("ProjectModifyList");
            }
            else
            {
                if (item == null)
                {
                    return RedirectToAction("ProjectModifyList");
                }
                else
                {
                    /* 失敗回原頁 */
                    return View(datapost);
                }
            }
        }



        /// <summary>
        /// ProjectModify上下架狀態變更
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [BackendCheckLogin("Menu000009", "MODIFY")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProjectModifySave(long id, string ApproveStatus)
        {
            JsonResponse<TbProjectModify> result = new JsonResponse<TbProjectModify>();
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
                    TbProjectModify? item = null;
                    IQueryable<TbProjectModify>? temp = _ProjectModifyService.Lookup<TbProjectModify>(ref _message, x => x.Id == id);
                    if (temp != null)
                        item = temp.FirstOrDefault();

                    if (item == null)
                    {
                        TempData["TempMsgDetail"] = "查無指定項目！";
                    }
                    else
                    {
                        if (ApproveStatus == "同意")
                        { item.IsApprove = true; }
                        else if (ApproveStatus == "不同意")
                        { item.IsApprove = false; }
                        //item.ModifyUser = userinfo.UserID;
                        item.ModifyDate = dtnow;

                        using (var transaction = _ProjectModifyService.GetTransaction())
                        {
                            try
                            {
                                //編輯
                                await _ProjectModifyService.Update(item, transaction);

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
