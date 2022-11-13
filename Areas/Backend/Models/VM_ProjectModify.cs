using BASE.Areas.Backend.Models.Base;
using BASE.Areas.Backend.Models.Extend;
using BASE.Models.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BASE.Areas.Backend.Models
{
    public class VM_ProjectModify
    {
        /// <summary>FAQ_查詢參數</summary>
        public VM_ProjectModifyQueryParam Search { get; set; } = new VM_ProjectModifyQueryParam();

        /// <summary>ProjectModify列表</summary>
        public List<ProjectModifyExtend>? ProjectModifyExtendList { get; set; }
        /// <summary>ProjectModify項目</summary>
        public ProjectModifyExtend? ProjectModifyExtendItem { get; set; }

        public string? IsApprove { get; set; }

        /// <summary> 類型 </summary>
        public List<SelectListItem> ddlApprove = new List<SelectListItem>() { new SelectListItem() { Text = "請選擇", Value = "" },
                                                                               new SelectListItem() { Text = "同意", Value = "同意" },
                                                                               new SelectListItem() { Text = "不同意", Value = "不同意" } };
    }

    /// <summary>FAQ_查詢參數</summary>
    public class VM_ProjectModifyQueryParam : VM_BaseQueryParam
    {
        /// <summary>關鍵字</summary>
        public string? Keyword { get; set; }
    }
}
