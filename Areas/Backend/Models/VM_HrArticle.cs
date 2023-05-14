using BASE.Areas.Backend.Models.Base;
using BASE.Areas.Backend.Models.Extend;
using BASE.Models.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BASE.Areas.Backend.Models
{
    public class VM_HrArticle
    {
        /// <summary>FAQ_查詢參數</summary>
        public VM_HrArticleQueryParam Search { get; set; } = new VM_HrArticleQueryParam();

        /// <summary>HrArticle列表</summary>
        public List<HrArticleExtend>? HrArticleExtendList { get; set; }
        /// <summary>HrArticle項目</summary>
        public HrArticleExtend? HrArticleExtendItem { get; set; }

        /// <summary>排序ID List</summary>
        public List<string>? SortList { get; set; }


        /// <summary> 類型 </summary>
        public List<SelectListItem> ddlCategory = new List<SelectListItem>() { new SelectListItem() { Text = "請選擇", Value = "" },
                                                                               new SelectListItem() { Text = "HR知識充電站", Value = "HR知識充電站" },
                                                                               new SelectListItem() { Text = "成功案例分享", Value = "成功案例分享" } };

        /// <summary> 上下架狀態 </summary>
        public List<SelectListItem> ddlPublish = new List<SelectListItem>() { new SelectListItem() { Text = "上架", Value = "上架" }, 
                                                                              new SelectListItem() { Text = "下架", Value = "下架" } };


        /// <summary>相關檔案</summary>
        public IFormFile? RelatedFile { get; set; }

        /// <summary>相關檔案(刪除用)</summary>
        public List<string>? DelFileList { get; set; }
    }

    /// <summary>FAQ_查詢參數</summary>
    public class VM_HrArticleQueryParam : VM_BaseQueryParam
    {
        /// <summary>關鍵字</summary>
        public string? Keyword { get; set; }

        /// <summary> 類型 </summary>
        public string? sCategory { get; set; }

        /// <summary> 類型 </summary>
        public string? sTitle { get; set; }
        
    }
}
