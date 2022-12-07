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
using Microsoft.AspNetCore.Mvc.Rendering;
using NuGet.Packaging;

namespace BASE.Areas.Backend.Controllers
{
    public class QuizController : BaseController
    {
        private readonly AllCommonService _allCommonService;
        private readonly CommonService _commonService;
        private readonly QuizService _quizService;
        private readonly MailService _mailService;

        public QuizController(AllCommonService allCommonService,
            CommonService commonService,
            QuizService quziService,
            MailService mailService) 
        {
            _allCommonService = allCommonService;
            _commonService = commonService;
            _quizService = quziService;
            _mailService = mailService;
        }

        [BackendCheckLogin("Menu000057", "ENABLED")]
        public async Task<IActionResult> List(VM_Quiz data)
        {
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);

            string Feature = "問卷管理", Action = "檢視";

            try
            {
                //取資料
                IQueryable<QuizExtend>? dataList = _quizService.GetExtendList(ref _message, data.Search);

                //分頁
                if (dataList != null)
                    data.ExtendList = await PagerInfoService.GetRange(dataList.OrderByDescending(x => x.Header.CreateDate), data.Search.PagerInfo);

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

        /// <param name="id">選擇的範本編號</param>
        [BackendCheckLogin("Menu000057", "ADD")]
        public async Task<IActionResult> Add(String id = "")
        {
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "問卷管理", Action = "檢視";

            var ExistQuizs = _quizService.Lookup<TbQuiz>(ref _message, x => x.IsDelete == false).Select(x => new { x.Name, x.Id }).ToList();
            VM_Quiz data = new VM_Quiz();
                    data.ExtendItem = new QuizExtend() { Lines = new List<TbQuizOption>() };
                    data.ddlExistQuiz = new List<SelectListItem>() { new SelectListItem() {  Text="請選擇", Value="" } };
                    data.ddlExistQuiz.AddRange(ExistQuizs.Select(x => new SelectListItem() { Text = x.Name, Value = EncryptService.AES.RandomizedEncrypt(x.Id) }));

            if (!String.IsNullOrEmpty(id))
            {
                string decrypt_id = EncryptService.AES.RandomizedDecrypt(id);
                var ExistQuiz = _quizService.GetExtendItem(ref _message, decrypt_id);
                if(ExistQuiz != null)
                {
                    //只複製必要欄位過去即可，避免copy整個模型連ID都copy過去了
                    data.ExtendItem = new QuizExtend() { Header = new TbQuiz(), Lines = new List<TbQuizOption>()};
                    data.ExtendItem.Header.Name = ExistQuiz.Header.Name;
                    data.ExtendItem.Header.Description = ExistQuiz.Header.Description;
                    foreach(var line in ExistQuiz.Lines)
                        data.ExtendItem.Lines.Add(new TbQuizOption()
                        {
                            Id = Guid.NewGuid(),
                            Type = line.Type,
                            QuizDescription = line.QuizDescription,
                            FillDirection = line.FillDirection,
                            Options = line.Options,
                            IsRequired= line.IsRequired
                        });
                }
            }

            await _commonService.OperateLog(userinfo.UserID, Feature, Action);
            return View(data);
        }

        [BackendCheckLogin("Menu000057", "ADD")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(VM_Quiz datapost)
        {
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "問卷管理", Action = "新增";

            // 最終動作成功與否
            bool isSuccess = false;

            // 例外錯誤發生，特別記錄至 TbLog
            bool unCaughtError = false;

            DateTime dtnow = DateTime.Now;
            TbQuiz? item = new TbQuiz();
            string? target_id = null;

            try
            {
                if (datapost.ExtendItem != null)
                {
                    item.Id = await _allCommonService.IDGenerator<TbQuiz>();
                    item.IsDelete = false;
                    item.Name = datapost.ExtendItem.Header.Name;
                    item.Description = datapost.ExtendItem.Header.Description;
                    item.CreateUser = userinfo.UserID;
                    item.CreateDate = dtnow;

                    Int32 sort = 1;
                    foreach (var line in datapost.ExtendItem.Lines)
                    {
                        line.QuizId = item.Id;
                        line.Sort = sort++;
                    }

                    using (var transaction = _quizService.GetTransaction())
                    {
                        try
                        {
                            //新增
                            await _quizService.Insert(item, transaction);

                            foreach (var line in datapost.ExtendItem.Lines)
                                await _quizService.Insert(line, transaction);

                            transaction.Commit();
                            isSuccess = true;
                            target_id = item.Id;
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
                return RedirectToAction("Edit", new { id = EncryptService.AES.RandomizedEncrypt(item.Id) });
            }
            else
            {
                if (item == null)
                {
                    return RedirectToAction("List");
                }
                else
                {
                    /* 失敗回原頁 */
                    var ExistQuizs = _quizService.Lookup<TbQuiz>(ref _message, x => x.IsDelete == false).Select(x => new { x.Name, x.Id }).ToList();
                    datapost.ddlExistQuiz = new List<SelectListItem>() { new SelectListItem() { Text = "請選擇", Value = "" } };
                    datapost.ddlExistQuiz.AddRange(ExistQuizs.Select(x => new SelectListItem() { Text = x.Name, Value = EncryptService.AES.RandomizedEncrypt(x.Id) }));

                    return View(datapost);
                }
            }
        }

        [BackendCheckLogin("Menu000057", "MODIFY")]
        public async Task<IActionResult> Edit(string id)
        {
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "問卷管理", Action = "編輯";

            VM_Quiz data = new VM_Quiz();

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
                    QuizExtend? dataList = _quizService.GetExtendItem(ref _message, decrypt_id);
                    if (dataList != null)
                        data.ExtendItem = dataList;

                    if (data.ExtendItem == null)
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

                return RedirectToAction("FAQList");
            }
        }

        [BackendCheckLogin("Menu000057", "MODIFY")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, VM_Quiz datapost)
        {
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "問卷管理", Action = "編輯";

            // 最終動作成功與否
            bool isSuccess = false;

            // 例外錯誤發生，特別記錄至 TbLog
            bool unCaughtError = false;

            DateTime dtnow = DateTime.Now;

            TbQuiz? item = null;

            string decrypt_id = EncryptService.AES.RandomizedDecrypt(id);

            try
            {
                if (string.IsNullOrEmpty(decrypt_id))
                {
                    TempData["TempMsgDetail"] = "金鑰逾時！請重新再操作一次！";
                }
                else
                {
                    IQueryable<TbQuiz>? temp = _quizService.Lookup<TbQuiz>(ref _message, x => x.Id == decrypt_id && x.IsDelete == false);
                    if (temp != null)
                        item = await temp.SingleOrDefaultAsync();

                    if (item == null)
                    {
                        TempData["TempMsgDetail"] = "查無指定項目！";
                    }
                    else if (datapost.ExtendItem == null)
                    {
                        TempData["TempMsgDetail"] = "資料回傳有誤，請重新操作！";
                    }
                    else
                    {
                        List<TbQuizOption> options = _quizService.Lookup<TbQuizOption>(ref _message, x => x.QuizId == decrypt_id).ToList();

                        item.Name = datapost.ExtendItem.Header.Name;
                        item.Description = datapost.ExtendItem.Header.Description;
                        item.ModifyUser = userinfo.UserID;
                        item.ModifyDate = dtnow;

                        using (var transaction = _quizService.GetTransaction())
                        {
                            try
                            {
                                //移除所有選項
                                await _quizService.DeleteRange(options, transaction);

                                //新增所有選項
                                Int32 sort = 1;
                                foreach (var line in datapost.ExtendItem.Lines)
                                {
                                    line.QuizId = item.Id;
                                    line.Sort = sort++;
                                    await _quizService.Insert(line, transaction);
                                }

                                //編輯
                                await _quizService.Update(item, transaction);

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
                return RedirectToAction("Edit", new { id = EncryptService.AES.RandomizedEncrypt(item.Id) });
            }
            else
            {
                if (item == null)
                {
                    return RedirectToAction("List");
                }
                else
                {
                    /* 失敗回原頁 */
                    return View(datapost);
                }
            }
        }

        [BackendCheckLogin("Menu000057", "DELETE")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            JsonResponse<TbQuiz> result = new JsonResponse<TbQuiz>();
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "問卷管理", Action = "刪除";

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
                    TbQuiz? item = null;
                    IQueryable<TbQuiz>? temp = _quizService.Lookup<TbQuiz>(ref _message, x => x.Id == decrypt_id && x.IsDelete == false);
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

                        using (var transaction = _quizService.GetTransaction())
                        {
                            try
                            {
                                //編輯
                                await _quizService.Update(item, transaction);

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


        [BackendCheckLogin("Menu000057", "MODIFY")]
        public async Task<IActionResult> Preview(string id)
        {
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "問卷管理", Action = "預覽";

            VM_Quiz data = new VM_Quiz();

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
                    QuizExtend? dataList = _quizService.GetExtendItem(ref _message, decrypt_id);
                    if (dataList != null)
                        data.ExtendItem = dataList;

                    if (data.ExtendItem == null)
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

                return RedirectToAction("FAQList");
            }
        }

    }
}
