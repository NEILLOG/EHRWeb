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
    public class FAQService : ServiceBase
    {
        public FAQService(DBContext context) : base(context)
        {
        }


        /// <summary>
        /// FAQ列表
        /// </summary>
        /// <param name="ErrMsg"></param>
        /// <param name="vmParam"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public IQueryable<FAQExtend>? GetFAQExtendList(ref String ErrMsg, VM_FAQQueryParam? vmParam, Expression<Func<FAQExtend, bool>>? filter = null)
        {
            try
            {
                IQueryable<FAQExtend> dataList = (from faq in _context.NtuFaq
                                                  where faq.IsDelete == false
                                                  select new FAQExtend
                                                  {
                                                      FAQ = faq
                                                  });

                if (filter != null)
                {
                    dataList = dataList.Where(filter);
                }

                if (vmParam != null)
                {
                    if (!string.IsNullOrEmpty(vmParam.Keyword))
                    {
                        //關鍵字搜尋：問題、答案
                        dataList = dataList.Where(x => x.FAQ.Question.Contains(vmParam.Keyword) ||
                                                    x.FAQ.Answer.Contains(vmParam.Keyword));
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

        /// <summary>
        /// FAQ項目
        /// </summary>
        /// <param name="ErrMsg"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public IQueryable<FAQExtend>? GetFAQExtendItem(ref string ErrMsg, string id)
        {
            try
            {
                IQueryable<FAQExtend>? dataList = (from faq in _context.NtuFaq
                                                   where faq.IsDelete == false && faq.Fid == id
                                                   select new FAQExtend
                                                   {
                                                       FAQ = faq
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
