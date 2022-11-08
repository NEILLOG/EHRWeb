using BASE.Areas.Backend.Models.Base;
using BASE.Areas.Backend.Models.Extend;
using BASE.Models.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BASE.Areas.Backend.Models
{
    public class VM_FAQ
    {
        /// <summary>FAQ_查詢參數</summary>
        public VM_FAQQueryParam Search { get; set; } = new VM_FAQQueryParam();

        /// <summary>FAQ列表</summary>
        public List<FAQExtend>? FAQExtendList { get; set; }
        /// <summary>FAQ項目</summary>
        public FAQExtend? FAQExtendItem { get; set; }

        /// <summary>排序ID List</summary>
        public List<string>? SortList { get; set; }



    }

    /// <summary>FAQ_查詢參數</summary>
    public class VM_FAQQueryParam : VM_BaseQueryParam
    {
        /// <summary>關鍵字</summary>
        public string? Keyword { get; set; }

    }
}
