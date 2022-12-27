using BASE.Areas.Backend.Models;
using BASE.Areas.Backend.Models.Extend;
using BASE.Extensions;
using BASE.Models.DB;
using BASE.Models.Enums;
using BASE.Service.Base;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BASE.Areas.Backend.Service
{
    public class NewsService : ServiceBase
    {
        public NewsService(DBContext context) : base(context)
        {
        }

        /// <summary>
        /// News列表
        /// </summary>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public IQueryable<NewsExtend>? GetNewsExtendList(ref String ErrMsg)
        {
            try
            {
                IQueryable<NewsExtend> dataList = (from news in _context.TbNews
                                                  where news.IsDelete == false
                                                  select new NewsExtend
                                                  {
                                                      News = news
                                                  });
                return dataList;
            }
            catch (Exception ex)
            {
                ErrMsg = ex.ToString();
                return null;
            }
        }

        /// <summary>
        /// News項目
        /// </summary>
        /// <param name="ErrMsg"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public IQueryable<NewsExtend>? GetNewsExtendItem(ref string ErrMsg, string id)
        {
            try
            {
                IQueryable<NewsExtend>? dataList = (from news in _context.TbNews
                                                         where news.IsDelete == false && news.Id == id
                                                         select new NewsExtend
                                                         {
                                                             News = news
                                                         });

                return dataList;
            }
            catch (Exception ex)
            {
                ErrMsg = ex.ToString();
                return null;
            }
        }

        /// <summary>
        /// 取得置頂ID
        /// </summary>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public List<string> GetTopId(ref string ErrMsg)
        {
            try
            {
                List<string> dataList = (from news in _context.TbNews
                                                    where news.IsDelete == false && 
                                                          news.IsKeepTop == true
                                                    select  news.Id).ToList();

                return dataList;
            }
            catch (Exception ex)
            {
                ErrMsg = ex.ToString();
                return null;
            }
        }
    }
}
