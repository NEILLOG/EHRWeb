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
    public class AlbumService : ServiceBase
    {
        public AlbumService(DBContext context) : base(context)
        {
        }

        /// <summary>
        /// Album列表
        /// </summary>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public IQueryable<AlbumExtend>? GetAlbumExtendList(ref String ErrMsg)
        {
            try
            {
                IQueryable<AlbumExtend> dataList = (from abl in _context.TbAlbum
                                                  where abl.IsDelete == false
                                                  select new AlbumExtend
                                                  {
                                                      Album = abl
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
        /// Advertis項目
        /// </summary>
        /// <param name="ErrMsg"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public IQueryable<AlbumExtend>? GetAlbumxtendItem(ref string ErrMsg, string id)
        {
            try
            {
                IQueryable<AlbumExtend>? dataList = (from abl in _context.TbAlbum
                                                   where abl.IsDelete == false && abl.Id == id
                                                   select new AlbumExtend
                                                   {
                                                       Album = abl
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
