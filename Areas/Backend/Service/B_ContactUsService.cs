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
    public class B_ContactUsService : ServiceBase
    {
        public B_ContactUsService(DBContext context) : base(context)
        {
        }

        /// <summary>
        /// ContactUs列表
        /// </summary>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public IQueryable<TbContactUs>? GetContactUsdList(ref String ErrMsg)
        {
            try
            {
                IQueryable<TbContactUs> dataList = (from con in _context.TbContactUs
                                                    select con);
                return dataList;
            }
            catch (Exception ex)
            {
                ErrMsg = ex.ToString();
                return null;
            }
        }

        /// <summary>
        /// 帳號列表
        /// </summary>
        /// <returns></returns>
        public IQueryable<ContactUsExtend>? GetContactUsExtendList(ref String ErrMsg)
        {
            try
            {
                IQueryable<ContactUsExtend> dataList = (from con in _context.TbContactUs
                                                        select new ContactUsExtend
                                                        {
                                                            ContactUs = con
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
