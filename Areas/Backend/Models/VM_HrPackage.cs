using BASE.Areas.Backend.Models.Base;
using BASE.Areas.Backend.Models.Extend;
using BASE.Models.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BASE.Areas.Backend.Models
{
    public class VM_HrPackage
    {
        /// <summary>FAQ_查詢參數</summary>
        public VM_HrPackageQueryParam Search { get; set; } = new VM_HrPackageQueryParam();

        /// <summary>HrPackage列表</summary>
        public List<HrPackageExtend>? HrPackageExtendList { get; set; }
        /// <summary>HrPackage項目</summary>
        public HrPackageExtend? HrPackageExtendItem { get; set; }

        /// <summary>OnePage項目</summary>
        public OnePageExtend? OnePageExtendItem { get; set; }

        /// <summary>排序ID List</summary>
        public List<string>? SortList { get; set; }


        /// <summary>相關檔案</summary>
        public IFormFile? RelatedFile { get; set; }


    }

    /// <summary>FAQ_查詢參數</summary>
    public class VM_HrPackageQueryParam : VM_BaseQueryParam
    {
        /// <summary>關鍵字</summary>
        public string? Keyword { get; set; }

        /// <summary> 類型 </summary>
        public string? sTitle { get; set; }
    }
}
