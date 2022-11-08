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
using BASE.Areas.Backend.Models;
using BASE.Areas.Backend.Service;

namespace BASE.Areas.Frontend.Controllers
{
    public class ActivityController : BaseController
    {
        private readonly IConfiguration _conf;
        private readonly AllCommonService _allCommonService;
        private readonly FileService _fileService;
        private readonly ActivityService _activityService;

        public ActivityController(IConfiguration configuration,
            AllCommonService allCommonService,
            ActivityService activtyService,
            FileService fileService)
        {
            _conf = configuration;
            _allCommonService = allCommonService;
            _activityService = activtyService;
            _fileService = fileService;
        }

        public async Task<IActionResult> List(String id = "")
        {
            VM_Activity data = new VM_Activity();
            switch (id)
            {
                case "已完成辦理之活動":
                case "講座":
                case "課程":
                case "活動": data.Search.Category = id; break;
                default:
                    data.Search.Category = "課程"; break;
            }

            try
            {
                //取資料
                IQueryable<ActivityExtend>? dataList = _activityService.GetActivityList(ref _message, data.Search);

                //分頁
                if (dataList != null)
                    data.ActivityExtendList = await PagerInfoService.GetRange(dataList, data.Search.PagerInfo);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Home", new { area = "Frontend" });
            }

            return View(data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> List(VM_Activity datapost)
        {
            try
            {
                //取資料
                IQueryable<ActivityExtend>? dataList = _activityService.GetActivityList(ref _message, datapost.Search);

                //分頁
                if (dataList != null)
                    datapost.ActivityExtendList = await PagerInfoService.GetRange(dataList, datapost.Search.PagerInfo);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Home", new { area = "Frontend" });
            }


            return View(datapost);
        }

        /// <summary> 最新消息-詳細頁</summary>
        public async Task<IActionResult> Detail(string id)
        {

            VM_Activity data = new VM_Activity();

            string decrypt_id = EncryptService.AES.RandomizedDecrypt(id);

            try
            {
                data.ActivityExtendItem = _activityService.GetActivityExtendItem(ref _message, decrypt_id);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Home", new { area = "Frontend" });
            }

            return View(data);
        }

        public async Task<IActionResult> Register(string id)
        {
            VM_ActivityReigster data = new VM_ActivityReigster();

            string decrypt_id = EncryptService.AES.RandomizedDecrypt(id);

            try
            {
                data.Sections = _activityService.GetSections(ref _message, decrypt_id).ToList();
                data.id = id;
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Home", new { area = "Frontend" });
            }

            return View(data);
        }

        [HttpPost]
        public async Task<IActionResult> Register(VM_ActivityReigster data)
        {
            string decrypt_id = EncryptService.AES.RandomizedDecrypt(data.id);//postback後仍然要解密
            string Feature = "活動報名", Action = "新增";
            bool isSuccess = false; // 最終動作成功與否
            bool unCaughtError = false;  // 例外錯誤發生，特別記錄至 TbLog

            DateTime dtnow = DateTime.Now;
            TbActivityRegister? main = new TbActivityRegister();
            List<TbActivityRegisterSection>? RegSections = new List<TbActivityRegisterSection>();

            try
            {
                data.Sections = _activityService.GetSections(ref _message, decrypt_id).ToList();

                if (data.Main == null || data.RegisterSection == null || data.RegisterSection.Count() <= 0)
                {
                    TempData["TempMsg"] = "資料回傳有誤，請重新操作！";
                }

                main.CompanyName = data.Main.CompanyName;
                main.CompanyLocation = data.Main.CompanyLocation;
                main.CompanyType = data.Main.CompanyType;
                main.Name = data.Main.Name;
                main.JobTitle = data.Main.JobTitle;
                main.Phone = data.Main.Phone;
                main.CellPhone = data.Main.CellPhone;
                main.Email = data.Main.Email;
                main.CompanyEmpAmount = data.Main.CompanyEmpAmount;
                main.InfoFrom = String.Join(",", data.ckbsInfoFrom);
                main.ActivityId = decrypt_id; //給解密後的ID
                main.CreateDate = DateTime.Now;

                foreach (var item in data.RegisterSection)
                {
                    TbActivityRegisterSection m = new TbActivityRegisterSection();
                    m.ActivityId = decrypt_id;  //給解密後的ID
                    m.RegisterSectionId = item.RegisterSectionId;
                    m.RegisterSectionType = item.RegisterSectionType;
                    m.IsVegin = item.IsVegin;

                    RegSections.Add(m);
                }

                using (var transaction = _activityService.GetTransaction())
                {
                    try
                    {
                        //新增
                        await _activityService.Insert(main, transaction);
                        //target_id = main.Id; //新增後的主鍵存起來

                        //新增場次
                        foreach (var reg in RegSections)
                        {
                            reg.RegisterId = main.Id;
                            await _activityService.Insert(reg, transaction);
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
                    await _allCommonService.Error_Record("FrontEnd", Feature + "-" + Action, _message);
                }
            }

            if (isSuccess)
                return RedirectToAction("Detail", new { id = data.id }); //給未解密的ID
            else
            {
                if (main == null)
                    return RedirectToAction("Detail", new { id = data.id }); //給未解密的ID
                else
                    return View(data); /* 失敗回原頁 */
            }

        }
    }
}
