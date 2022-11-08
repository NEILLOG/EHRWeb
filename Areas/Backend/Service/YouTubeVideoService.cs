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
    public class YouTubeVideoService : ServiceBase
    {
        public YouTubeVideoService(DBContext context) : base(context)
        {
        }

        /// <summary>
        /// YouTubeVideo列表
        /// </summary>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public IQueryable<YouTubeVideoExtend>? GetYouTubeVideoExtendList(ref String ErrMsg)
        {
            try
            {
                IQueryable<YouTubeVideoExtend> dataList = (from ytv in _context.TbYouTubeVideo
                                                  where ytv.IsDelete == false
                                                  select new YouTubeVideoExtend
                                                  {
                                                      YouTubeVideo = ytv
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
        /// YouTubeVideo項目
        /// </summary>
        /// <param name="ErrMsg"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public IQueryable<YouTubeVideoExtend>? GetYouTubeVideoxtendItem(ref string ErrMsg, int id)
        {
            try
            {
                IQueryable<YouTubeVideoExtend>? dataList = (from ytv in _context.TbYouTubeVideo
                                                   where ytv.IsDelete == false && ytv.Id == id
                                                   select new YouTubeVideoExtend
                                                   {
                                                       YouTubeVideo = ytv
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
