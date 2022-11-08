using BASE.Areas.Frontend.Models.Extend;
using BASE.Models.DB;
using BASE.Models.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BASE.Areas.Frontend.Models
{
    public class VM_Activity
    {
        /// <summary>列表_查詢參數</summary>
        public VM_ActivityQueryParam Search { get; set; } = new VM_ActivityQueryParam();
        /// <summary>列表</summary>
        public List<ActivityExtend>? ActivityExtendList { get; set; }
        /// <summary>項目</summary>
        public ActivityExtend? ActivityExtendItem { get; set; }

    }

    /// <summary>列表_查詢參數</summary>
    public class VM_ActivityQueryParam : VM_BaseQueryParam
    {
        public VM_ActivityQueryParam()
        {
            PagerInfo.m_iPageCount = 6;
        }

        /// <summary>
        /// 關鍵字        
        /// </summary>
        public string? Keyword { get; set; }

        /// <summary>
        /// 活動類型
        /// </summary>
        public string? Category { get; set; }
    }

}
