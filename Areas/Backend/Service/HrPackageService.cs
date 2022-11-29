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
    public class HrPackageService : ServiceBase
    {
        public HrPackageService(DBContext context) : base(context)
        {
        }

        /// <summary>
        /// HrPackage列表
        /// </summary>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public IQueryable<HrPackageExtend>? GetHrPackageExtendList(ref String ErrMsg)
        {
            try
            {
                IQueryable<HrPackageExtend> dataList = (from HrPackage in _context.TbHrPackage
                                                        where HrPackage.IsDelete == false
                                                        select new HrPackageExtend
                                                        {
                                                            HrPackage = HrPackage
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
        /// HrPackage項目
        /// </summary>
        /// <param name="ErrMsg"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public IQueryable<HrPackageExtend>? GetHrPackageExtendItem(ref string ErrMsg, string id)
        {
            try
            {
                IQueryable<HrPackageExtend>? dataList = (from HrPackage in _context.TbHrPackage
                                                         where HrPackage.Id == id && HrPackage.IsDelete == false
                                                         select new HrPackageExtend
                                                         {
                                                             HrPackage = HrPackage
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
        /// HrPackage列表
        /// </summary>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public int? GetHrPackageMAXSort(ref String ErrMsg)
        {
            try
            {
                var dataList = (from HrPackage in _context.TbHrPackage
                                where HrPackage.IsDelete == false
                                select HrPackage.Sort);

                return dataList.Max();
            }
            catch (Exception ex)
            {
                ErrMsg = ex.ToString();
                return null;
            }
        }

        /// <summary>
        /// OnePage項目
        /// </summary>
        /// <param name="ErrMsg"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public IQueryable<OnePageExtend>? GetOnePageExtendItem(ref string ErrMsg, string id)
        {
            try
            {
                IQueryable<OnePageExtend>? dataList = (from OnePage in _context.TbOnePage
                                                       where OnePage.Id == id
                                                       select new OnePageExtend
                                                       {
                                                           OnePage = OnePage
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
        /// OnePage項目
        /// </summary>
        /// <param name="ErrMsg"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public string GetOnePageId(ref string ErrMsg, string Description)
        {
            try
            {
                string strId = (from OnePage in _context.TbOnePage
                                where OnePage.Description == Description
                                select OnePage.Id).FirstOrDefault().ToString();

                return strId;
            }
            catch (Exception ex)
            {
                ErrMsg = ex.ToString();
                return null;
            }
        }
    }
}
