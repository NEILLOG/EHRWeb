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
                case "其他資源": data.Search.Category = id; break;
                default:
                    data.Search.Category = "企業訓練資源"; break;
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
                    case "PJ00000002": data.SampleFilePath = "/sample/modify3.doc"; break;
                    //企業人力資源提升計畫
                    case "PJ00000001": data.SampleFilePath = "/sample/modify2.doc"; break;
                    //小型企業人力提升計畫
                    case "PJ00000003": data.SampleFilePath = "/sample/modify4.doc"; break;
                    //在職中高齡者及高齡者穩定就業訓練補助實施計畫
                    case "PJ00000004": data.SampleFilePath = "/sample/modify5.doc"; break;
                    //中高齡者退休後再就業準備訓練補助實施計畫
                    case "PJ00000005": data.SampleFilePath = "/sample/modify6.doc"; break;
                    //充電再出發訓練計畫
                    case "PJ00000006": data.SampleFilePath = "/sample/modify1.doc"; break;
                }
            }

            try
            {
                data.ExtendItem = _proejctService.GetItem(ref _message, decrypt_id);
                data.ModifyItem = new TbProjectModify() { ProjectId = id };
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
                var proejct_user = _allCommonService.Lookup<TbUserInfo>(ref _message, x => x.UserId == proejct.CreateUser).FirstOrDefault();

                await _mailService.SendEmail(new MailViewModel()
                {
                    Subject = MailTmeplate.Resource.MODIFY_APPLY_SUBJECT,
                    Body    = String.Format(MailTmeplate.Resource.MODIFY_APPLY_CONTNET, DateTime.Now.ToString("yyyy年MM月dd日 ")),
                    ToList = new List<MailAddressInfo>() { new MailAddressInfo(proejct_user.Email) }
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
