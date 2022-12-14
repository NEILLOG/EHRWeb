using BASE.Areas.Frontend.Models;
using BASE.Service;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Serialization;
using BASE.Extensions;
using BASE.Filters;
using BASE.Models.Enums;
using BASE.Models.DB;
using Microsoft.EntityFrameworkCore;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using System.Reflection;
using BASE.Areas.Frontend.Service;
using BASE.Models;
using BASE.Areas.Frontend.Models.Extend;
using BASE.Areas.Backend.Service;
using ConsultService = BASE.Areas.Frontend.Service.ConsultService;
using Microsoft.AspNetCore.Http;
using NPOI.SS.Formula.Functions;

namespace BASE.Areas.Frontend.Controllers
{
    public class ConsultController : BaseController
    {
        private readonly IConfiguration _conf;
        private readonly AllCommonService _allCommonService;
        private readonly OnePageService _onepageService;
        private readonly ConsultService _consultService;
        private readonly FileService _fileService;
        private readonly MailService _mailService;

        public ConsultController(IConfiguration configuration,
            AllCommonService allCommonService,
            OnePageService onepageService,
            FileService fileService,
            ConsultService consultService,
            MailService mailService)
        {
            _conf = configuration;
            _allCommonService = allCommonService;
            _onepageService = onepageService;
            _consultService = consultService;
            _fileService = fileService;
            _mailService = mailService;
        }

        public async Task<IActionResult> Index()
        {
            VM_OnePage data = new VM_OnePage();

            try
            {
                data.ExtendItem = _onepageService.GetExtendItem(ref _message, "OP000003"); //TODO: 待補編號
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Home", new { area = "Frontend" });
            }

            return View(data);
        }


        public async Task<IActionResult> Register()
        {
            HttpContext.Session.Set(Backend.Models.SessionStruct.VerifyCode.Consult, new ValidImageHelper().RandomCode(5));

            VM_ConsultRegister data = new VM_ConsultRegister();
         
            try
            {
                data.ckbSubjects = _allCommonService.Lookup<TbBasicColumn>(ref _message, x => x.BacolCode == "professionalField" && x.IsActive == true)
                                    .Select(x => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem() { Text = x.Title, Value = x.BacolId })
                                    .ToList();
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Home", new { area = "Frontend" });
            }

            return View(data);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(VM_ConsultRegister datapost)
        {
            string Feature = "諮詢輔導服務", Action = "新增";

            
            bool isSuccess = false; // 最終動作成功與否
            bool unCaughtError = false; // 例外錯誤發生，特別記錄至 TbLog

            String code = HttpContext.Session.Get<String>(Backend.Models.SessionStruct.VerifyCode.Consult);

            if (code == datapost.VerifyCode)
            {
                try
                {
                    datapost.ExtendItem.CreateDate = DateTime.Now;
                    datapost.ExtendItem.ConsultSubjects = String.Join(",", datapost.CheckedSubjects);
                    try
                    {
                        await _consultService.Insert(datapost.ExtendItem);
                        isSuccess = true;
                    }
                    catch (Exception ex)
                    {
                        _message += ex.ToString();
                        TempData["TempMsgDetail"] = "發生技術性錯誤，請聯絡技術人員或稍後再試一次";
                        unCaughtError = true;
                    }
                }
                catch (Exception ex)
                {
                    TempData["TempMsg"] = "伺服器連線異常，請檢查您的網路狀態後再試一次！";
                    _message += ex.ToString();
                    unCaughtError = true;
                }
            }
            else
            {
                TempData["TempMsg"] = "驗證碼錯誤";
            }


            if (isSuccess)
            {
                TempData["CostomTempEmail"] = datapost.ExtendItem.ContactEmail;

                await _mailService.SendEmail(new MailViewModel()
                {
                    Subject = MailTmeplate.Consult.REQUIRED_SURVEY_SUBJECT,
                    Body = String.Format(MailTmeplate.Consult.REQUIRED_SURVEY_CONTNET,
                                        datapost.ExtendItem.Name,
                                        datapost.ExtendItem.ContactName,
                                        Url.Action("RequireSurveyUpload", "Consult", new { id = EncryptService.AES.RandomizedEncrypt(datapost.ExtendItem.Id.ToString()) }, Request.Scheme)
                                        ),
                    ToList = new List<MailAddressInfo>() { new MailAddressInfo(datapost.ExtendItem.ContactEmail) }
                });
            }
            else
            {
                TempData["TempMsgType"] = MsgTypeEnum.error;
                TempData["TempMsg"] = TempData["TempMsg"] ?? "儲存失敗";

                if (unCaughtError)
                {
                    await _allCommonService.Error_Record("Frontend", Feature + "-" + Action, _message);
                }
            }

            if (isSuccess)
            {
                return RedirectToAction("Index");
            }
            else
            {
                if (datapost.ExtendItem == null)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    datapost.ckbSubjects = _allCommonService.Lookup<TbBasicColumn>(ref _message, x => x.BacolCode == "professionalField" && x.IsActive == true)
                                            .Select(x => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem() { Text = x.Title, Value = x.BacolId })
                                            .ToList();

                    return View(datapost);
                }
            }

        }


        /// <summary>使用者上傳諮詢服務滿意度問卷</summary>
        /// <param name="id">諮詢報名編號(已加密)</param>
        public async Task<IActionResult> SatisfySurveyUpload(string id)
        {
            string test = EncryptService.AES.RandomizedEncrypt("1");

            VM_OtherUpload data = new VM_OtherUpload();

            string decrypt_id = EncryptService.AES.RandomizedDecrypt(id);
            Int64 _decrypt_id = 0; Int64.TryParse(decrypt_id, out _decrypt_id);

            TbConsultRegister? main = _fileService.Lookup<TbConsultRegister>(ref _message, x => x.Id == _decrypt_id).FirstOrDefault();

            try
            {
                //檢查ID是否存在
                if (main == null)
                    throw new Exception("查無此筆諮詢報名資料");
            }
            catch (Exception ex)
            {
                TempData["TempMsgType"] = MsgTypeEnum.error;
                TempData["TempMsg"] = TempData["TempMsg"] ?? "儲存失敗";

                return RedirectToAction("Index", "Home", new { area = "Frontend" });
            }

            return View(data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SatisfySurveyUpload(string id, VM_OtherUpload datapost)
        {
            string decrypt_id = EncryptService.AES.RandomizedDecrypt(id);
            Int64 _decrypt_id = 0; Int64.TryParse(decrypt_id, out _decrypt_id);

            string Feature = "諮詢服務滿意度問卷", Action = "新增";

            // 最終動作成功與否
            bool isSuccess = false;

            // 例外錯誤發生，特別記錄至 TbLog
            bool unCaughtError = false;

            TbConsultRegister? main = _fileService.Lookup<TbConsultRegister>(ref _message, x => x.Id == _decrypt_id).FirstOrDefault();
            main.ModifyDate = DateTime.Now;
            main.ModifyUser = "使用者上傳";

            try
            {
                using (var transaction = _fileService.GetTransaction())
                {
                    try
                    {
                        if (datapost.ModifyFile != null)
                        {
                            var upload = await _fileService.FileUploadAsync(datapost.ModifyFile,"SatisfySurveyFile/" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                                                                                  "SatisfySurveyFile",
                                                                                  null,
                                                                                  null,
                                                                                  transaction);

                            if (upload.IsSuccess == true && !string.IsNullOrEmpty(upload.FileID))
                            {
                                main.SatisfySurveyFile = upload.FileID;
                            }
                            else
                            {
                                _message += upload.Message;
                            }
                        }
                        //新增
                        await _fileService.Update(main, transaction);

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
            catch (Exception ex)
            {
                TempData["TempMsg"] = "伺服器連線異常，請檢查您的網路狀態後再試一次！";

                _message += ex.ToString();
                unCaughtError = true;
            }


            if (isSuccess)
            {
                TempData["TempMsgType"] = MsgTypeEnum.success;
                TempData["TempMsg"] = "諮詢服務滿意度問卷上傳成功";
            }
            else
            {
                TempData["TempMsgType"] = MsgTypeEnum.error;
                TempData["TempMsg"] = TempData["TempMsg"] ?? "儲存失敗";

                if (unCaughtError)
                {
                    await _allCommonService.Error_Record("Frontend", Feature + "-" + Action, _message);
                }
            }

            if (isSuccess)
            {
                return RedirectToAction("Index","Home"); //導向首頁
            }
            else
            {
                return View(datapost);
            }
        }

        /// <summary>企業需求調查表上傳</summary>
        public async Task<IActionResult> RequireSurveyUpload(string id)
        {
            VM_OtherUpload data = new VM_OtherUpload();

            string decrypt_id = EncryptService.AES.RandomizedDecrypt(id);
            Int64 _decrypt_id = 0; Int64.TryParse(decrypt_id, out _decrypt_id);

            try
            {
                //檢查ID是否存在
                TbConsultRegister? main = _fileService.Lookup<TbConsultRegister>(ref _message, x => x.Id == _decrypt_id).FirstOrDefault();

                if (main == null)
                    throw new Exception("查無此筆報名資料");
            }
            catch (Exception ex)
            {
                TempData["TempMsgType"] = MsgTypeEnum.error;
                TempData["TempMsg"] = TempData["TempMsg"] ?? "儲存失敗";

                return RedirectToAction("Index", "Home", new { area = "Frontend" });
            }

            return View(data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequireSurveyUpload(string id, VM_OtherUpload datapost)
        {
            string decrypt_id = EncryptService.AES.RandomizedDecrypt(id);
            Int64 _decrypt_id = 0; Int64.TryParse(decrypt_id, out _decrypt_id);

            string Feature = "企業需求調查表上傳", Action = "新增";

            // 最終動作成功與否
            bool isSuccess = false;

            // 例外錯誤發生，特別記錄至 TbLog
            bool unCaughtError = false;

            TbConsultRegister? main = _fileService.Lookup<TbConsultRegister>(ref _message, x => x.Id == _decrypt_id).FirstOrDefault();
            main.ModifyDate = DateTime.Now;
            main.ModifyUser = "使用者上傳";

            try
            {
                using (var transaction = _fileService.GetTransaction())
                {
                    try
                    {
                        //輪播圖片
                        if (datapost.ModifyFile != null)
                        {
                            var photo_upload = await _fileService.FileUploadAsync(datapost.ModifyFile,"Survey/" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                                                                                  "Survey",
                                                                                  null,
                                                                                  null,
                                                                                  transaction);

                            if (photo_upload.IsSuccess == true && !string.IsNullOrEmpty(photo_upload.FileID))
                            {
                                main.RequireSurveyFile = photo_upload.FileID;
                            }
                            else
                            {
                                _message += photo_upload.Message;
                            }
                        }
                        //新增
                        await _fileService.Update(main, transaction);

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
            catch (Exception ex)
            {
                TempData["TempMsg"] = "伺服器連線異常，請檢查您的網路狀態後再試一次！";

                _message += ex.ToString();
                unCaughtError = true;
            }


            if (isSuccess)
            {
                TempData["TempMsgType"] = MsgTypeEnum.success;
                TempData["TempMsg"] = "健康聲明書上傳成功";
            }
            else
            {
                TempData["TempMsgType"] = MsgTypeEnum.error;
                TempData["TempMsg"] = TempData["TempMsg"] ?? "儲存失敗";

                if (unCaughtError)
                {
                    await _allCommonService.Error_Record("Frontend", Feature + "-" + Action, _message);
                }
            }

            if (isSuccess)
            {
                return RedirectToAction("List"); //導向清單頁
            }
            else
            {
                return View(datapost);
            }
        }
    }
}
