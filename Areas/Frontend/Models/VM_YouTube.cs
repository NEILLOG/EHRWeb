using BASE.Areas.Frontend.Models.Extend;
using BASE.Models.DB;
using BASE.Models.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BASE.Areas.Frontend.Models
{
    public class VM_YouTube
    {
        /// <summary>列表_查詢參數</summary>
        public VM_YouTubeQueryParam Search { get; set; } = new VM_YouTubeQueryParam();
        /// <summary>列表</summary>
        public List<TbYouTubeVideo>? DataList { get; set; }
    }

    /// <summary>列表_查詢參數</summary>
    public class VM_YouTubeQueryParam : VM_BaseQueryParam
    {
        public VM_YouTubeQueryParam()
        {
            PagerInfo.m_iPageCount = 6;
        }
    }

}
