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
    public class NewsController : BaseController
    {
        private readonly IConfiguration _conf;
        private readonly AllCommonService _allCommonService;
        private readonly FileService _fileService;
        private readonly NewsService _newsService;


        public NewsController(IConfiguration configuration,
            AllCommonService allCommonService,
            NewsService newsService,
            FileService fileService)
        {
            _conf = configuration;
            _allCommonService = allCommonService;
            _newsService = newsService;
            _fileService = fileService;
        }

        /// <summary>最新消息首頁</summary>d
        public async Task<IActionResult> List(String id = "")
        {
            VM_News data = new VM_News();
            switch (id)
            {
                case "計畫消息":
                case "其他消息": 
                    data.Search.Category = id;
                    ViewBag.Title = "最新消息 - " + id;
                    break;
                default:
                    data.Search.Category = String.Empty;
                    ViewBag.Title = "最新消息 - 全部";
                    break;
            }

            try
            {
                //取資料
                IQueryable<NewsExtend>? dataList = _newsService.GetNewsList(ref _message, data.Search);

                //分頁
                if (dataList != null)
                    data.NewsExtendList = await PagerInfoService.GetRange(dataList, data.Search.PagerInfo);
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
        public async Task<IActionResult> List(VM_News datapost)
        {
            try
            {
                ViewBag.Title = datapost.Search.Category;

                //取資料
                IQueryable<NewsExtend>? dataList = _newsService.GetNewsList(ref _message, datapost.Search);

                //分頁
                if (dataList != null)
                    datapost.NewsExtendList = await PagerInfoService.GetRange(dataList, datapost.Search.PagerInfo);
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
            VM_News data = new VM_News();

            string decrypt_id = EncryptService.AES.RandomizedDecrypt(id);

            try
            {
                data.NewsExtendItem = _newsService.GetNewsExtendItem(ref _message, decrypt_id);

                ViewBag.Title = "最新消息 - " + data.NewsExtendItem.Header.Title;
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Home", new { area = "Frontend" });
            }

            return View(data);
        }

    }
}
