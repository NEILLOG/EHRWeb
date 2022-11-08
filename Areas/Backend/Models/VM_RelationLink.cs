using BASE.Areas.Backend.Models.Base;
using BASE.Areas.Backend.Models.Extend;
using BASE.Models.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BASE.Areas.Backend.Models
{
    public class VM_RelationLink
    {
        /// <summary>RelationLink_查詢參數</summary>
        public VM_RelationLinkQueryParam Search { get; set; } = new VM_RelationLinkQueryParam();

        /// <summary>Advertise列表</summary>
        public List<RelationLinkExtend>? RelationLinkExtendList { get; set; }
        /// <summary>Advertise項目</summary>
        public RelationLinkExtend? RelationLinkExtendItem { get; set; }

        /// <summary>排序ID List</summary>
        public List<string>? SortList { get; set; }

        /// <summary> 上下架狀態 </summary>
        public List<SelectListItem> ddlPublish = new List<SelectListItem>() { new SelectListItem() { Text = "上架", Value = "上架" }, 
                                                                              new SelectListItem() { Text = "下架", Value = "下架" } };

        /// <summary>輪播圖片</summary>
        public IFormFile? RelationLinkImageFile { get; set; }

    }

    /// <summary>FAQ_查詢參數</summary>
    public class VM_RelationLinkQueryParam : VM_BaseQueryParam
    {
        /// <summary>關鍵字</summary>
        public string? Keyword { get; set; }

        public string? sPublish { get; set; }
    }
}
