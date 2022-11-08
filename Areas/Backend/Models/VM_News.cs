using BASE.Areas.Backend.Models.Base;
using BASE.Areas.Backend.Models.Extend;
using BASE.Models.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BASE.Areas.Backend.Models
{
    public class VM_News
    {
        /// <summary>FAQ_查詢參數</summary>
        public VM_NewsQueryParam Search { get; set; } = new VM_NewsQueryParam();

        /// <summary>News列表</summary>
        public List<NewsExtend>? NewsExtendList { get; set; }
        /// <summary>News項目</summary>
        public NewsExtend? NewsExtendItem { get; set; }

        /// <summary>排序ID List</summary>
        public List<string>? SortList { get; set; }


        /// <summary> 類型 </summary>
        public List<SelectListItem> ddlCategory = new List<SelectListItem>() { new SelectListItem() { Text = "請選擇", Value = "" },
                                                                               new SelectListItem() { Text = "計畫消息", Value = "計畫消息" },
                                                                               new SelectListItem() { Text = "其他消息", Value = "其他消息" } };

        /// <summary> 上下架狀態 </summary>
        public List<SelectListItem> ddlPublish = new List<SelectListItem>() { new SelectListItem() { Text = "上架", Value = "上架" }, 
                                                                              new SelectListItem() { Text = "下架", Value = "下架" } };


        /// <summary>相關檔案</summary>
        public IFormFile? RelatedFile { get; set; }

        /// <summary> 是否上架 </summary>
        public string? isPublish { get; set; }
    }

    /// <summary>FAQ_查詢參數</summary>
    public class VM_NewsQueryParam : VM_BaseQueryParam
    {
        /// <summary>關鍵字</summary>
        public string? Keyword { get; set; }

        /// <summary> 類型 </summary>
        public string? sCategory { get; set; }

        /// <summary> 類型 </summary>
        public string? sTitle { get; set; }

        /// <summary> 時間區間起 </summary>
        public string? sTime { get; set; }
        /// <summary> 時間區間訖 </summary>
        public string? eTime { get; set; }

        /// <summary> 是否上架 </summary>
        public string? sPublish { get; set; }
        
    }
}
