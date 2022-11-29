using BASE.Areas.Backend.Models.Base;
using BASE.Areas.Backend.Models.Extend;
using BASE.Models.DB;
using BASE.Models.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BASE.Areas.Backend.Models
{
    public class VM_ContactUs
    {
        /// <summary>ContactUs_查詢參數</summary>
        public VM_AlbumQueryParam Search { get; set; } = new VM_AlbumQueryParam();

        /// <summary>ContactUs列表</summary>
        public List<TbContactUs>? ContactUsList { get; set; }
        /// <summary>ContactUs項目</summary>
    }

    /// <summary>ContactUs_查詢參數</summary>
    public class VM_ContactUsQueryParam : VM_BaseQueryParam
    {
        /// <summary>關鍵字</summary>
        public string? Keyword { get; set; }
    }
}
