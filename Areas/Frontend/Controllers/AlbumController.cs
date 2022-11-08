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
    public class AlbumController : BaseController
    {
        private readonly IConfiguration _conf;
        private readonly AllCommonService _allCommonService;
        private readonly FileService _fileService;
        private readonly AlbumService _albumService;


        public AlbumController(IConfiguration configuration,
            AllCommonService allCommonService,
            AlbumService albumService,
            FileService fileService)
        {
            _conf = configuration;
            _allCommonService = allCommonService;
            _albumService = albumService;
            _fileService = fileService;
        }

        public async Task<IActionResult> List()
        {
            VM_Album data = new VM_Album();

            try
            {
                //取資料
                IQueryable<AlbumExtend>? dataList = _albumService.GetList(ref _message, data.Search);

                //分頁
                if (dataList != null)
                    data.AlbumExtendList = await PagerInfoService.GetRange(dataList, data.Search.PagerInfo);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Home", new { area = "Frontend" });
            }

            return View(data);
        }

        /// <summary>最新消息首頁POST</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> List(VM_Album datapost)
        {
            try
            {
                //取資料
                IQueryable<AlbumExtend>? dataList = _albumService.GetList(ref _message, datapost.Search);

                //分頁
                if (dataList != null)
                    datapost.AlbumExtendList = await PagerInfoService.GetRange(dataList, datapost.Search.PagerInfo);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Home", new { area = "Frontend" });
            }


            return View(datapost);
        }


    }
}
