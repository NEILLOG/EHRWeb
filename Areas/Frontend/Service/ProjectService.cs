using BASE.Areas.Frontend.Models;
using BASE.Areas.Frontend.Models.Extend;
using BASE.Models.DB;
using BASE.Models.Enums;
using BASE.Service;
using BASE.Service.Base;
using Microsoft.EntityFrameworkCore;

namespace BASE.Areas.Frontend.Service
{
    public class ProjectService : ServiceBase
    {
        private readonly IConfiguration _conf;
        private readonly AllCommonService _allCommonService;
        public ProjectService(DBContext context,
            AllCommonService allCommonService,
            IConfiguration configuration) : base(context)
        {
            _conf = configuration;
            _allCommonService = allCommonService;
        }

        public IQueryable<TbProject>? GetList(ref String ErrMsg, VM_ProjectQueryParam? vmParam)
        {
            try
            {
                IQueryable<TbProject>? dataList = (from ProejctInfo in _context.TbProject.Where(x => !x.IsDelete)
                                                    orderby ProejctInfo.CreateDate descending
                                                    select ProejctInfo
                                                    );

                if (!String.IsNullOrEmpty(vmParam.Category))
                    switch (vmParam.Category)
                    {
                        case "企業訓練資源":
                        case "就業服務資源":
                        case "紓困資源":
                        case "其他資源":
                            dataList = dataList.Where(x => x.Category == vmParam.Category);
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

        /// <summary>取得內頁</summary>
        public TbProject GetItem(ref String ErrMsg, string id)
        {
            try
            {
                IQueryable<TbProject>? allDataList = (from ProejctInfo in _context.TbProject.Where(x => !x.IsDelete)
                                                   orderby ProejctInfo.CreateDate descending
                                                   select ProejctInfo
                                                    );
                // 找出該筆資料
                TbProject data = allDataList.Where(x => x.Id == id).FirstOrDefault();

                return data;
            }
            catch (Exception ex)
            {
                ErrMsg = ex.ToString();
                return null;
            }
        }

    }
}
