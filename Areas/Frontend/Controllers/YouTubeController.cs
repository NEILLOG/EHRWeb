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
    public class YouTubeController : BaseController
    {
        private readonly IConfiguration _conf;
        private readonly AllCommonService _allCommonService;
        private readonly FileService _fileService;
        private readonly YouTubeService _youtubeService;


        public YouTubeController(IConfiguration configuration,
            AllCommonService allCommonService,
            YouTubeService youtubeService,
            FileService fileService)
        {
            _conf = configuration;
            _allCommonService = allCommonService;
            _youtubeService = youtubeService;
            _fileService = fileService;
        }

        public async Task<IActionResult> List()
        {
            VM_YouTube data = new VM_YouTube();

            try
            {
                //取資料
                IQueryable<TbYouTubeVideo>? dataList = _youtubeService.GetYouTubeList(ref _message, data.Search);

                //分頁
                if (dataList != null)
                    data.DataList = await PagerInfoService.GetRange(dataList, data.Search.PagerInfo);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Home", new { area = "Frontend" });
            }

            return View(data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> List(VM_YouTube datapost)
        {
            try
            {
                //取資料
                IQueryable<TbYouTubeVideo>? dataList = _youtubeService.GetYouTubeList(ref _message, datapost.Search);

                //分頁
                if (dataList != null)
                    datapost.DataList = await PagerInfoService.GetRange(dataList, datapost.Search.PagerInfo);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Home", new { area = "Frontend" });
            }

            return View(datapost);
        }

    }
}
