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

namespace BASE.Areas.Frontend.Controllers
{
    public class ResourceController : BaseController
    {
        private readonly IConfiguration _conf;
        private readonly AllCommonService _allCommonService;
        private readonly FileService _fileService;
        private readonly OnePageService _onepageService;


        public ResourceController(IConfiguration configuration,
            AllCommonService allCommonService,
            OnePageService onepageService,
            FileService fileService)
        {
            _conf = configuration;
            _allCommonService = allCommonService;
            _onepageService = onepageService;
            _fileService = fileService;
        }

        /// <summary>計畫資源地圖</summary>d
        public async Task<IActionResult> Map()
        {
            VM_OnePage data = new VM_OnePage();

            try
            {
                var dataList = _onepageService.GetExtendItem(ref _message, "123"); //TODO: 待補編號
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Home", new { area = "Frontend" });
            }

            return View(data);
        }

    }
}
