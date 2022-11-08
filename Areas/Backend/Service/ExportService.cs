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
    public class ExportService : ServiceBase
    {
        private string _Message = string.Empty;

        public ExportService(DBContext context) : base(context)
        {
        }

        /// <summary>
        /// 全資料列
        /// </summary>
        /// <param name="dataList"> list=列, list<string>=欄 </param>
        /// <param name="HasHeader">是否含標題列</param>
        /// <param name="LastColumnIsNumber">最後一個 column 轉型為數字</param>
        /// <returns></returns>
        public ActionResultModel<MemoryStream> ExportExcel(List<List<string>> dataList, bool HasHeader = true, bool LastColumnIsNumber = false)
        {
            ActionResultModel<MemoryStream> result = new ActionResultModel<MemoryStream>();
            try
            {
                //建立Excel
                IWorkbook workbook = new XSSFWorkbook(); //建立活頁簿
                MemoryStream ms = new MemoryStream();

                ISheet sheet = workbook.CreateSheet("sheet"); //建立sheet

                //設定樣式
                ICellStyle headerStyle = workbook.CreateCellStyle();
                IFont headerfont = workbook.CreateFont();
                headerStyle.Alignment = HorizontalAlignment.Center; //水平置中
                headerStyle.VerticalAlignment = VerticalAlignment.Center; //垂直置中
                headerfont.FontName = "微軟正黑體";
                headerfont.FontHeightInPoints = 20;
                headerfont.Boldweight = (short)FontBoldWeight.Bold;
                headerStyle.SetFont(headerfont);
                headerStyle.WrapText = true;


                //填入資料
                for (int row_index = 0; row_index < dataList.Count; row_index++)
                {
                    //-- 建立欄位
                    sheet.CreateRow(row_index);

                    for (int col_index = 0; col_index < dataList[row_index].Count; col_index++)
                    {
                        if ((!HasHeader || row_index > 0) && (LastColumnIsNumber && col_index == dataList[row_index].Count - 1))
                        {
                            double.TryParse(dataList[row_index][col_index], out double value);
                            sheet.GetRow(row_index).CreateCell(col_index).SetCellValue(value);
                        }
                        else
                        {
                            sheet.GetRow(row_index).CreateCell(col_index).SetCellValue(dataList[row_index][col_index]);
                        }
                    }
                }

                workbook.Write(ms);

                ms.Close();
                ms.Dispose();

                result.Data = ms;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.IsSuccess = false;
            }

            return result;
        }

    }
}
