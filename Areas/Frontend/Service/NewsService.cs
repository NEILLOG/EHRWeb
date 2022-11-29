using BASE.Areas.Frontend.Models;
using BASE.Areas.Frontend.Models.Extend;
using BASE.Models.DB;
using BASE.Models.Enums;
using BASE.Service;
using BASE.Service.Base;
using Microsoft.EntityFrameworkCore;

namespace BASE.Areas.Frontend.Service
{
    public class NewsService : ServiceBase
    {
        private readonly IConfiguration _conf;
        private readonly AllCommonService _allCommonService;
        public NewsService(DBContext context,
            AllCommonService allCommonService,
            IConfiguration configuration) : base(context)
        {
            _conf = configuration;
            _allCommonService = allCommonService;
        }

        /// <summary>
        /// 取得最新消息列表
        /// </summary>
        /// <returns></returns>
        public IQueryable<NewsExtend>? GetNewsList(ref String ErrMsg, VM_NewsQueryParam? vmParam)
        {
            DateTime dtnow = DateTime.Now;
            try
            {
                IQueryable<NewsExtend>? dataList = (from NewsInfo in _context.TbNews.Where(x => !x.IsDelete && x.IsPublish)

                                                    join FileInfo in _context.TbFileInfo.Where(x => !x.IsDelete) on NewsInfo.FileId equals FileInfo.FileId into News_File
                                                    from FileInfo in News_File.DefaultIfEmpty()
                                                    
                                                    orderby NewsInfo.IsKeepTop descending, NewsInfo.DisplayDate descending
                                                    select new NewsExtend
                                                    {
                                                        Header = NewsInfo,
                                                        FileInfo = FileInfo,
                                                    });

                if (vmParam != null)
                {
                    if (!string.IsNullOrEmpty(vmParam.Keyword))
                    {
                        dataList = dataList.Where(x => x.Header.Title.Contains(vmParam.Keyword) || x.Header.Contents.Contains(vmParam.Keyword));
                    }

                    if (!String.IsNullOrEmpty(vmParam.Category))
                        switch (vmParam.Category)
                        {
                            case "計畫消息":
                            case "其他消息":
                                dataList = dataList.Where(x => x.Header.Category == vmParam.Category);
                                break;
                        }
                }
                return dataList;
            }
            catch (Exception ex)
            {
                ErrMsg = ex.ToString();
                return null;
            }
        }

        /// <summary>取得最新消息內頁</summary>
        public NewsExtend GetNewsExtendItem(ref String ErrMsg, string id)
        {
            DateTime dtNow = DateTime.Now;
            try
            {
                List<NewsExtend> allDataList = (from NewsInfo in _context.TbNews.Where(x => !x.IsDelete && x.IsPublish)
                                                join FileInfo in _context.TbFileInfo.Where(x => !x.IsDelete) on NewsInfo.FileId equals FileInfo.FileId into News_File
                                                from FileInfo in News_File.DefaultIfEmpty()
                                                select new NewsExtend
                                                {
                                                    Header = NewsInfo,
                                                    FileInfo = FileInfo,
                                                }).ToList();
                // 找出該筆資料
                NewsExtend data = allDataList.Where(x => x.Header.Id == id).FirstOrDefault();

                int index = allDataList.FindIndex(x => x.Header.Id == id);

                return data;
            }
            catch (Exception ex)
            {
                ErrMsg = ex.ToString();
                return null;
            }
        }

    }
}
