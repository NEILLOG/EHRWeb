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
    public class AdvertiseService : ServiceBase
    {
        public AdvertiseService(DBContext context) : base(context)
        {
        }

        /// <summary>
        /// Advertise列表
        /// </summary>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public IQueryable<AdvertiseExtend>? GetAdvertiseExtendList(ref String ErrMsg)
        {
            try
            {
                IQueryable<AdvertiseExtend> dataList = (from adv in _context.TbAdvertise
                                                  where adv.IsDelete == false
                                                  select new AdvertiseExtend
                                                  {
                                                      Advertise = adv
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
        /// Advertise列表
        /// </summary>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public int? GetAdvertisMAXSort(ref String ErrMsg)
        {
            try
            {
                var dataList = (from adv in _context.TbAdvertise
                                                        where adv.IsDelete == false
                                                        select adv.Sort);

                return dataList.Max();
            }
            catch (Exception ex)
            {
                ErrMsg = ex.ToString();
                return null;
            }
        }

        /// <summary>
        /// Advertis項目
        /// </summary>
        /// <param name="ErrMsg"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public IQueryable<AdvertiseExtend>? GetAdvertisExtendItem(ref string ErrMsg, long id)
        {
            try
            {
                IQueryable<AdvertiseExtend>? dataList = (from adv in _context.TbAdvertise
                                                   where adv.IsDelete == false && adv.Id == id
                                                   select new AdvertiseExtend
                                                   {
                                                       Advertise = adv
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
