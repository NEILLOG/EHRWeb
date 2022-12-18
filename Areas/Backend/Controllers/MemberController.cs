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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using NPOI.SS.UserModel;

namespace BASE.Areas.Backend.Controllers
{
    public class MemberController : BaseController
    {
        private readonly MemberService _accountService;
        private readonly AllCommonService _allCommonService;
        private readonly CommonService _commonService;
        private readonly ExportService _exportService;
        private readonly ImportService _importService;
        private readonly IConfiguration _config;

        public MemberController(MemberService accountService,
            AllCommonService allCommonService,
            CommonService commonService,
            ExportService exportService,
            ImportService importService,
            IConfiguration configuration)
        {
            _allCommonService = allCommonService;
            _commonService = commonService;
            _accountService = accountService;
            _exportService = exportService;
            _importService = importService;
            _config = configuration;
        }

        #region 帳號管理

        /// <summary>
        /// 帳號列表
        /// </summary>
        /// <returns></returns>
        [BackendCheckLogin("Menu000003", "ENABLED")]
        public async Task<IActionResult> MemberList(VM_Member data) 
        {
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "帳號管理", Action = "檢視";

            try {
                // 下拉群組
                data.ddlGroup = _accountService.SetDDL_Group(2);

                //取資料
                IQueryable<MemberExtend>? dataList = _accountService.GetAccountExtendList(ref _message, data.Search);

                //分頁
                if (dataList != null)
                    data.MemberExtendList = await PagerInfoService.GetRange(dataList, data.Search.PagerInfo);

                //操作紀錄
                await _commonService.OperateLog(userinfo.UserID, Feature, Action, null, data);
            } catch (Exception ex) {

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
        /// 新增帳號
        /// </summary>
        /// <returns></returns>
        [BackendCheckLogin("Menu000003", "ADD")]
        public async Task<IActionResult> MemberAdd()
        {
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "新增帳號", Action = "檢視";

            VM_Member data = new VM_Member();

            // 下拉群組
            data.ddlGroup = _accountService.SetDDL_Group(1);

            // radioButton:專業領域
            data.chbProfessionalField = _accountService.SetDDL_ProfessionalField(0);

            await _commonService.OperateLog(userinfo.UserID, Feature, Action);
            return View(data);
        }

        /// <summary>
        /// POST新增帳號
        /// </summary>
        /// <returns></returns>
        [BackendCheckLogin("Menu000003", "ADD")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MemberAdd(VM_Member datapost)
        {

            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "新增帳號", Action = "新增";

            // 最終動作成功與否
            bool isSuccess = false;

            // 例外錯誤發生，特別記錄至 TbLog
            bool unCaughtError = false;

            DateTime dtnow = DateTime.Now;
            TbUserInfo? item = new TbUserInfo();
            TbUserInGroup? itemGroup = new TbUserInGroup();
            List<TbUserRight>? listUserInRight = new List<TbUserRight>();
            List<TbUserInfoExperience>? listUserEXP = new List<TbUserInfoExperience>(); 
            string? target_id = null;

            try
            {
                if (datapost.MemberExtendItem != null)
                {
                    // 判斷帳號是否重複
                    bool isRepeatAcc = _allCommonService.Lookup<TbUserInfo>(ref _message, x => x.Account == datapost.MemberExtendItem.userinfo.Account && !x.IsDelete).Any();
                    if (isRepeatAcc)
                    {
                        // 下拉群組
                        datapost.ddlGroup = _accountService.SetDDL_Group(1);
                        // radioButton:專業領域
                        datapost.chbProfessionalField = _accountService.SetDDL_ProfessionalField(0);

                        TempData["TempMsgType"] = MsgTypeEnum.error;
                        TempData["TempMsg"] = TempData["TempMsg"] ?? "帳號已重複";

                        /* 失敗回原頁 */
                        return View(datapost);
                    }

                    /* 配置 key */
                    item.UserId = await _allCommonService.IDGenerator<TbUserInfo>();
                    item.Account = datapost.MemberExtendItem.userinfo.Account;
                    item.Aua8 =  EncryptService.AES.Base64Encrypt(datapost.MemberExtendItem.userinfo.Aua8);
                    item.UserName = datapost.MemberExtendItem.userinfo.UserName;
                    item.Phone = datapost.MemberExtendItem.userinfo.Phone;
                    item.Email = datapost.MemberExtendItem.userinfo.Email;
                    item.IsActive = datapost.MemberExtendItem.userinfo.IsActive;
                    item.IsDelete = false;
                    item.CellPhone = datapost.MemberExtendItem.userinfo.CellPhone;
                    if (datapost.Search.sGroup == _config.GetValue<string>("Site:ConsultantGroupID"))
                    {
                        // 顧問選項
                        item.Sex = datapost.MemberExtendItem.userinfo.Sex;
                        item.JobTitle = datapost.MemberExtendItem.userinfo.JobTitle;
                        item.IdNumber = datapost.MemberExtendItem.userinfo.IdNumber;
                        item.Industry = datapost.MemberExtendItem.userinfo.Industry;
                        item.ServiceUnit = datapost.MemberExtendItem.userinfo.ServiceUnit;
                        item.ContactAddr = datapost.MemberExtendItem.userinfo.ContactAddr;
                        item.PermanentAddr = datapost.MemberExtendItem.userinfo.PermanentAddr;
                        item.Education = datapost.MemberExtendItem.userinfo.Education;
                        item.Expertise = datapost.MemberExtendItem.userinfo.Expertise;
                        string strSkill = "";
                        foreach (var itemSkill in datapost.chbProfessionalField.Where(x => x.Selected))
                        {
                            strSkill += string.IsNullOrEmpty(strSkill) ? itemSkill.Value : string.Concat(",",itemSkill.Value);
                        }
                        item.Skill = strSkill;

                        if (datapost.MemberExtendItem.listUserEXP != null && datapost.MemberExtendItem.listUserEXP.Any())
                        {
                            foreach (var itemUserEXP in datapost.MemberExtendItem.listUserEXP)
                            {
                                TbUserInfoExperience newUserEXP = new TbUserInfoExperience();
                                newUserEXP.UserId = item.UserId;
                                newUserEXP.ServDepartment = itemUserEXP.ServDepartment;
                                newUserEXP.JobTitle = itemUserEXP.JobTitle;
                                newUserEXP.Durinration = itemUserEXP.Durinration;
                                listUserEXP.Add(newUserEXP);
                            }
                        }

                    }
                    item.CreateUser = userinfo.UserID;
                    item.CreateDate = dtnow;

                    // 新增對應的USER群組
                    itemGroup.UserId = item.UserId;
                    itemGroup.GroupId = datapost.Search.sGroup;
                    // 新增對應的USER 權限
                    List<TbGroupRight> listGroup = new List<TbGroupRight>();
                    listGroup = _accountService.Lookup<TbGroupRight>(ref _message, x => x.GroupId == datapost.Search.sGroup).ToList();
                    foreach (var itemGroupRight in listGroup)
                    {
                        TbUserRight newUserRight = new TbUserRight();
                        newUserRight.UserId = item.UserId;
                        newUserRight.MenuId = itemGroupRight.MenuId;
                        newUserRight.Enabled = itemGroupRight.Enabled;
                        newUserRight.AddEnabled = itemGroupRight.AddEnabled;
                        newUserRight.UploadEnabled = itemGroupRight.UploadEnabled;
                        newUserRight.ModifyEnabled = itemGroupRight.ModifyEnabled;
                        newUserRight.DownloadEnabled = itemGroupRight.DownloadEnabled;
                        newUserRight.DeleteEnabled = itemGroupRight.DeleteEnabled;
                        newUserRight.ViewEnabled = itemGroupRight.ViewEnabled;

                        listUserInRight.Add(newUserRight);
                    }

                    using (var transaction = _accountService.GetTransaction())
                    {
                        try
                        {
                            //新增 TbUserInfo
                            await _accountService.Insert(item, transaction);
                            //新增 TbUserInfoExperience
                            await _accountService.InsertRange(listUserEXP, transaction);
                            //新增 TbUserInGroup
                            await _accountService.Insert(itemGroup, transaction);
                            //新增 TbUserRight
                            await _accountService.InsertRange(listUserInRight, transaction);

                            transaction.Commit();
                            isSuccess = true;
                            target_id = item.UserId;
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
                return RedirectToAction("MemberList");
            }
            else
            {
                if (item == null)
                {
                    return RedirectToAction("MemberList");
                }
                else
                {
                    /* 失敗回原頁 */
                    return View(datapost);
                }
            }
        }

        /// <summary>
        /// 編輯帳號
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [BackendCheckLogin("Menu000003", "MODIFY")]
        public async Task<IActionResult> MemberEdit(string id)
        {
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "帳號編輯", Action = "檢視";

            VM_Member data = new VM_Member();

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
                    // radioButton:專業領域
                    data.chbProfessionalField = _accountService.SetDDL_ProfessionalField(0);

                    IQueryable<MemberExtend>? dataList = _accountService.GetAccountExtendItem(ref _message, decrypt_id);
                    if (dataList != null)
                    {
                        data.MemberExtendItem = await dataList.SingleOrDefaultAsync();

                        // 編輯頁面用的密碼，需先解密呈現在前端，因為前端無法解密只好重後端處理
                        // 但如果直接對本來的值做改寫，會直接被存進DB
                        data.editAua8 = EncryptService.AES.Base64Decrypt(data.MemberExtendItem.userinfo.Aua8);

                        // 下拉清單:群組
                        data.Search.sGroup = data.MemberExtendItem.userinfoGroup.GroupId;

                        // 下拉群組
                        data.ddlGroup = _accountService.SetDDL_Group(1, data.MemberExtendItem.userinfoGroup.GroupId);

                        // 專業領域
                        if (data.MemberExtendItem.userinfoGroup.GroupId == _config.GetValue<string>("Site:ConsultantGroupID"))
                        {
                            // 取得使用者經歷
                            data.MemberExtendItem.listUserEXP = _accountService.Lookup<TbUserInfoExperience>(ref _message, x => x.UserId == data.MemberExtendItem.userinfo.UserId).ToList();

                            // 專業領域
                            if (!string.IsNullOrEmpty(data.MemberExtendItem.userinfo.Skill))
                            {
                                List<string> listSkill = new List<string>();
                                listSkill.AddRange(data.MemberExtendItem.userinfo.Skill.Split(','));
                                foreach (var itemSkill in data.chbProfessionalField.Where(x => listSkill.Contains(x.Value)))
                                {
                                    itemSkill.Selected = true;
                                }
                            }
                        }
                    }

                    if (data.MemberExtendItem == null)
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

                return RedirectToAction("MemberList");
            }
        }

        /// <summary>
        /// POST編輯帳號_存檔
        /// </summary>
        /// <returns></returns>
        [BackendCheckLogin("Menu000003", "MODIFY")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MemberEdit(string id, VM_Member datapost)
        {
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "帳號編輯", Action = "編輯";

            // 最終動作成功與否
            bool isSuccess = false;

            // 例外錯誤發生，特別記錄至 TbLog
            bool unCaughtError = false;

            DateTime dtnow = DateTime.Now;

            bool isChangeGroup = false;

            bool isChangPWD = false;

            TbUserInfo? item = null;
            TbUserInGroup? userGroup = null;
            List<TbUserInfoExperience>? listUserExp = new List<TbUserInfoExperience>();
            List<TbUserRight>? oldUserRight = new List<TbUserRight>();
            List<TbUserRight>? newUserRight = new List<TbUserRight>();

            string decrypt_id = EncryptService.AES.RandomizedDecrypt(id);

            try
            {
                if (string.IsNullOrEmpty(decrypt_id))
                {
                    TempData["TempMsgDetail"] = "金鑰逾時！請重新再操作一次！";
                }
                else
                {
                    IQueryable<TbUserInfo>? temp = _accountService.Lookup<TbUserInfo>(ref _message, x => x.UserId == decrypt_id && x.IsDelete == false);
                    if (temp != null)
                        item = await temp.SingleOrDefaultAsync();

                    if (item == null)
                    {
                        TempData["TempMsgDetail"] = "查無指定項目！";
                    }
                    else if (datapost.MemberExtendItem == null)
                    {
                        TempData["TempMsgDetail"] = "資料回傳有誤，請重新操作！";
                    }
                    else
                    {
                        // 判斷帳號是否重複
                        bool isRepeatAcc = _allCommonService.Lookup<TbUserInfo>(ref _message, x => x.Account == datapost.MemberExtendItem.userinfo.Account && x.Account != item.Account && !x.IsDelete).Any();
                        if (isRepeatAcc)
                        {
                            // 下拉群組
                            datapost.ddlGroup = _accountService.SetDDL_Group(1);
                            // radioButton:專業領域
                            datapost.chbProfessionalField = _accountService.SetDDL_ProfessionalField(0);

                            TempData["TempMsgType"] = MsgTypeEnum.error;
                            TempData["TempMsg"] = TempData["TempMsg"] ?? "帳號已重複";

                            /* 失敗回原頁 */
                            return View(datapost);
                        }

                        item.UserName = datapost.MemberExtendItem.userinfo.UserName;
                        item.Account = datapost.MemberExtendItem.userinfo.Account;
                        item.Email = datapost.MemberExtendItem.userinfo.Email;
                        item.Phone = datapost.MemberExtendItem.userinfo.Phone;
                        item.CellPhone = datapost.MemberExtendItem.userinfo.CellPhone;
                        if (datapost.editAua8 != EncryptService.AES.Base64Decrypt(item.Aua8))
                        {
                            item.Aua8 = EncryptService.AES.Base64Encrypt(datapost.editAua8);
                            isChangPWD = true;
                            // 判斷密碼是否可以修改
                            string ErrorMsg = string.Empty;
                            bool checkPwd = _accountService.UserPWDCheck(ref ErrorMsg, item.UserId, item.Aua8);

                            if (!checkPwd)
                            {
                                // 下拉群組
                                datapost.ddlGroup = _accountService.SetDDL_Group(1);
                                // radioButton:專業領域
                                datapost.chbProfessionalField = _accountService.SetDDL_ProfessionalField(0);

                                TempData["TempMsgType"] = MsgTypeEnum.error;
                                TempData["TempMsg"] = TempData["TempMsg"] ?? ErrorMsg;

                                /* 失敗回原頁 */
                                return View(datapost);
                            }
                        }
                        if (datapost.Search.sGroup == _config.GetValue<string>("Site:ConsultantGroupID"))
                        {
                            item.Sex = datapost.MemberExtendItem.userinfo.Sex;
                            item.IdNumber = datapost.MemberExtendItem.userinfo.IdNumber;
                            item.Industry = datapost.MemberExtendItem.userinfo.Industry;
                            item.ServiceUnit = datapost.MemberExtendItem.userinfo.ServiceUnit;
                            item.ContactAddr = datapost.MemberExtendItem.userinfo.ContactAddr;
                            item.PermanentAddr = datapost.MemberExtendItem.userinfo.PermanentAddr;
                            item.Education = datapost.MemberExtendItem.userinfo.Education;
                            item.Expertise = datapost.MemberExtendItem.userinfo.Expertise;

                            // 經歷
                            if (datapost.MemberExtendItem.listUserEXP != null && datapost.MemberExtendItem.listUserEXP.Any())
                            {
                                foreach (var itemUserEXP in datapost.MemberExtendItem.listUserEXP)
                                {
                                    TbUserInfoExperience tempUserEXP = new TbUserInfoExperience();
                                    tempUserEXP.UserId = item.UserId;
                                    tempUserEXP.ServDepartment = itemUserEXP.ServDepartment;
                                    tempUserEXP.JobTitle = itemUserEXP.JobTitle;
                                    tempUserEXP.Durinration = itemUserEXP.Durinration;
                                    listUserExp.Add(tempUserEXP);
                                }
                            }

                            // 專業領域
                            string strSkill = "";
                            foreach (var itemSkill in datapost.chbProfessionalField.Where(x => x.Selected))
                            {
                                strSkill += string.IsNullOrEmpty(strSkill) ? itemSkill.Value : string.Concat(",", itemSkill.Value);
                            }
                            item.Skill = strSkill;
                        }
                        item.IsActive = datapost.MemberExtendItem.userinfo.IsActive;
                        item.ModifyDate = dtnow;
                        item.ModifyUser = userinfo.UserID;

                        //TODO 判斷群組是否有改
                        IQueryable<TbUserInGroup>? tempUserInGroup = _accountService.Lookup<TbUserInGroup>(ref _message, x => x.UserId == item.UserId);
                        if (tempUserInGroup != null)
                            userGroup = await tempUserInGroup.SingleOrDefaultAsync();
                        if (datapost.Search.sGroup != userGroup.GroupId)
                        {
                            isChangeGroup = true;
                            userGroup.GroupId = datapost.Search.sGroup;
                            userGroup.CreateUser = userinfo.UserID;
                            userGroup.CreateDate = dtnow;

                            //取得原先的使用者權限，全部刪除
                            oldUserRight = _accountService.Lookup<TbUserRight>(ref _message, x => x.UserId == item.UserId).ToList();

                            // 取得新群組權限，增加使用者權限
                            List<TbGroupRight> listGroupRight = _accountService.Lookup<TbGroupRight>(ref _message, x => x.GroupId == datapost.Search.sGroup).ToList();
                            foreach (var itemNewRight in listGroupRight)
                            {
                                TbUserRight tempRight = new TbUserRight();
                                tempRight.UserId = item.UserId;
                                tempRight.MenuId = itemNewRight.MenuId;
                                tempRight.Enabled = itemNewRight.Enabled;
                                tempRight.AddEnabled = itemNewRight.AddEnabled;
                                tempRight.ModifyEnabled = itemNewRight.ModifyEnabled;
                                tempRight.DeleteEnabled = itemNewRight.DeleteEnabled;
                                tempRight.UploadEnabled = itemNewRight.UploadEnabled;
                                tempRight.DownloadEnabled = itemNewRight.DownloadEnabled;
                                tempRight.ViewEnabled = itemNewRight.ViewEnabled;
                                newUserRight.Add(tempRight);
                            }

                        }

                        using (var transaction = _accountService.GetTransaction())
                        {
                            try
                            {
                                //編輯
                                await _accountService.Update(item, transaction);
                                // 刪除經歷
                                List<TbUserInfoExperience> oriUserEXPList = new List<TbUserInfoExperience>();
                                oriUserEXPList = _accountService.Lookup<TbUserInfoExperience>(ref _message, x => x.UserId == item.UserId).ToList();
                                if (datapost.Search.sGroup == _config.GetValue<string>("Site:ConsultantGroupID"))
                                {
                                    // 先清除原先經歷
                                    if (oriUserEXPList != null && oriUserEXPList.Any())
                                        await _accountService.DeleteRange(oriUserEXPList, transaction);
                                    // 重新新增經歷
                                    if (listUserExp != null && listUserExp.Any())
                                        await _accountService.InsertRange(listUserExp, transaction);
                                }
                                else
                                {
                                    // 若非顧問群組清空
                                    if (oriUserEXPList != null && oriUserEXPList.Any())
                                        await _accountService.DeleteRange(oriUserEXPList, transaction);
                                }

                                if (isChangeGroup)
                                {
                                    // 更新使用者群組(因為GroupID是鍵值EF無法update，所以只能先delete再insert)
                                    await _accountService.Delete(tempUserInGroup.FirstOrDefault(), transaction);
                                    await _accountService.Insert(userGroup, transaction);
                                    // 更新使用者權限
                                    await _accountService.DeleteRange(oldUserRight, transaction);
                                    await _accountService.InsertRange(newUserRight, transaction);
                                }

                                if (isChangPWD)
                                {
                                    // 寫入密碼歷程
                                    TbPwdLog PwdLog = new TbPwdLog();
                                    PwdLog.UserId = item.UserId;
                                    PwdLog.Password = item.Aua8;
                                    PwdLog.CreateUser = userinfo.UserID;
                                    PwdLog.CreateDate = DateTime.Now;

                                    await _commonService.Insert(PwdLog, transaction);
                                }

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
                return RedirectToAction("MemberList");               
            }
            else
            {
                if (item == null)
                {
                    return RedirectToAction("MemberList");
                }
                else
                {
                    /* 失敗回原頁 */
                    return View(datapost);
                }
            }
        }

        /// <summary>
        /// 刪除帳號
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [BackendCheckLogin("Menu000003", "DELETE")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MemberDelete(string id)
        {
            JsonResponse<NtuFaq> result = new JsonResponse<NtuFaq>();
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "帳號管理", Action = "刪除";

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
                    TbUserInfo? item = null;
                    IQueryable<TbUserInfo>? temp = _accountService.Lookup<TbUserInfo>(ref _message, x => x.UserId == decrypt_id && x.IsDelete == false);
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

                        using (var transaction = _accountService.GetTransaction())
                        {
                            try
                            {
                                //編輯
                                await _accountService.Update(item, transaction);

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
        /// 匯出帳號列表
        /// </summary>
        /// <returns></returns>
        [BackendCheckLogin("Menu000003", "DOWNLOAD")]
        [ValidateAntiForgeryToken]
        [RequestFormLimits(ValueCountLimit = int.MaxValue)] // post form data 大小限制 
        [HttpPost]
        public async Task<IActionResult> MemberExport(VM_Member data)
        {
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "帳號管理", Action = "匯出";

            // 最終動作成功與否
            bool isSuccess = false;

            // 例外錯誤發生，特別記錄至 TbLog
            bool unCaughtError = false;

            ActionResultModel<MemoryStream> result = new ActionResultModel<MemoryStream>();
            string FileName = "帳號列表_" + DateTime.Today.ToString("yyyyMMdd");
            
            try
            {
                IQueryable<MemberExtend>? dataList = _accountService.GetAccountExtendList(ref _message, data.Search);
                
                if (dataList != null)
                    result = _exportService.AccountExcel(dataList);

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
                return RedirectToAction("MemberList");
            }
        }

        #endregion

        #region 個人資料管理
        /// <summary>
        /// 個人資料管理
        /// </summary>
        /// <returns></returns>
        [BackendCheckLogin("Menu000014", "MODIFY")]
        public async Task<IActionResult> PersonalManage()
        {
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "個人資料管理", Action = "檢視";

            VM_Member data = new VM_Member();

            // 最終動作成功與否
            bool isSuccess = false;

            // 例外錯誤發生，特別記錄至 TbLog
            bool unCaughtError = false;

            try
            {
                // 下拉群組
                data.ddlGroup = _accountService.SetDDL_Group(1);
                // radioButton:專業領域
                data.chbProfessionalField = _accountService.SetDDL_ProfessionalField(0);

                IQueryable<MemberExtend>? dataList = _accountService.GetAccountExtendItem(ref _message, userinfo.UserID);
                if (dataList != null)
                {
                    data.MemberExtendItem = await dataList.SingleOrDefaultAsync();

                    // 編輯頁面用的密碼，需先解密呈現在前端，因為前端無法解密只好重後端處理
                    // 但如果直接對本來的值做改寫，會直接被存進DB
                    data.editAua8 = EncryptService.AES.Base64Decrypt(data.MemberExtendItem.userinfo.Aua8);

                    // 下拉清單:群組
                    data.Search.sGroup = data.MemberExtendItem.userinfoGroup.GroupId;
                    // 專業領域
                    if (data.MemberExtendItem.userinfoGroup.GroupId == _config.GetValue<string>("Site:ConsultantGroupID"))
                    {
                        // 取得使用者經歷
                        data.MemberExtendItem.listUserEXP = _accountService.Lookup<TbUserInfoExperience>(ref _message, x => x.UserId == data.MemberExtendItem.userinfo.UserId).ToList();

                        // 專業領域
                        if (!string.IsNullOrEmpty(data.MemberExtendItem.userinfo.Skill))
                        {
                            List<string> listSkill = new List<string>();
                            listSkill.AddRange(data.MemberExtendItem.userinfo.Skill.Split(','));
                            foreach (var itemSkill in data.chbProfessionalField.Where(x => listSkill.Contains(x.Value)))
                            {
                                itemSkill.Selected = true;
                            }
                        }
                    }
                }

                if (data.MemberExtendItem == null)
                {
                    TempData["TempMsgDetail"] = "查無指定項目！";
                }
                else
                {
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
                await _commonService.OperateLog(userinfo.UserID, Feature, Action);
                return View(data);
            }
            else
            {
                TempData["TempMsgType"] = MsgTypeEnum.error;
                TempData["TempMsg"] = TempData["TempMsg"] ?? "操作失敗";

                //操作紀錄
                string response = (TempData["TempMsg"] == null ? "" : TempData["TempMsg"].ToString()) + "\r\n" + (TempData["TempMsgDetail"] == null ? "" : TempData["TempMsgDetail"].ToString());
                await _commonService.OperateLog(userinfo.UserID, Feature, Action, userinfo.UserID, userinfo.UserID, _message, response, isSuccess);

                if (unCaughtError)
                {
                    await _allCommonService.Error_Record("Backend", Feature + "-" + Action, _message);
                }

                return RedirectToAction("MemberList");
            }
        }

        /// <summary>
        /// POST個人資料管理_存檔
        /// </summary>
        /// <returns></returns>
        [BackendCheckLogin("Menu000014", "MODIFY")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PersonalManage(VM_Member datapost) 
        {
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "個人資料管理", Action = "編輯";

            // 最終動作成功與否
            bool isSuccess = false;
            bool isChangPWD = false;

            // 例外錯誤發生，特別記錄至 TbLog
            bool unCaughtError = false;

            DateTime dtnow = DateTime.Now;

            TbUserInfo? item = null;
            TbUserInGroup? userGroup = null;
            List<TbUserInfoExperience>? listUserExp = new List<TbUserInfoExperience>();

            try
            {
                IQueryable<TbUserInfo>? temp = _accountService.Lookup<TbUserInfo>(ref _message, x => x.UserId == userinfo.UserID && x.IsDelete == false);
                if (temp != null)
                    item = await temp.SingleOrDefaultAsync();

                if (item == null)
                {
                    TempData["TempMsgDetail"] = "查無指定項目！";
                }
                else if (datapost.MemberExtendItem == null)
                {
                    TempData["TempMsgDetail"] = "資料回傳有誤，請重新操作！";
                }
                else
                {
                    item.UserName = datapost.MemberExtendItem.userinfo.UserName;
                    item.Email = datapost.MemberExtendItem.userinfo.Email;
                    item.Phone = datapost.MemberExtendItem.userinfo.Phone;
                    item.CellPhone = datapost.MemberExtendItem.userinfo.CellPhone;
                    if (datapost.editAua8 != EncryptService.AES.Base64Decrypt(item.Aua8))
                    {
                        item.Aua8 = EncryptService.AES.Base64Encrypt(datapost.editAua8);
                        isChangPWD = true;
                        // 判斷密碼是否可以修改
                        string ErrorMsg = string.Empty;
                        bool checkPwd = _accountService.UserPWDCheck(ref ErrorMsg, item.UserId, item.Aua8);

                        if (!checkPwd)
                        {
                            // 下拉群組
                            datapost.ddlGroup = _accountService.SetDDL_Group(1);
                            // radioButton:專業領域
                            datapost.chbProfessionalField = _accountService.SetDDL_ProfessionalField(0);

                            TempData["TempMsgType"] = MsgTypeEnum.error;
                            TempData["TempMsg"] = TempData["TempMsg"] ?? ErrorMsg;

                            /* 失敗回原頁 */
                            return View(datapost);
                        }
                    }
                        
                    if (datapost.Search.sGroup == _config.GetValue<string>("Site:ConsultantGroupID"))
                    {
                        item.Sex = datapost.MemberExtendItem.userinfo.Sex;
                        item.IdNumber = datapost.MemberExtendItem.userinfo.IdNumber;
                        item.Industry = datapost.MemberExtendItem.userinfo.Industry;
                        item.ServiceUnit = datapost.MemberExtendItem.userinfo.ServiceUnit;
                        item.ContactAddr = datapost.MemberExtendItem.userinfo.ContactAddr;
                        item.PermanentAddr = datapost.MemberExtendItem.userinfo.PermanentAddr;
                        item.Education = datapost.MemberExtendItem.userinfo.Education;
                        item.Expertise = datapost.MemberExtendItem.userinfo.Expertise;

                        // 經歷
                        if (datapost.MemberExtendItem.listUserEXP != null && datapost.MemberExtendItem.listUserEXP.Any())
                        {
                            foreach (var itemUserEXP in datapost.MemberExtendItem.listUserEXP)
                            {
                                TbUserInfoExperience tempUserEXP = new TbUserInfoExperience();
                                tempUserEXP.UserId = item.UserId;
                                tempUserEXP.ServDepartment = itemUserEXP.ServDepartment;
                                tempUserEXP.JobTitle = itemUserEXP.JobTitle;
                                tempUserEXP.Durinration = itemUserEXP.Durinration;
                                listUserExp.Add(tempUserEXP);
                            }
                        }

                        // 專業領域
                        string strSkill = "";
                        foreach (var itemSkill in datapost.chbProfessionalField.Where(x => x.Selected))
                        {
                            strSkill += string.IsNullOrEmpty(strSkill) ? itemSkill.Value : string.Concat(",", itemSkill.Value);
                        }
                        item.Skill = strSkill;
                    }
                    item.IsActive = datapost.MemberExtendItem.userinfo.IsActive;
                    item.ModifyDate = dtnow;
                    item.ModifyUser = userinfo.UserID;

                    using (var transaction = _accountService.GetTransaction())
                    {
                        try
                        {
                            //編輯
                            await _accountService.Update(item, transaction);
                            // 刪除經歷
                            List<TbUserInfoExperience> oriUserEXPList = new List<TbUserInfoExperience>();
                            oriUserEXPList = _accountService.Lookup<TbUserInfoExperience>(ref _message, x => x.UserId == item.UserId).ToList();
                            if (datapost.Search.sGroup == _config.GetValue<string>("Site:ConsultantGroupID"))
                            {
                                // 先清除原先經歷
                                if (oriUserEXPList != null && oriUserEXPList.Any())
                                    await _accountService.DeleteRange(oriUserEXPList, transaction);
                                // 重新新增經歷
                                if (listUserExp != null && listUserExp.Any())
                                    await _accountService.InsertRange(listUserExp, transaction);
                            }
                            else
                            {
                                // 若非顧問群組清空
                                if (oriUserEXPList != null && oriUserEXPList.Any())
                                    await _accountService.DeleteRange(oriUserEXPList, transaction);
                            }

                            if (isChangPWD)
                            {
                                // 寫入密碼歷程
                                TbPwdLog PwdLog = new TbPwdLog();
                                PwdLog.UserId = item.UserId;
                                PwdLog.Password = item.Aua8;
                                PwdLog.CreateUser = userinfo.UserID;
                                PwdLog.CreateDate = DateTime.Now;

                                await _commonService.Insert(PwdLog, transaction);
                            }

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
            await _commonService.OperateLog(userinfo.UserID, Feature, Action, userinfo.UserID, datapost, _message, response, isSuccess);

            if (isSuccess)
            {
                return RedirectToAction("PersonalManage", "Member");
            }
            else
            {
                if (item == null)
                {
                    return RedirectToAction("PersonalManage", "Member");
                }
                else
                {
                    /* 失敗回原頁 */
                    return View(datapost);
                }
            }
        }

        #endregion

        #region 使用者操作歷程記錄
        /// <summary>
        /// 使用者操作歷程記錄
        /// </summary>
        /// <returns></returns>
        [BackendCheckLogin("Menu000007", "ENABLED")]
        public async Task<IActionResult> UserOperateRecord(VM_Member data)
        {
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "使用者操作歷程記錄", Action = "檢視";

            try
            {
                // 下拉操作帳號
                data.ddlMember = _accountService.SetDDL_Account(1);

                //取資料
                IQueryable<OperateExtend>? dataList = _accountService.GetOperateExtendList(ref _message, data.Search);

                //分頁
                if (dataList != null)
                    data.OperateExtendList = await PagerInfoService.GetRange(dataList.OrderByDescending(x=>x.operateLog.CreateDate), data.Search.PagerInfo);

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
        #endregion

        #region 匯入顧問

        /// <summary>
        /// 匯入顧問資料
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        [BackendCheckLogin("Menu000003", "DOWNLOAD")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConsultantImport(IFormFile file)
        {
            JsonResponse<string> result = new JsonResponse<string>();
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "帳號管理", Action = "資料匯入";

            DateTime dtnow = DateTime.Now;

            // 最終動作成功與否
            bool isSuccess = false;

            // 例外錯誤發生，特別記錄至 TbLog
            bool unCaughtError = false;

            try
            {
                bool hasError = false;

                /* 欄位數 */
                int column_num = 16;

                // 取得 附檔名
                string extension = Path.GetExtension(file.FileName);
                // 取得 Workbook 實例
                var wb = _importService.ReadExcel(extension, file);

                if (wb.IsSuccess == false || wb.Data == null)
                {
                    result.MessageDetail = "檔案讀取失敗";
                    _message += wb.Message;
                    hasError = true;
                }
                else
                {
                    List<TbUserInfo> insert_UserInfo = new List<TbUserInfo>();
                    List<TbUserInGroup> insert_UserInGroup = new List<TbUserInGroup>();
                    List<TbUserRight> insert_UserRight = new List<TbUserRight>();
                    // 若帳號已存在則須更新
                    List<TbUserInfo> update_UserInfo = new List<TbUserInfo>();

                    for (int i = 0; i < wb.Data.NumberOfSheets; i++)
                    {
                        // 取得 sheet
                        var sheet = wb.Data.GetSheetAt(i);

                        // sheet 名稱
                        string sheetName = sheet.SheetName;

                        VM_MemberQueryParam vmMemberQueryParam = new VM_MemberQueryParam();
                        vmMemberQueryParam.sGroup = "G000000004";
                        // 取得目前顧問群組下的所有帳號
                        IQueryable<MemberExtend>? dataList = _accountService.GetAccountExtendList(ref _message, vmMemberQueryParam);
                        List<MemberExtend> existList = dataList.ToList();

                        // 取得群組權限
                        List<TbGroupRight> listGroup = new List<TbGroupRight>();
                        listGroup = _accountService.Lookup<TbGroupRight>(ref _message, x => x.GroupId == "G000000004").ToList();

                        // 取得專業領域選項
                        List<TbBasicColumn> skillColumns = _accountService.Lookup<TbBasicColumn>(ref _message, x => x.BacolCode == "professionalField").ToList();

                        for (int row = 1; row <= sheet.PhysicalNumberOfRows; row++)
                        {
                            // 驗證不是空白列
                            IRow sheetRow = sheet.GetRow(row);

                            /* row 的欄位數對才處理 */
                            if (sheetRow != null)
                            {
                                if (sheetRow.PhysicalNumberOfCells == column_num)
                                {
                                    string member = sheetRow.GetCell(0).ToString();
                                    string aua8 = sheetRow.GetCell(1).ToString();
                                    string name = sheetRow.GetCell(2).ToString();
                                    string phone = sheetRow.GetCell(3).ToString();
                                    string email = sheetRow.GetCell(4).ToString();
                                    string cellphone = sheetRow.GetCell(5).ToString();
                                    string sex = sheetRow.GetCell(6).ToString();
                                    string IDNumber = sheetRow.GetCell(7).ToString();
                                    string Industry = sheetRow.GetCell(8).ToString();
                                    string ServiceUnit = sheetRow.GetCell(9).ToString();
                                    string ContactAddr = sheetRow.GetCell(10).ToString();
                                    string PermanentAddr = sheetRow.GetCell(11).ToString();
                                    string Education = sheetRow.GetCell(12).ToString();
                                    string Expertise = sheetRow.GetCell(13).ToString();
                                    string JobTitle = sheetRow.GetCell(14).ToString();
                                    string Skill = sheetRow.GetCell(15).ToString();

                                    /* 檢查必填項 */
                                    if (StringExtensions.CheckIsNullOrEmpty(member, name, aua8, email, phone, cellphone, sex, Skill))
                                    {
                                        result.MessageDetail += $"第 {i + 1} 活頁簿，第 {row + 1} 列帳號、信箱、姓名、電話、手機、密碼、性別與專業領域須必填，已略過\n";
                                        hasError = true;
                                    }
                                    else
                                    {
                                        string skillCode = "";
                                        // 處理專業領域文字轉代碼
                                        if (!string.IsNullOrEmpty(Skill))
                                        {
                                            List<string> listSkillText = new List<string>();
                                            listSkillText = Skill.Split(',').ToList();
                                            List<string> listSkillCode = new List<string>();
                                            listSkillCode = skillColumns.Where(x => listSkillText.Contains(x.Title)).Select(x => x.BacolId).ToList();

                                            foreach (var itemSkill in listSkillCode)
                                            {
                                                skillCode = string.IsNullOrEmpty(skillCode) ? skillCode + itemSkill : skillCode + "," + itemSkill;
                                            }
                                        }

                                        MemberExtend existItem = existList.Where(x => x.userinfo.Account == member).FirstOrDefault();
                                        if (existItem != null && existItem.userinfo != null)
                                        {// 需要更新的顧問資料
                                            existItem.userinfo.Aua8 = EncryptService.AES.Base64Encrypt(aua8);
                                            existItem.userinfo.UserName = name;
                                            existItem.userinfo.Phone = phone;
                                            existItem.userinfo.Email = email;
                                            existItem.userinfo.CellPhone = cellphone;
                                            existItem.userinfo.Sex = sex == "男" ? "M" : "F";
                                            existItem.userinfo.IdNumber = IDNumber;
                                            existItem.userinfo.Industry = Industry;
                                            existItem.userinfo.ServiceUnit = ServiceUnit;
                                            existItem.userinfo.ContactAddr = ContactAddr;
                                            existItem.userinfo.PermanentAddr = PermanentAddr;
                                            existItem.userinfo.Education = Education;
                                            existItem.userinfo.Expertise = Expertise;
                                            existItem.userinfo.JobTitle = JobTitle;
                                            existItem.userinfo.Skill = skillCode;
                                            existItem.userinfo.ModifyDate = dtnow;
                                            existItem.userinfo.ModifyUser = userinfo.UserID;

                                            update_UserInfo.Add(existItem.userinfo);
                                        }
                                        else {
                                            TbUserInfo newUser = new TbUserInfo();
                                            newUser.UserId = await _allCommonService.IDGenerator<TbUserInfo>();
                                            newUser.Account = member;
                                            newUser.Aua8 = EncryptService.AES.Base64Encrypt(aua8);
                                            newUser.UserName = name;
                                            newUser.Phone = phone;
                                            newUser.Email = email;
                                            newUser.IsActive = true;
                                            newUser.IsDelete = false;
                                            newUser.CellPhone = cellphone;
                                            newUser.Sex = sex == "男" ? "M" : "F";
                                            newUser.JobTitle = JobTitle;
                                            newUser.IdNumber = IDNumber;
                                            newUser.Industry = Industry;
                                            newUser.ServiceUnit = ServiceUnit;
                                            newUser.ContactAddr = ContactAddr;
                                            newUser.PermanentAddr = PermanentAddr;
                                            newUser.Education = Education;
                                            newUser.Expertise = Expertise;
                                            newUser.Skill = skillCode;
                                            newUser.CreateDate = dtnow;
                                            newUser.CreateUser = userinfo.UserID;
                                            insert_UserInfo.Add(newUser);


                                            // 新增對應的USER群組
                                            TbUserInGroup newUserInGroup = new TbUserInGroup();
                                            newUserInGroup.UserId = newUser.UserId;
                                            newUserInGroup.GroupId = "G000000004";
                                            insert_UserInGroup.Add(newUserInGroup);

                                            // 新增對應的USER 權限
                                            foreach (var itemGroupRight in listGroup)
                                            {
                                                TbUserRight newUserRight = new TbUserRight();
                                                newUserRight.UserId = newUser.UserId;
                                                newUserRight.MenuId = itemGroupRight.MenuId;
                                                newUserRight.Enabled = itemGroupRight.Enabled;
                                                newUserRight.AddEnabled = itemGroupRight.AddEnabled;
                                                newUserRight.UploadEnabled = itemGroupRight.UploadEnabled;
                                                newUserRight.ModifyEnabled = itemGroupRight.ModifyEnabled;
                                                newUserRight.DownloadEnabled = itemGroupRight.DownloadEnabled;
                                                newUserRight.DeleteEnabled = itemGroupRight.DeleteEnabled;
                                                newUserRight.ViewEnabled = itemGroupRight.ViewEnabled;

                                                insert_UserRight.Add(newUserRight);
                                            }

                                        }
                                    }
                                }
                                else
                                {
                                    result.MessageDetail += $"第 {i + 1} 活頁簿，第 {row + 1} 列欄位數目不正確，已略過\n";
                                    hasError = true;
                                }
                            }
                        }
                    }

                    if (!hasError)
                    {
                        using (var transaction = _accountService.GetTransaction())
                        {
                            try
                            {
                                /* 更新此次處理結果 */
                                await _accountService.UpdateRange(update_UserInfo, transaction);

                                /* 新增此次處理結果 */
                                await _accountService.InsertRange(insert_UserInfo, transaction);
                                await _accountService.InsertRange(insert_UserInGroup, transaction);
                                await _accountService.InsertRange(insert_UserRight, transaction);
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
                result.Message = "匯入成功";
            }
            else
            {
                result.alert_type = "error";
                result.Message = result.Message ?? "匯入失敗";

                if (unCaughtError)
                {
                    await _allCommonService.Error_Record("Backend", Feature + "-" + Action, _message);
                }
            }

            string response = result.Message + "\r\n" + result.MessageDetail;

            await _commonService.OperateLog(userinfo.UserID, Feature, Action, null, null, _message, response, isSuccess);

            return Json(result);
        }

        #endregion
    }
}
