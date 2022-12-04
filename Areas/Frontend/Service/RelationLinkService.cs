using BASE.Areas.Frontend.Models;
using BASE.Areas.Frontend.Models.Extend;
using BASE.Models.DB;
using BASE.Models.Enums;
using BASE.Service;
using BASE.Service.Base;
using Microsoft.EntityFrameworkCore;

namespace BASE.Areas.Frontend.Service
{
    public class RelationLinkService : ServiceBase
    {
        private readonly IConfiguration _conf;
        private readonly AllCommonService _allCommonService;
        public RelationLinkService(DBContext context,
            AllCommonService allCommonService,
            IConfiguration configuration) : base(context)
        {
            _conf = configuration;
            _allCommonService = allCommonService;
        }

        public List<RelationLinkExtend> GetExtendItemList(ref String ErrMsg)
        {
            try
            {
                List<RelationLinkExtend> allDataList = (from link in _context.TbRelationLink.Where(x => !x.IsDelete && x.IsPublish)
                                                        join FileInfo in _context.TbFileInfo.Where(x => !x.IsDelete) on link.FileId equals FileInfo.FileId into link_File
                                                        from FileInfo in link_File.DefaultIfEmpty()
                                                        orderby link.Sort ascending
                                                        select new RelationLinkExtend
                                                        {
                                                            Header = link,
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
