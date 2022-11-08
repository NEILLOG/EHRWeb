using BASE.Areas.Backend.Models.Base;
using BASE.Areas.Backend.Models.Extend;
using BASE.Models.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BASE.Areas.Backend.Models
{
    public class VM_Album
    {
        /// <summary>FAQ_查詢參數</summary>
        public VM_AlbumQueryParam Search { get; set; } = new VM_AlbumQueryParam();

        /// <summary>Album列表</summary>
        public List<AlbumExtend>? AlbumExtendList { get; set; }
        /// <summary>Album項目</summary>
        public AlbumExtend? AlbumExtendItem { get; set; }

        /// <summary>排序ID List</summary>
        public List<string>? SortList { get; set; }

        /// <summary> 上下架狀態 </summary>
        public List<SelectListItem> ddlPublish = new List<SelectListItem>() { new SelectListItem() { Text = "上架", Value = "上架" }, 
                                                                              new SelectListItem() { Text = "下架", Value = "下架" } };

        /// <summary>輪播圖片</summary>
        public IFormFile? AlbumImageFile { get; set; }

    }

    /// <summary>FAQ_查詢參數</summary>
    public class VM_AlbumQueryParam : VM_BaseQueryParam
    {
        /// <summary>關鍵字</summary>
        public string? Keyword { get; set; }

        public string? sPublish { get; set; }
    }
}
