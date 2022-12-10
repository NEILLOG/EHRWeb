using BASE.Areas.Backend.Models;
using BASE.Areas.Backend.Models.Extend;
using BASE.Models.DB;
using BASE.Service.Base;
using BASE.Models.Enums;
using BASE.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using BASE.Service;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using BASE.Models;
using NPOI.XSSF.UserModel;

namespace BASE.Areas.Backend.Service
{
    /// <summary>
    /// BY專案的共用功能
    /// </summary>
    public class ImportService : ServiceBase
    {
        private readonly AllCommonService _allCommonService;
        private readonly IConfiguration _conf;
        private string _Message = string.Empty;

        public ImportService(DBContext context,
                             AllCommonService allCommonService,
                             IConfiguration configuration) : base(context)
        {
            _allCommonService = allCommonService;
            _conf = configuration;
        }
        /// <summary>
        /// 全資料列
        /// </summary>
        /// <param name="dataList"></param>
        /// <param name="HasHeader">是否含標題列</param>
        /// <param name="LastColumnIsNumber">最後一個 column 轉型為數字</param>
        /// <returns></returns>
        public ActionResultModel<IWorkbook> ReadExcel(string extension, IFormFile file)
        {
            ActionResultModel<IWorkbook> result = new ActionResultModel<IWorkbook>();
            try
            {
                MemoryStream ms = new MemoryStream();
                //file.CopyTo(ms);

                //Stream stream = new MemoryStream(ms.ToArray());

                switch (extension)
                {
                    case ".xls":
                        result.Data = new HSSFWorkbook(ms);
                        break;

                    case ".xlsx":
                        result.Data = new XSSFWorkbook(ms);
                        break;

                    default:
                        result.Data = null;
                        result.IsSuccess = false;
                        result.Description = "不支援此副檔名";
                        result.Message = "不支援此副檔名";
                        break;
                }

                return result;
            }
            catch (Exception ex)
            {
                result.Description = "檔案上傳失敗";
                result.Message = ex.ToString();
                result.IsSuccess = false;
            }

            return result;
        }

    }
}
