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
using System.Net.Mail;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using System.IO;

namespace BASE.Areas.Frontend.Controllers
{
    public class ResourceController : BaseController
    {
        private readonly IConfiguration _conf;
        private readonly AllCommonService _allCommonService;
        private readonly FileService _fileService;
        private readonly OnePageService _onepageService;
        private readonly ProjectService _proejctService;
        private readonly MailService _mailService;

        public ResourceController(IConfiguration configuration,
            AllCommonService allCommonService,
            OnePageService onepageService,
            FileService fileService,
            ProjectService proejctService,
            MailService mailService)
        {
            _conf = configuration;
            _allCommonService = allCommonService;
            _onepageService = onepageService;
            _fileService = fileService;
            _proejctService = proejctService;
            _mailService = mailService;
        }

        /// <summary>計畫資源地圖</summary>d
        public async Task<IActionResult> Map()
        {
            ViewBag.Title = "計畫資源地圖";
            VM_OnePage data = new VM_OnePage();

            try
            {
                data.ExtendItem = _onepageService.GetExtendItem(ref _message, "OP000001"); //TODO: 待補編號
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Home", new { area = "Frontend" });
            }

            return View(data);
        }

        public async Task<IActionResult> List(String id = "")
        {
            VM_Project data = new VM_Project();
            switch (id)
            {
                case "企業訓練資源":
                case "就業服務資源":
                case "紓困資源":
                case "其他資源": 
                    data.Search.Category = id;
                    ViewBag.Title = id;
                    break;
                default:
                    data.Search.Category = "企業訓練資源";
                    ViewBag.Title = "企業訓練資源";
                    break;
            }

            try
            {
                //取資料
                var dataList = _proejctService.GetList(ref _message, data.Search);

                //分頁
                if (dataList != null)
                    data.ExtendList = await PagerInfoService.GetRange(dataList, data.Search.PagerInfo);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Home", new { area = "Frontend" });
            }

            return View(data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> List(VM_Project datapost)
        {
            try
            {
                ViewBag.Title = datapost.Search.Category;

               //取資料
               var dataList = _proejctService.GetList(ref _message, datapost.Search);

                //分頁
                if (dataList != null)
                    datapost.ExtendList = await PagerInfoService.GetRange(dataList, datapost.Search.PagerInfo);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Home", new { area = "Frontend" });
            }


            return View(datapost);
        }

        public async Task<IActionResult> Detail(string id)
        {
            string b = TempData["TempMsg"] == null ? "" : TempData["TempMsg"].ToString();

            VM_Project data = new VM_Project();

            string decrypt_id = EncryptService.AES.RandomizedDecrypt(id);

            var FixedProjectIds = _conf.GetSection("Custom:FixedProjectIds").Get<List<string>>();
            if (FixedProjectIds.Contains(decrypt_id))
            {
                data.IsShowModifyArea = true;
                switch (decrypt_id)
                {
                    //充電起飛計畫
                    case "PJ00000002": data.SampleFilePath = "/sample/modify3.odt"; break; //因無障礙須優先提供開放格式，故先改為odt
                    //企業人力資源提升計畫
                    case "PJ00000001": data.SampleFilePath = "/sample/modify2.odt"; break; //因無障礙須優先提供開放格式，故先改為odt
                    //小型企業人力提升計畫
                    case "PJ00000003": data.SampleFilePath = "/sample/modify4.odt"; break; //因無障礙須優先提供開放格式，故先改為odt
                    //在職中高齡者及高齡者穩定就業訓練補助實施計畫
                    case "PJ00000004": data.SampleFilePath = "/sample/modify5.odt"; break; //因無障礙須優先提供開放格式，故先改為odt
                    //中高齡者退休後再就業準備訓練補助實施計畫
                    case "PJ00000005": data.SampleFilePath = "/sample/modify6.odt"; break; //因無障礙須優先提供開放格式，故先改為odt
                    //充電再出發訓練計畫
                    case "PJ00000006": data.SampleFilePath = "/sample/modify1.odt"; break; //因無障礙須優先提供開放格式，故先改為odt
                }
            }

            try
            {
                data.ExtendItem = _proejctService.GetItem(ref _message, decrypt_id);
                data.ModifyItem = new TbProjectModify() { ProjectId = id };

                ViewBag.Title = data.ExtendItem.Name;
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Home", new { area = "Frontend" });
            }

            return View(data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Modify(VM_Project datapost)
        {
            string Feature = "課程臨時變更申請新增", Action = "新增";

            // 最終動作成功與否
            bool isSuccess = false;

            // 例外錯誤發生，特別記錄至 TbLog
            bool unCaughtError = false;

            DateTime dtnow = DateTime.Now;
            string? target_id = null;

            try
            {
                // 取得 附檔名
                string extension = Path.GetExtension(datapost.ModifyFile.FileName);
                if (extension != ".png" && extension != ".jpg" && extension != ".jpeg" && extension != ".doc" && extension != ".docx" && extension != ".pdf")
                {
                    TempData["TempMsgType"] = MsgTypeEnum.warning;
                    TempData["TempMsg"] = "不支援此副檔名";

                    return RedirectToAction("Detail", new { id = datapost.ModifyItem.ProjectId });
                }

                datapost.ModifyItem.CreateDate = DateTime.Now;
                datapost.ModifyItem.ProjectId = EncryptService.AES.RandomizedDecrypt(datapost.ModifyItem.ProjectId);

                using (var transaction = _proejctService.GetTransaction())
                {
                    try
                    {
                        if (datapost.ModifyFile != null)
                        {
                            var photo_upload = await _fileService.FileUploadAsync(datapost.ModifyFile, "ProjectModifyFiles/" + DateTime.Now.ToString("yyyyMMddHHmmss"), 
                                                                                  "ProjectModifyFiles", 
                                                                                  null, 
                                                                                  null, 
                                                                                  transaction);

                            if (photo_upload.IsSuccess == true && !string.IsNullOrEmpty(photo_upload.FileID))
                            {
                                datapost.ModifyItem.FileId = photo_upload.FileID;
                            }
                            else
                            {
                                _message += photo_upload.Message;
                            }
                        }
                        //新增
                        await _proejctService.Insert(datapost.ModifyItem, transaction);

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
                TempData["TempMsg"] = "課程臨時變更申請完成";

                var proejct = _allCommonService.Lookup<TbProject>(ref _message, x => x.Id == datapost.ModifyItem.ProjectId).FirstOrDefault();

                //取得所有信箱
                var emails = String.IsNullOrEmpty(proejct.NotifyEmails) ? new List<String>() : proejct.NotifyEmails.Split(';').ToList();
                List<MailAddressInfo> _emails = new List<MailAddressInfo>();
                foreach(var email in emails)
                {
                    try
                    {
                        new MailAddress(email); //利用new的方式，來檢查email是否正確
                        _emails.Add(new MailAddressInfo(email));
                    } 
                    catch(Exception){continue;}
                }

                if(_emails.Count <= 0)
                {
                    TempData["TempMsgType"] = MsgTypeEnum.warning;
                    TempData["TempMsg"] = "課程臨時變更申請完成，但因系統發生問題，無法寄送通知信件至承辦單位，請電話與我們聯繫確認變更情形，謝謝";
                } 
                else 
                    await _mailService.SendEmail(new MailViewModel()
                    {
                        //Subject = MailTmeplate.Resource.MODIFY_APPLY_SUBJECT,
                        Subject = String.Format(MailTmeplate.Resource.MODIFY_APPLY_SUBJECT, proejct.Name),
                        Body    = String.Format(MailTmeplate.Resource.MODIFY_APPLY_CONTNET, DateTime.Now.ToString("yyyy年MM月dd日 ")),
                        ToList = _emails
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
                return RedirectToAction("Detail", new { id = EncryptService.AES.RandomizedEncrypt(datapost.ModifyItem.ProjectId) });
            }
            else
            {
                if (datapost.ModifyItem == null)
                {
                    return RedirectToAction("List", "Resource");
                }
                else
                {
                    return View(datapost);
                }
            }
        }


    
    }
}
