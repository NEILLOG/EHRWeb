using BASE.Areas.Frontend.Models;
using BASE.Areas.Frontend.Models.Extend;
using BASE.Models.DB;
using BASE.Models.Enums;
using BASE.Service;
using BASE.Service.Base;
using Microsoft.EntityFrameworkCore;

namespace BASE.Areas.Frontend.Service
{
    public class HRService : ServiceBase
    {
        private readonly IConfiguration _conf;
        private readonly AllCommonService _allCommonService;
        public HRService(DBContext context,
            AllCommonService allCommonService,
            IConfiguration configuration) : base(context)
        {
            _conf = configuration;
            _allCommonService = allCommonService;
        }

        public IQueryable<HRExtend>? GetList(ref String ErrMsg, VM_HRQueryParam? vmParam)
        {
            try
            {
                IQueryable<HRExtend>? dataList = (from HRInfo in _context.TbHrArticle.Where(x => !x.IsDelete && x.IsPublish)
                                                  join FileInfo in _context.TbFileInfo.Where(x => !x.IsDelete) on HRInfo.FileId equals FileInfo.FileId into HR_File
                                                  from FileInfo in HR_File.DefaultIfEmpty()
                                                  orderby HRInfo.CreateDate descending
                                                  select new HRExtend
                                                  {
                                                      Header = HRInfo,
                                                      FileInfo = FileInfo,
                                                  });

                if (!String.IsNullOrEmpty(vmParam.Category))
                    switch (vmParam.Category)
                    {
                        case "成功案例分享":
                        case "HR知識充電站":
                            dataList = dataList.Where(x => x.Header.Category == vmParam.Category);
                            break;
                    }

                return dataList;
            }
            catch (Exception ex)
            {
                ErrMsg = ex.ToString();
                return null;
            }
        }

        public HRExtend GetExtendItem(ref String ErrMsg, long id)
        {
            DateTime dtNow = DateTime.Now;
            try
            {
                IQueryable<HRExtend>? allDataList = (from HRInfo in _context.TbHrArticle.Where(x => !x.IsDelete && x.IsPublish)
                                                  join FileInfo in _context.TbFileInfo.Where(x => !x.IsDelete) on HRInfo.FileId equals FileInfo.FileId into HR_File
                                                  from FileInfo in HR_File.DefaultIfEmpty()
                                                  orderby HRInfo.CreateDate descending
                                                  select new HRExtend
                                                  {
                                                      Header = HRInfo,
                                                      FileInfo = FileInfo,
                                                  });
                //找出該筆資料
                HRExtend data = allDataList.Where(x => x.Header.Id == id).FirstOrDefault();

                return data;
            }
            catch (Exception ex)
            {
                ErrMsg = ex.ToString();
                return null;
            }
        }


        public IQueryable<HRPackageExtend>? GetPackageList(ref String ErrMsg, VM_HRPackageQueryParam? vmParam)
        {
            try
            {
                IQueryable<HRPackageExtend>? dataList = (from HRPackageInfo in _context.TbHrPackage.Where(x => !x.IsDelete && x.IsPublish)
                                                         join FileInfo in _context.TbFileInfo.Where(x => !x.IsDelete) on HRPackageInfo.FileId equals FileInfo.FileId into HR_File
                                                         from FileInfo in HR_File.DefaultIfEmpty()
                                                         orderby HRPackageInfo.CreateDate descending
                                                         select new HRPackageExtend
                                                         {
                                                             Header = HRPackageInfo,
                                                             FileInfo = FileInfo,
                                                         });

                if (vmParam != null)
                {
                    if (!string.IsNullOrEmpty(vmParam.Keyword))
                    {
                        dataList = dataList.Where(x => x.Header.Title.Contains(vmParam.Keyword));
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
    }
}
