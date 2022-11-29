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
    public class HrArticleService : ServiceBase
    {
        public HrArticleService(DBContext context) : base(context)
        {
        }

        /// <summary>
        /// HrArticle列表
        /// </summary>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public IQueryable<HrArticleExtend>? GetHrArticleExtendList(ref String ErrMsg)
        {
            try
            {
                IQueryable<HrArticleExtend> dataList = (from HrArticle in _context.TbHrArticle
                                                  select new HrArticleExtend
                                                  {
                                                      HrArticle = HrArticle
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
        /// HrArticle項目
        /// </summary>
        /// <param name="ErrMsg"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public IQueryable<HrArticleExtend>? GetHrArticleExtendItem(ref string ErrMsg, long id)
        {
            try
            {
                IQueryable<HrArticleExtend>? dataList = (from HrArticle in _context.TbHrArticle
                                                         where HrArticle.Id == id
                                                         select new HrArticleExtend
                                                         {
                                                             HrArticle = HrArticle
                                                         });

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
