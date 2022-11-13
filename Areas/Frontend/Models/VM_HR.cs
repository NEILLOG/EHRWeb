using BASE.Areas.Frontend.Models.Extend;
using BASE.Models.DB;
using BASE.Models.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BASE.Areas.Frontend.Models
{
    public class VM_HR
    {
        /// <summary>列表_查詢參數</summary>
        public VM_HRQueryParam Search { get; set; } = new VM_HRQueryParam();
        /// <summary>列表</summary>
        public List<HRExtend>? ExtendList { get; set; }
        /// <summary>項目</summary>
        public HRExtend? ExtendItem { get; set; }



    }

    /// <summary>列表_查詢參數</summary>
    public class VM_HRQueryParam : VM_BaseQueryParam
    {
        public VM_HRQueryParam()
        {
            PagerInfo.m_iPageCount = 9;
        }

        /// <summary>
        /// 關鍵字        
        /// </summary>
        public string? Keyword { get; set; }

        public string? Category { get; set; }
    }

}
