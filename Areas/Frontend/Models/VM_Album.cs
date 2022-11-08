using BASE.Areas.Frontend.Models.Extend;
using BASE.Models.DB;
using BASE.Models.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BASE.Areas.Frontend.Models
{
    public class VM_Album
    {
        /// <summary>列表_查詢參數</summary>
        public VM_AlbumQueryParam Search { get; set; } = new VM_AlbumQueryParam();
        /// <summary>列表</summary>
        public List<AlbumExtend>? AlbumExtendList { get; set; }
    }

    /// <summary>列表_查詢參數</summary>
    public class VM_AlbumQueryParam : VM_BaseQueryParam
    {
        public VM_AlbumQueryParam()
        {
            PagerInfo.m_iPageCount = 6;
        }
    }

}
