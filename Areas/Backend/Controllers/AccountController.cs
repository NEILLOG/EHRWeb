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

namespace BASE.Areas.Backend.Controllers
{
    public class AccountController : BaseController
    {
        private readonly AccountService _accountService;
        private readonly AllCommonService _allCommonService;
        private readonly CommonService _commonService;
        private readonly IConfiguration _config;

        public AccountController(AccountService accountService,
            AllCommonService allCommonService,
            CommonService commonService,
            IConfiguration configuration)
        {
            _allCommonService = allCommonService;
            _commonService = commonService;
            _accountService = accountService;
            _config = configuration;
        }

        #region 帳號管理

        /// <summary>
        /// 帳號列表
        /// </summary>
        /// <returns></returns>
        [BackendCheckLogin("Menu000003", "ENABLED")]
        public async Task<IActionResult> AccountList(VM_Account data) 
        {
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "帳號管理", Action = "檢視";

            try {
                // 下拉群組
                data.ddlGroup = _accountService.SetDDL_Group(2);

                //取資料
                IQueryable<AccountExtend>? dataList = _accountService.GetAccountExtendList(ref _message, data.Search);

                //分頁
                if (dataList != null)
                    data.AccountExtendList = await PagerInfoService.GetRange(dataList, data.Search.PagerInfo);

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
        public async Task<IActionResult> AccountAdd()
        {
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "新增帳號", Action = "檢視";

            VM_Account data = new VM_Account();

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
        public async Task<IActionResult> AccountAdd(VM_Account datapost)
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
                if (datapost.AccountExtendItem != null)
                {
                    // 判斷帳號是否重複
                    bool isRepeatAcc = _allCommonService.Lookup<TbUserInfo>(ref _message, x => x.Account == datapost.AccountExtendItem.account.Account && !x.IsDelete).Any();
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
                    item.Account = datapost.AccountExtendItem.account.Account;
                    item.Aua8 =  EncryptService.AES.Base64Encrypt(datapost.AccountExtendItem.account.Aua8);
                    item.UserName = datapost.AccountExtendItem.account.UserName;
                    item.Phone = datapost.AccountExtendItem.account.Phone;
                    item.Email = datapost.AccountExtendItem.account.Email;
                    item.IsActive = datapost.AccountExtendItem.account.IsActive;
                    item.IsActive = datapost.AccountExtendItem.account.IsActive;
                    item.IsDelete = false;
                    item.CellPhone = datapost.AccountExtendItem.account.CellPhone;
                    if (datapost.Search.sGroup == _config.GetValue<string>("Site:ConsultantGroupID"))
                    {
                        // 顧問選項
                        item.Sex = datapost.AccountExtendItem.account.Sex;
                        item.JobTitle = datapost.AccountExtendItem.account.JobTitle;
                        item.IdNumber = datapost.AccountExtendItem.account.IdNumber;
                        item.Industry = datapost.AccountExtendItem.account.Industry;
                        item.ServiceUnit = datapost.AccountExtendItem.account.ServiceUnit;
                        item.ContactAddr = datapost.AccountExtendItem.account.ContactAddr;
                        item.PermanentAddr = datapost.AccountExtendItem.account.PermanentAddr;
                        item.Education = datapost.AccountExtendItem.account.Education;
                        item.Expertise = datapost.AccountExtendItem.account.Expertise;
                        string strSkill = "";
                        foreach (var itemSkill in datapost.chbProfessionalField.Where(x => x.Selected))
                        {
                            strSkill += string.IsNullOrEmpty(strSkill) ? itemSkill.Value : string.Concat(",",itemSkill.Value);
                        }
                        item.Skill = strSkill;

                        if (datapost.AccountExtendItem.listUserEXP != null && datapost.AccountExtendItem.listUserEXP.Any())
                        {
                            foreach (var itemUserEXP in datapost.AccountExtendItem.listUserEXP)
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
                return RedirectToAction("AccountList");
            }
            else
            {
                if (item == null)
                {
                    return RedirectToAction("AccountList");
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
        public async Task<IActionResult> AccountEdit(string id)
        {
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "帳號編輯", Action = "檢視";

            VM_Account data = new VM_Account();

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

                    IQueryable<AccountExtend>? dataList = _accountService.GetAccountExtendItem(ref _message, decrypt_id);
                    if (dataList != null)
                    {
                        data.AccountExtendItem = await dataList.SingleOrDefaultAsync();

                        // 編輯頁面用的密碼，需先解密呈現在前端，因為前端無法解密只好重後端處理
                        // 但如果直接對本來的值做改寫，會直接被存進DB
                        data.editAua8 = EncryptService.AES.Base64Decrypt(data.AccountExtendItem.account.Aua8);

                        // 下拉清單:群組
                        data.Search.sGroup = data.AccountExtendItem.accountGroup.GroupId;

                        // 下拉群組
                        data.ddlGroup = _accountService.SetDDL_Group(1, data.AccountExtendItem.accountGroup.GroupId);

                        // 專業領域
                        if (data.AccountExtendItem.accountGroup.GroupId == _config.GetValue<string>("Site:ConsultantGroupID"))
                        {
                            // 取得使用者經歷
                            data.AccountExtendItem.listUserEXP = _accountService.Lookup<TbUserInfoExperience>(ref _message, x => x.UserId == data.AccountExtendItem.account.UserId).ToList();

                            // 專業領域
                            if (!string.IsNullOrEmpty(data.AccountExtendItem.account.Skill))
                            {
                                List<string> listSkill = new List<string>();
                                listSkill.AddRange(data.AccountExtendItem.account.Skill.Split(','));
                                foreach (var itemSkill in data.chbProfessionalField.Where(x => listSkill.Contains(x.Value)))
                                {
                                    itemSkill.Selected = true;
                                }
                            }
                        }
                    }

                    if (data.AccountExtendItem == null)
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

                return RedirectToAction("AccountList");
            }
        }

        /// <summary>
        /// POST編輯帳號_存檔
        /// </summary>
        /// <returns></returns>
        [BackendCheckLogin("Menu000003", "MODIFY")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AccountEdit(string id, VM_Account datapost)
        {
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "帳號編輯", Action = "編輯";

            // 最終動作成功與否
            bool isSuccess = false;

            // 例外錯誤發生，特別記錄至 TbLog
            bool unCaughtError = false;

            DateTime dtnow = DateTime.Now;

            bool isChangeGroup = false;

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
                    else if (datapost.AccountExtendItem == null)
                    {
                        TempData["TempMsgDetail"] = "資料回傳有誤，請重新操作！";
                    }
                    else
                    {
                        // 判斷帳號是否重複
                        bool isRepeatAcc = _allCommonService.Lookup<TbUserInfo>(ref _message, x => x.Account == datapost.AccountExtendItem.account.Account && x.Account != item.Account && !x.IsDelete).Any();
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

                        item.UserName = datapost.AccountExtendItem.account.UserName;
                        item.Account = datapost.AccountExtendItem.account.Account;
                        item.Email = datapost.AccountExtendItem.account.Email;
                        item.Phone = datapost.AccountExtendItem.account.Phone;
                        item.CellPhone = datapost.AccountExtendItem.account.CellPhone;
                        if(datapost.editAua8 != EncryptService.AES.Base64Decrypt(item.Aua8))
                            item.Aua8 = EncryptService.AES.Base64Encrypt(datapost.editAua8);
                        if (datapost.Search.sGroup == _config.GetValue<string>("Site:ConsultantGroupID"))
                        {
                            item.Sex = datapost.AccountExtendItem.account.Sex;
                            item.IdNumber = datapost.AccountExtendItem.account.IdNumber;
                            item.Industry = datapost.AccountExtendItem.account.Industry;
                            item.ServiceUnit = datapost.AccountExtendItem.account.ServiceUnit;
                            item.ContactAddr = datapost.AccountExtendItem.account.ContactAddr;
                            item.PermanentAddr = datapost.AccountExtendItem.account.PermanentAddr;
                            item.Education = datapost.AccountExtendItem.account.Education;
                            item.Expertise = datapost.AccountExtendItem.account.Expertise;

                            // 經歷
                            if (datapost.AccountExtendItem.listUserEXP != null && datapost.AccountExtendItem.listUserEXP.Any())
                            {
                                foreach (var itemUserEXP in datapost.AccountExtendItem.listUserEXP)
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
                        item.IsActive = datapost.AccountExtendItem.account.IsActive;
                        item.ModifyDate = dtnow;
                        item.ModifyUser = userinfo.UserID;

                        //TODO 判斷群組是否有改
                        IQueryable<TbUserInGroup>? tempUserInGroup = _accountService.Lookup<TbUserInGroup>(ref _message, x => x.UserId == item.UserId);
                        if (tempUserInGroup != null)
                            userGroup = await tempUserInGroup.SingleOrDefaultAsync();
                        //userGroup = _accountService.Lookup<TbUserInGroup>(ref _message, x => x.UserId == item.UserId).FirstOrDefault();
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
                return RedirectToAction("AccountList");
            }
            else
            {
                if (item == null)
                {
                    return RedirectToAction("AccountList");
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
        public async Task<IActionResult> AccountDelete(string id)
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

            VM_Account data = new VM_Account();

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

                IQueryable<AccountExtend>? dataList = _accountService.GetAccountExtendItem(ref _message, userinfo.UserID);
                if (dataList != null)
                {
                    data.AccountExtendItem = await dataList.SingleOrDefaultAsync();

                    // 編輯頁面用的密碼，需先解密呈現在前端，因為前端無法解密只好重後端處理
                    // 但如果直接對本來的值做改寫，會直接被存進DB
                    data.editAua8 = EncryptService.AES.Base64Decrypt(data.AccountExtendItem.account.Aua8);

                    // 下拉清單:群組
                    data.Search.sGroup = data.AccountExtendItem.accountGroup.GroupId;
                    // 專業領域
                    if (data.AccountExtendItem.accountGroup.GroupId == _config.GetValue<string>("Site:ConsultantGroupID"))
                    {
                        // 取得使用者經歷
                        data.AccountExtendItem.listUserEXP = _accountService.Lookup<TbUserInfoExperience>(ref _message, x => x.UserId == data.AccountExtendItem.account.UserId).ToList();

                        // 專業領域
                        if (!string.IsNullOrEmpty(data.AccountExtendItem.account.Skill))
                        {
                            List<string> listSkill = new List<string>();
                            listSkill.AddRange(data.AccountExtendItem.account.Skill.Split(','));
                            foreach (var itemSkill in data.chbProfessionalField.Where(x => listSkill.Contains(x.Value)))
                            {
                                itemSkill.Selected = true;
                            }
                        }
                    }
                }

                if (data.AccountExtendItem == null)
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

                return RedirectToAction("AccountList");
            }
        }

        /// <summary>
        /// POST個人資料管理_存檔
        /// </summary>
        /// <returns></returns>
        [BackendCheckLogin("Menu000014", "MODIFY")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PersonalManage(VM_Account datapost) 
        {
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "個人資料管理", Action = "編輯";

            // 最終動作成功與否
            bool isSuccess = false;

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
                else if (datapost.AccountExtendItem == null)
                {
                    TempData["TempMsgDetail"] = "資料回傳有誤，請重新操作！";
                }
                else
                {
                    item.UserName = datapost.AccountExtendItem.account.UserName;
                    item.Email = datapost.AccountExtendItem.account.Email;
                    item.Phone = datapost.AccountExtendItem.account.Phone;
                    item.CellPhone = datapost.AccountExtendItem.account.CellPhone;
                    if (datapost.editAua8 != EncryptService.AES.Base64Decrypt(item.Aua8))
                        item.Aua8 = EncryptService.AES.Base64Encrypt(datapost.editAua8);
                    if (datapost.Search.sGroup == _config.GetValue<string>("Site:ConsultantGroupID"))
                    {
                        item.Sex = datapost.AccountExtendItem.account.Sex;
                        item.IdNumber = datapost.AccountExtendItem.account.IdNumber;
                        item.Industry = datapost.AccountExtendItem.account.Industry;
                        item.ServiceUnit = datapost.AccountExtendItem.account.ServiceUnit;
                        item.ContactAddr = datapost.AccountExtendItem.account.ContactAddr;
                        item.PermanentAddr = datapost.AccountExtendItem.account.PermanentAddr;
                        item.Education = datapost.AccountExtendItem.account.Education;
                        item.Expertise = datapost.AccountExtendItem.account.Expertise;

                        // 經歷
                        if (datapost.AccountExtendItem.listUserEXP != null && datapost.AccountExtendItem.listUserEXP.Any())
                        {
                            foreach (var itemUserEXP in datapost.AccountExtendItem.listUserEXP)
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
                    item.IsActive = datapost.AccountExtendItem.account.IsActive;
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
                return RedirectToAction("PersonalManage", "Account");
            }
            else
            {
                if (item == null)
                {
                    return RedirectToAction("PersonalManage", "Account");
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
        public async Task<IActionResult> UserOperateRecord(VM_Account data)
        {
            UserSessionModel? userinfo = HttpContext.Session.Get<UserSessionModel>(SessionStruct.Login.UserInfo);
            string Feature = "使用者操作歷程記錄", Action = "檢視";

            try
            {
                // 下拉操作帳號
                data.ddlAccount = _accountService.SetDDL_Account(1);

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
    }
}
