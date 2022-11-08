using BASE.Areas.Frontend.Models;
using BASE.Areas.Frontend.Models.Extend;
using BASE.Models.DB;
using BASE.Models.Enums;
using BASE.Service;
using BASE.Service.Base;
using Microsoft.EntityFrameworkCore;

namespace BASE.Areas.Frontend.Service
{
    public class AlbumService : ServiceBase
    {
        private readonly IConfiguration _conf;
        private readonly AllCommonService _allCommonService;
        public AlbumService(DBContext context,
            AllCommonService allCommonService,
            IConfiguration configuration) : base(context)
        {
            _conf = configuration;
            _allCommonService = allCommonService;
        }

        public IQueryable<AlbumExtend>? GetList(ref String ErrMsg, VM_AlbumQueryParam? vmParam)
        {
            DateTime dtnow = DateTime.Now;
            try
            {
                IQueryable<AlbumExtend>? dataList = (from AlbumInfo in _context.TbAlbum.Where(x => !x.IsDelete)
                                                    join FileInfo in _context.TbFileInfo.Where(x => !x.IsDelete) on AlbumInfo.FileId equals FileInfo.FileId into News_File
                                                    from FileInfo in News_File.DefaultIfEmpty()
                                                    orderby AlbumInfo.DisplayDate descending
                                                    select new AlbumExtend
                                                    {
                                                        Header = AlbumInfo,
                                                        FileInfo = FileInfo,
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
