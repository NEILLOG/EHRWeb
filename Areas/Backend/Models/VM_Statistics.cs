using BASE.Areas.Backend.Models.Base;
using BASE.Areas.Backend.Models.Extend;
using BASE.Models.DB;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BASE.Areas.Backend.Models
{
    public class VM_Statistics
    {
       
        
        #region 下拉選單
        /// <summary> 年度 </summary>
        public List<SelectListItem> ddlYear {
            get
            {
                return new List<SelectListItem>()
                {
                    new SelectListItem() { Text = "請選擇", Value = "" },
                    new SelectListItem() { Text = "111", Value = "111" },
                };
            }
        }

        /// <summary> 類型 </summary>
        public List<SelectListItem> ddlType {
            get
            {
                return new List<SelectListItem>()
                {
                    new SelectListItem() { Text = "請選擇", Value = "" },
                    new SelectListItem() { Text = "活動", Value = "1" },
                    new SelectListItem() { Text = "諮詢輔導", Value = "2" }
                };
            }
        }

        /// <summary> 活動名稱 </summary>
        public List<SelectListItem> ddlActivityName { get; set; }

        /// <summary> 篩選條件 </summary>
        public List<SelectListItem> ddlFilter {
            get
            {
                return new List<SelectListItem>()
                {
                    new SelectListItem() { Text = "請選擇", Value = "" },
                    new SelectListItem() { Text = "企業所在地", Value = "1" },
                    new SelectListItem() { Text = "產業別", Value = "2" },
                    new SelectListItem() { Text = "公司員工人數", Value = "3" },
                    new SelectListItem() { Text = "訊息來源地", Value = "4" },
                };
            }
        }       

        /// <summary>活動管理_查詢參數</summary>
        public VM_StatisticsQueryParam Search { get; set; } = new VM_StatisticsQueryParam();

        public ChartInfo ChartInfo { get; set; }

        /// <summary> 總申請家數 </summary>
        public int? TotalCount { get; set; }
        #endregion

    }

    public class ChartInfo
    {
        public List<string> ChartData { get; set; }
        public List<int> ChartValue { get; set; }
    } 

    public class VM_StatisticsQueryParam : VM_BaseQueryParam
    {
        /// <summary> 類型 </summary>
        public string? Type { get; set; }

        /// <summary>活動年度</summary>
        public string? ActYear { get; set; }

        /// <summary>諮詢輔導年度</summary>
        public string? ConYear { get; set; }

        /// <summary> 活動名稱 </summary>
        public string? ActivityName { get; set; }

        /// <summary> 篩選條件 </summary>
        public string? Filter { get; set; }

        

    }
}
