using BASE.Areas.Frontend.Models;
using BASE.Areas.Frontend.Models.Extend;
using BASE.Models.DB;
using BASE.Models.Enums;
using BASE.Service;
using BASE.Service.Base;
using Microsoft.EntityFrameworkCore;

namespace BASE.Areas.Frontend.Service
{
    public class AdService : ServiceBase
    {
        private readonly IConfiguration _conf;
        private readonly AllCommonService _allCommonService;
        public AdService(DBContext context,
            AllCommonService allCommonService,
            IConfiguration configuration) : base(context)
        {
            _conf = configuration;
            _allCommonService = allCommonService;
        }

        public List<AdExtend> GetExtendItemList(ref String ErrMsg)
        {
            try
            {
                List<AdExtend> allDataList = (from ad in _context.TbAdvertise.Where(x => !x.IsDelete && x.IsPublish)
                                                join FileInfo in _context.TbFileInfo.Where(x => !x.IsDelete) on ad.FileId equals FileInfo.FileId into ad_File
                                                from FileInfo in ad_File.DefaultIfEmpty()
                                                orderby ad.Sort ascending
                                                select new AdExtend
                                                {
                                                    Header = ad,
                                                    FileInfo = FileInfo,
                                                }).ToList();
                return allDataList;
            }
            catch (Exception ex)
            {
                ErrMsg = ex.ToString();
                return null;
            }
        }

    }
}
