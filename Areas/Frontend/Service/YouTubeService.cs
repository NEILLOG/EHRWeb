using BASE.Areas.Frontend.Models;
using BASE.Areas.Frontend.Models.Extend;
using BASE.Models.DB;
using BASE.Models.Enums;
using BASE.Service;
using BASE.Service.Base;
using Microsoft.EntityFrameworkCore;

namespace BASE.Areas.Frontend.Service
{
    public class YouTubeService : ServiceBase
    {
        private readonly IConfiguration _conf;
        private readonly AllCommonService _allCommonService;
        public YouTubeService(DBContext context,
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
        public IQueryable<TbYouTubeVideo>? GetYouTubeList(ref String ErrMsg, VM_YouTubeQueryParam? vmParam)
        {
            DateTime dtnow = DateTime.Now;
            try
            {
                IQueryable<TbYouTubeVideo>? dataList = (from YouTubeInfo in _context.TbYouTubeVideo.Where(x => !x.IsDelete && x.IsPublish)
                                                        orderby YouTubeInfo.CreateDate descending
                                                        select YouTubeInfo);

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
