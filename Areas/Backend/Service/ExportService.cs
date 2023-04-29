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
using NPOI.XWPF.UserModel;
using System.Reflection;
using NPOI.OpenXmlFormats.Wordprocessing;
using NPOI.SS.Formula;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Text;
using NPOI.WP.UserModel;
using NPOI.SS.Util;
using ICell = NPOI.SS.UserModel.ICell;
using NPOI.SS.Formula.Functions;
using System.IO;
using System.IO.Pipes;

namespace BASE.Areas.Backend.Service
{
    /// <summary>
    /// BY專案的共用功能
    /// </summary>
    public class ExportService : ServiceBase
    {
        private string _Message = string.Empty;
        private readonly FileService _fileService;


        public ExportService(DBContext context,
                             FileService fileService) : base(context)
        {
            _fileService = fileService;
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

        /// <summary>
        /// 帳號管理
        /// </summary>
        /// <param name="dataList"></param>
        /// <param name="columnList"></param>
        /// <returns></returns>
        public ActionResultModel<MemoryStream> AccountExcel(IQueryable<MemberExtend> dataList)
        {
            ActionResultModel<MemoryStream> result = new ActionResultModel<MemoryStream>();
            try
            {
                // 取資料
                List<MemberExportExtend> Data = (from data in dataList
                                                  select new MemberExportExtend
                                                  {
                                                      UserName = data.userinfo.UserName,
                                                      GroupName = data.groupInfo.GroupName,
                                                      UserInfo = data.userinfo.Account,
                                                      Status = data.userinfo.IsActive ? ActiveStatus.True.GetDisplayName() : ActiveStatus.False.GetDisplayName()
                                                  }).ToList();

                //建立Excel
                MemoryStream ms = new MemoryStream();
                //DataTable dt = new DataTable();
                IWorkbook workbook = new XSSFWorkbook(); //建立活頁簿
                ISheet sheet = workbook.CreateSheet("sheet"); //建立sheet

                //標題
                sheet.CreateRow(0); //需先用CreateRow建立,才可通过GetRow取得該欄位
                sheet.GetRow(0).CreateCell(0).SetCellValue("序號");
                sheet.GetRow(0).CreateCell(1).SetCellValue("姓名");
                sheet.GetRow(0).CreateCell(2).SetCellValue("群組");
                sheet.GetRow(0).CreateCell(3).SetCellValue("帳號");
                sheet.GetRow(0).CreateCell(4).SetCellValue("啟用");

                //資料
                int rowIndex = 1;
                foreach (MemberExportExtend item in Data)
                {
                    sheet.CreateRow(rowIndex);
                    sheet.GetRow(rowIndex).CreateCell(0).SetCellValue(rowIndex);
                    sheet.GetRow(rowIndex).CreateCell(1).SetCellValue(item.GetType().GetProperty("UserName")?.GetValue(item, null)?.ToString() ?? string.Empty);
                    sheet.GetRow(rowIndex).CreateCell(2).SetCellValue(item.GetType().GetProperty("GroupName")?.GetValue(item, null)?.ToString() ?? string.Empty);
                    sheet.GetRow(rowIndex).CreateCell(3).SetCellValue(item.GetType().GetProperty("UserInfo")?.GetValue(item, null)?.ToString() ?? string.Empty);
                    sheet.GetRow(rowIndex).CreateCell(4).SetCellValue(item.GetType().GetProperty("Status")?.GetValue(item, null)?.ToString() ?? string.Empty);
                    rowIndex++;
                }

                //寫入MemoryStream
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

        /// <summary>
        /// 活動訊息報名名單
        /// </summary>
        /// <returns></returns>
        public ActionResultModel<MemoryStream> EventInfoRegistrationExcel(IQueryable<RegistrationExtend> dataList)
        {
            ActionResultModel<MemoryStream> result = new ActionResultModel<MemoryStream>();
            try
            {
                // 取資料
                List<EventInfoExportExtend> Data = (from data in dataList
                                                    select new EventInfoExportExtend
                                                    {
                                                        CompanyName = data.register.CompanyName,
                                                        CompanyLocation = data.register.CompanyLocation,
                                                        CompanyType = data.register.CompanyType,
                                                        Name = data.register.Name,
                                                        JobTitle = data.register.JobTitle,
                                                        Phone = data.register.Phone,
                                                        CellPhone = data.register.CellPhone,
                                                        Email = data.register.Email,
                                                        CompanyEmpAmount = data.register.CompanyEmpAmount,
                                                        RegisterSectionType = data.registerSection.RegisterSectionType,
                                                        IsVegin = data.registerSection.RegisterSectionType == "實體" ? (data.registerSection.IsVegin ? "葷" : "素") : "",
                                                        IsValid = data.registerSection.IsValid.HasValue ? (data.registerSection.IsValid.Value ? "通過" : "不通過") : "尚未審核"
                                                    }).ToList();

                //建立Excel
                MemoryStream ms = new MemoryStream();
                //DataTable dt = new DataTable();
                IWorkbook workbook = new XSSFWorkbook(); //建立活頁簿
                ISheet sheet = workbook.CreateSheet("sheet"); //建立sheet

                //標題
                sheet.CreateRow(0); //需先用CreateRow建立,才可通过GetRow取得該欄位
                sheet.GetRow(0).CreateCell(0).SetCellValue("序號");
                sheet.GetRow(0).CreateCell(1).SetCellValue("企業名稱");
                sheet.GetRow(0).CreateCell(2).SetCellValue("企業所在地");
                sheet.GetRow(0).CreateCell(3).SetCellValue("產業別");
                sheet.GetRow(0).CreateCell(4).SetCellValue("姓名");
                sheet.GetRow(0).CreateCell(5).SetCellValue("職稱");
                sheet.GetRow(0).CreateCell(6).SetCellValue("連絡電話");
                sheet.GetRow(0).CreateCell(7).SetCellValue("手機");
                sheet.GetRow(0).CreateCell(8).SetCellValue("電子郵件");
                sheet.GetRow(0).CreateCell(9).SetCellValue("公司人數");
                sheet.GetRow(0).CreateCell(10).SetCellValue("課程參與模式");
                sheet.GetRow(0).CreateCell(11).SetCellValue("飲食選擇");
                sheet.GetRow(0).CreateCell(12).SetCellValue("審核狀態");

                //資料
                int rowIndex = 1;
                foreach (EventInfoExportExtend item in Data)
                {
                    sheet.CreateRow(rowIndex);
                    sheet.GetRow(rowIndex).CreateCell(0).SetCellValue(rowIndex);
                    sheet.GetRow(rowIndex).CreateCell(1).SetCellValue(item.GetType().GetProperty("CompanyName")?.GetValue(item, null)?.ToString() ?? string.Empty);
                    sheet.GetRow(rowIndex).CreateCell(2).SetCellValue(item.GetType().GetProperty("CompanyLocation")?.GetValue(item, null)?.ToString() ?? string.Empty);
                    sheet.GetRow(rowIndex).CreateCell(3).SetCellValue(item.GetType().GetProperty("CompanyType")?.GetValue(item, null)?.ToString() ?? string.Empty);
                    sheet.GetRow(rowIndex).CreateCell(4).SetCellValue(item.GetType().GetProperty("Name")?.GetValue(item, null)?.ToString() ?? string.Empty);
                    sheet.GetRow(rowIndex).CreateCell(5).SetCellValue(item.GetType().GetProperty("JobTitle")?.GetValue(item, null)?.ToString() ?? string.Empty);
                    sheet.GetRow(rowIndex).CreateCell(6).SetCellValue(item.GetType().GetProperty("Phone")?.GetValue(item, null)?.ToString() ?? string.Empty);
                    sheet.GetRow(rowIndex).CreateCell(7).SetCellValue(item.GetType().GetProperty("CellPhone")?.GetValue(item, null)?.ToString() ?? string.Empty);
                    sheet.GetRow(rowIndex).CreateCell(8).SetCellValue(item.GetType().GetProperty("Email")?.GetValue(item, null)?.ToString() ?? string.Empty);
                    sheet.GetRow(rowIndex).CreateCell(9).SetCellValue(item.GetType().GetProperty("CompanyEmpAmount")?.GetValue(item, null)?.ToString() ?? string.Empty);
                    sheet.GetRow(rowIndex).CreateCell(10).SetCellValue(item.GetType().GetProperty("RegisterSectionType")?.GetValue(item, null)?.ToString() ?? string.Empty);
                    sheet.GetRow(rowIndex).CreateCell(11).SetCellValue(item.GetType().GetProperty("IsVegin")?.GetValue(item, null)?.ToString() ?? string.Empty);
                    sheet.GetRow(rowIndex).CreateCell(12).SetCellValue(item.GetType().GetProperty("IsValid")?.GetValue(item, null)?.ToString() ?? string.Empty);
                    rowIndex++;
                }

                //寫入MemoryStream
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

        /// <summary>
        /// 諮詢輔導報名名單
        /// </summary>
        /// <returns></returns>
        public ActionResultModel<MemoryStream> ConsultRegistrationExcel(IQueryable<ConsultExtend> dataList)
        {
            ActionResultModel<MemoryStream> result = new ActionResultModel<MemoryStream>();
            try
            {
                // 取資料
                List<ConsultExportExtned> Data = (from data in dataList
                                                  select new ConsultExportExtned
                                                  {
                                                      Name = data.ConsultRegister.Name,
                                                      ConsultSubject = data.textOfSubject,
                                                      Adviser = data.ConsultantList,
                                                      AdviserAssistant = data.Assistant,
                                                      IsClose = data.ConsultRegister.IsClose ? "已結案" : "未結案"
                                                  }).ToList();

                //建立Excel
                MemoryStream ms = new MemoryStream();
                //DataTable dt = new DataTable();
                IWorkbook workbook = new XSSFWorkbook(); //建立活頁簿
                ISheet sheet = workbook.CreateSheet("sheet"); //建立sheet

                //標題
                sheet.CreateRow(0); //需先用CreateRow建立,才可通过GetRow取得該欄位
                sheet.GetRow(0).CreateCell(0).SetCellValue("序號");
                sheet.GetRow(0).CreateCell(1).SetCellValue("企業名稱");
                sheet.GetRow(0).CreateCell(2).SetCellValue("諮詢主題");
                sheet.GetRow(0).CreateCell(3).SetCellValue("顧問");
                sheet.GetRow(0).CreateCell(4).SetCellValue("輔導助理");
                sheet.GetRow(0).CreateCell(5).SetCellValue("審核狀態");

                //資料
                int rowIndex = 1;
                foreach (ConsultExportExtned item in Data)
                {
                    sheet.CreateRow(rowIndex);
                    sheet.GetRow(rowIndex).CreateCell(0).SetCellValue(rowIndex);
                    sheet.GetRow(rowIndex).CreateCell(1).SetCellValue(item.GetType().GetProperty("Name")?.GetValue(item, null)?.ToString() ?? string.Empty);
                    sheet.GetRow(rowIndex).CreateCell(2).SetCellValue(item.GetType().GetProperty("ConsultSubject")?.GetValue(item, null)?.ToString() ?? string.Empty);
                    sheet.GetRow(rowIndex).CreateCell(3).SetCellValue(item.GetType().GetProperty("Adviser")?.GetValue(item, null)?.ToString() ?? string.Empty);
                    sheet.GetRow(rowIndex).CreateCell(4).SetCellValue(item.GetType().GetProperty("AdviserAssistant")?.GetValue(item, null)?.ToString() ?? string.Empty);
                    sheet.GetRow(rowIndex).CreateCell(5).SetCellValue(item.GetType().GetProperty("IsClose")?.GetValue(item, null)?.ToString() ?? string.Empty);
                    rowIndex++;
                }

                //寫入MemoryStream
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

        /// <summary>
        /// 諮詢輔導報名簽到表
        /// </summary>
        /// <returns></returns>
        public ActionResultModel<MemoryStream> ConsultSigninExcel(CounselingSigninExtend dataItem)
        {
            ActionResultModel<MemoryStream> result = new ActionResultModel<MemoryStream>();
            try
            {
                //建立Excel
                MemoryStream ms = new MemoryStream();
                //DataTable dt = new DataTable();
                IWorkbook workbook = new XSSFWorkbook(); //建立活頁簿
                ISheet sheet = workbook.CreateSheet(dataItem.sheetName); //建立sheet
                sheet.PrintSetup.PaperSize = (short)PaperSize.A4_Small;
                sheet.PrintSetup.Landscape = true;
                sheet.PrintSetup.FitWidth = 1;                           //所有列在一页  
                sheet.PrintSetup.FitHeight = 0;                          //所有行在一页

                // 設定標題的格式
                ICellStyle cellStyle = workbook.CreateCellStyle();//聲明樣式
                cellStyle.Alignment = HorizontalAlignment.Center;//水平居中
                cellStyle.VerticalAlignment = VerticalAlignment.Center;//垂直居中
                IFont font = workbook.CreateFont();//聲明字體
                font.Boldweight = (Int16)FontBoldWeight.Bold;//加粗
                font.FontHeightInPoints = 30;//字體大小
                font.FontName = "標楷體";
                cellStyle.SetFont(font);//加入單元格

                IRow row0 = sheet.CreateRow(0);//創建行
                row0.HeightInPoints = 35;//行高
                ICell cell0 = row0.CreateCell(0);//創建單元格
                cell0.SetCellValue("勞動部勞動力發展署桃竹苗分署");//賦值
                cell0.CellStyle = cellStyle;//設置樣式
                sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, 3));//合併單元格（第幾行，到第幾行，第幾列，到第幾列）

                IRow row1 = sheet.CreateRow(1);//創建行
                row1.HeightInPoints = 35;//行高
                ICell cell1 = row1.CreateCell(0);//創建單元格
                cell1.SetCellValue("111年度桃竹苗區域運籌人力資源整合服務計畫");//賦值
                cell1.CellStyle = cellStyle;//設置樣式
                sheet.AddMergedRegion(new CellRangeAddress(1, 1, 0, 3));//合併單元格（第幾行，到第幾行，第幾列，到第幾列）

                IRow row2 = sheet.CreateRow(2);//創建行
                row2.HeightInPoints = 35;//行高
                ICell cell2 = row2.CreateCell(0);//創建單元格
                cell2.SetCellValue("諮詢輔導服務-輔導簽到表\r\n");//賦值
                cell2.CellStyle = cellStyle;//設置樣式
                sheet.AddMergedRegion(new CellRangeAddress(2, 2, 0, 3));//合併單元格（第幾行，到第幾行，第幾列，到第幾列）

                //-- 輔導時間與地點
                ICellStyle cellStyle2 = workbook.CreateCellStyle();//聲明樣式
                IFont font2 = workbook.CreateFont();//聲明字體
                font2.FontHeightInPoints = 20;//字體大小
                font2.FontName = "標楷體";
                cellStyle2.SetFont(font2);//加入單元格

                IRow row3 = sheet.CreateRow(3);//創建行
                row3.HeightInPoints = 30;//行高
                ICell cell3 = row3.CreateCell(0);//創建單元格
                cell3.SetCellValue("輔導時間：" + dataItem.CounselingTime);//賦值
                cell3.CellStyle = cellStyle2;//設置樣式
                sheet.AddMergedRegion(new CellRangeAddress(3, 3, 0, 3));//合併單元格（第幾行，到第幾行，第幾列，到第幾列）

                IRow row4 = sheet.CreateRow(4);//創建行
                row4.HeightInPoints = 30;//行高
                ICell cell4 = row4.CreateCell(0);//創建單元格
                cell4.SetCellValue("輔導地點：" + dataItem.CounselingPlace);//賦值
                cell4.CellStyle = cellStyle2;//設置樣式
                sheet.AddMergedRegion(new CellRangeAddress(4, 4, 0, 3));//合併單元格（第幾行，到第幾行，第幾列，到第幾列）

                // 空白行
                IRow row5 = sheet.CreateRow(5);//創建行
                row5.HeightInPoints = 10;//行高
                ICell cell5 = row5.CreateCell(0);//創建單元格
                //cell5.SetCellValue("輔導地點：" + dataItem.CounselingPlace);//賦值
                cell5.CellStyle = cellStyle;//設置樣式
                sheet.AddMergedRegion(new CellRangeAddress(5, 5, 0, 3));//合併單元格（第幾行，到第幾行，第幾列，到第幾列）

                // 資料列表標頭
                IRow row6 = sheet.CreateRow(6);
                row6.HeightInPoints = 30;//行高
                ICell cell6 = row6.CreateCell(0);//創建單元格
                cell6.CellStyle = cellStyle2;//設置樣式
                sheet.GetRow(6).CreateCell(0).SetCellValue("單位");
                sheet.GetRow(6).CreateCell(1).SetCellValue("姓名");
                sheet.GetRow(6).CreateCell(2).SetCellValue("職稱");
                sheet.GetRow(6).CreateCell(3).SetCellValue("簽到");

                //資料
                int rowIndex = 7;
                foreach (CounselingObjectExtend item in dataItem.listObject)
                {
                    IRow rowX = sheet.CreateRow(rowIndex);
                    rowX.HeightInPoints = 30;//行高
                    ICell cellX = rowX.CreateCell(0);//創建單元格
                    cellX.CellStyle = cellStyle2;//設置樣式

                    sheet.GetRow(rowIndex).CreateCell(0).SetCellValue(item.Unit);
                    sheet.GetRow(rowIndex).CreateCell(1).SetCellValue(item.Name);
                    sheet.GetRow(rowIndex).CreateCell(2).SetCellValue(item.JobTitle);
                    sheet.GetRow(rowIndex).CreateCell(3).SetCellValue("");
                    rowIndex++;
                }

                // 自動調整欄位
                sheet.SetColumnWidth(0, 32 * 256);
                sheet.SetColumnWidth(1, 32 * 256);
                sheet.SetColumnWidth(2, 32 * 256);
                sheet.SetColumnWidth(4, 32 * 256);

                //sheet.AutoSizeColumn(0);
                //sheet.AutoSizeColumn(1);
                //sheet.AutoSizeColumn(2);
                //sheet.AutoSizeColumn(3);

                //寫入MemoryStream
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

        /// <summary>
        /// 帳號管理
        /// </summary>
        /// <param name="dataList"></param>
        /// <param name="columnList"></param>
        /// <returns></returns>
        public ActionResultModel<MemoryStream> ContactUsExcel(IQueryable<ContactUsExtend> dataList)
        {
            ActionResultModel<MemoryStream> result = new ActionResultModel<MemoryStream>();
            try
            {
                // 取資料
                List<ContactUsExportExtend> Data = (from data in dataList
                                                    select new ContactUsExportExtend
                                                    {
                                                        Name = data.ContactUs.Name,
                                                        Email = data.ContactUs.Email,
                                                        Response = data.ContactUs.Response,
                                                        CreateDate = data.ContactUs.CreateDate
                                                    }).ToList();

                //建立Excel
                MemoryStream ms = new MemoryStream();
                //DataTable dt = new DataTable();
                IWorkbook workbook = new XSSFWorkbook(); //建立活頁簿
                ISheet sheet = workbook.CreateSheet("sheet"); //建立sheet

                //標題
                sheet.CreateRow(0); //需先用CreateRow建立,才可通过GetRow取得該欄位
                sheet.GetRow(0).CreateCell(0).SetCellValue("序號");
                sheet.GetRow(0).CreateCell(1).SetCellValue("姓名");
                sheet.GetRow(0).CreateCell(2).SetCellValue("信箱");
                sheet.GetRow(0).CreateCell(3).SetCellValue("建議內容");
                sheet.GetRow(0).CreateCell(4).SetCellValue("留言時間");

                //資料
                int rowIndex = 1;
                foreach (ContactUsExportExtend item in Data)
                {
                    sheet.CreateRow(rowIndex);
                    sheet.GetRow(rowIndex).CreateCell(0).SetCellValue(rowIndex);
                    sheet.GetRow(rowIndex).CreateCell(1).SetCellValue(item.GetType().GetProperty("Name")?.GetValue(item, null)?.ToString() ?? string.Empty);
                    sheet.GetRow(rowIndex).CreateCell(2).SetCellValue(item.GetType().GetProperty("Email")?.GetValue(item, null)?.ToString() ?? string.Empty);
                    sheet.GetRow(rowIndex).CreateCell(3).SetCellValue(item.GetType().GetProperty("Response")?.GetValue(item, null)?.ToString() ?? string.Empty);
                    sheet.GetRow(rowIndex).CreateCell(4).SetCellValue(item.GetType().GetProperty("CreateDate")?.GetValue(item, null)?.ToString() ?? string.Empty);
                    rowIndex++;
                }

                //寫入MemoryStream
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


        /// <summary>
        /// 功能權限申請表(Word)
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public ActionResultModel<MemoryStream> EventSigninWord(RegistrationExportExtend item)
        {
            ActionResultModel<MemoryStream> result = new ActionResultModel<MemoryStream>();

            try
            {
                string filePath = _fileService.MapPath("Sample/ActivitySignin.docx");
                using (FileStream stream = File.OpenRead(filePath))
                {
                    XWPFDocument doc = new XWPFDocument(stream);

                    //段落
                    foreach (var para in doc.Paragraphs)
                    {
                        ReplaceKeyObjet(para, item);
                    }

                    //表格
                    var tables = doc.Tables;
                    XWPFTable oprTable = tables[0];
                    for (int i = 0; i < item.listData.Count(); i++)
                    {
                        XWPFTableRow m_Row = oprTable.CreateRow();
                        m_Row.Height = 100;
                        m_Row.GetCell(0).SetText((i + 1).ToString());
                        m_Row.GetCell(1).SetText(item.listData[i].register.CompanyName);
                        m_Row.GetCell(2).SetText(item.listData[i].register.Name);
                        m_Row.GetCell(3).SetText(item.listData[i].register.JobTitle);
                        m_Row.GetCell(4).SetText("");
                        m_Row.GetCell(5).SetText("");

                        XWPFParagraph p0 = m_Row.GetCell(0).AddParagraph();
                        XWPFRun r0 = p0.CreateRun();
                        r0.FontFamily = "Microsoft JhengHei";

                        XWPFParagraph p1 = m_Row.GetCell(1).AddParagraph();
                        XWPFRun r1 = p1.CreateRun();
                        r1.FontFamily = "Microsoft JhengHei";

                        XWPFParagraph p2 = m_Row.GetCell(2).AddParagraph();
                        XWPFRun r2 = p2.CreateRun();
                        r2.FontFamily = "Microsoft JhengHei";

                        XWPFParagraph p3 = m_Row.GetCell(3).AddParagraph();
                        XWPFRun r3 = p3.CreateRun();
                        r3.FontFamily = "Microsoft JhengHei";
                    }

                    // 將word寫入MemoryStream
                    MemoryStream ms = new MemoryStream();
                    doc.Write(ms);
                    ms.Close();
                    ms.Dispose();

                    result.Data = ms;
                }

            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.IsSuccess = false;
            }
            return result;
        }

        /// <summary>
        /// 出席學員證明(Word)
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public ActionResultModel<MemoryStream> ProofWord(ProofExportExtend item,string filePath)
        {
            ActionResultModel<MemoryStream> result = new ActionResultModel<MemoryStream>();

            try
            {
                string sampleFilePath = _fileService.MapPath("Sample/Sample_Proof.docx");
                using (FileStream stream = File.OpenRead(sampleFilePath))
                {
                    XWPFDocument doc = new XWPFDocument(stream);

                    //段落
                    foreach (var para in doc.Paragraphs)
                    {
                        ReplaceKeyObjet(para, item);
                    }

                    FileStream Fs = new FileStream(filePath, FileMode.OpenOrCreate);
                    doc.Write(Fs);
                    Fs.Close();
                }
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.IsSuccess = false;
            }
            return result;
        }

        private static void ReplaceKeyObjet(XWPFParagraph para, object model)
        {
            Type t = model.GetType();
            PropertyInfo[] pi = t.GetProperties();
            foreach (PropertyInfo p in pi)
            {
                string key = $"$[{p.Name}]";
                if (para.Text.Contains(key))
                {
                    try
                    {
                        para.ReplaceText(key, p.GetValue(model, null)?.ToString());
                    }
                    catch (Exception ex)
                    {
                        para.ReplaceText(key, "");
                    }
                }
            }
        }
    }
}
