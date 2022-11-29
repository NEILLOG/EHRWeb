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


        public ResourceController(IConfiguration configuration,
            AllCommonService allCommonService,
            OnePageService onepageService,
            FileService fileService,
            ProjectService proejctService)
        {
            _conf = configuration;
            _allCommonService = allCommonService;
            _onepageService = onepageService;
            _fileService = fileService;
            _proejctService = proejctService;
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
                        //輪播圖片
                        if (datapost.ModifyFile != null)
                        {
                            var photo_upload = await _fileService.FileUploadAsync(datapost.ModifyFile, 
                                                                                  "ProjectModifyFiles/" + DateTime.Now.ToString("yyyyMMddHHmmss"), 
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
                TempData["TempMsg"] = "儲存成功";
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


        /// <summary>
        /// 健康聲明表調查
        /// </summary>
        public async Task<IActionResult> SurveyUpload(string id)
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
        public async Task<IActionResult> SurveyUpload(string id, VM_OtherUpload datapost)
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
                            var photo_upload = await _fileService.FileUploadAsync(datapost.ModifyFile,
                                                                                  "Survey/" + DateTime.Now.ToString("yyyyMMddHHmmss"),
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
