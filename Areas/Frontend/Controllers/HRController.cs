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
    public class HRController : BaseController
    {
        private readonly IConfiguration _conf;
        private readonly AllCommonService _allCommonService;
        private readonly FileService _fileService;
        private readonly HRService _hrService;
        private readonly OnePageService _onepageService;

        public HRController(IConfiguration configuration,
            AllCommonService allCommonService,
            HRService hrService,
            FileService fileService,
            OnePageService onepageService)
        {
            _conf = configuration;
            _allCommonService = allCommonService;
            _hrService = hrService;
            _fileService = fileService;
            _onepageService = onepageService;
        }

        public async Task<IActionResult> List(String id = "")
        {
            VM_HR data = new VM_HR();
            switch (id)
            {
                case "成功案例分享":
                case "HR知識充電站":
                    data.Search.Category = id;
                    ViewBag.Title = id;
                    break;
                default:
                    data.Search.Category = "HR知識充電站";
                    ViewBag.Title = "HR知識充電站";
                    break;
            }

            try
            {
                //取資料
                IQueryable<HRExtend>? dataList = _hrService.GetList(ref _message, data.Search);

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
        public async Task<IActionResult> List(VM_HR datapost)
        {
            try
            {
                ViewBag.Title = datapost.Search.Category;

                //取資料
                IQueryable<HRExtend>? dataList = _hrService.GetList(ref _message, datapost.Search);

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

            VM_HR data = new VM_HR();

            long decrypt_id = Convert.ToInt64(EncryptService.AES.RandomizedDecrypt(id));

            try
            {
                data.ExtendItem = _hrService.GetExtendItem(ref _message, decrypt_id);
                ViewBag.Title = data.ExtendItem.Header.Title;
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Home", new { area = "Frontend" });
            }

            return View(data);
        }


        public async Task<IActionResult> Manual()
        {
            VM_OnePage data = new VM_OnePage();

            try
            {
                data.ExtendItem = _onepageService.GetExtendItem(ref _message, "OP000002"); //TODO: 待補編號
                ViewBag.Title = "HR工具說明書";
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Home", new { area = "Frontend" });
            }

            return View(data);
        }


        public async Task<IActionResult> Packages()
        {
            VM_HRPackage data = new VM_HRPackage();

            ViewBag.Title = "HR材料包";

            try
            {
                //取資料
                IQueryable<HRPackageExtend>? dataList = _hrService.GetPackageList(ref _message, data.Search);

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
        public async Task<IActionResult> Packages(VM_HRPackage datapost)
        {
            try
            {
                ViewBag.Title = "HR材料包";

                //取資料
                IQueryable<HRPackageExtend>? dataList = _hrService.GetPackageList(ref _message, datapost.Search);

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
    }
}
