using BASE.Models;

namespace BASE.Areas.Backend.Models.Base
{
    /// <summary>查詢參數的基底(含分頁及排序)</summary>
    public class VM_BaseQueryParam
    {
        public VM_BaseQueryParam()
        {
            this.PagerInfo = new PagerInfo();
            this.PagerInfo.m_iPageCount = 10;
        }

        /// <summary>排序欄位</summary>
        public string SortOrder { get; set; } = string.Empty;

        /// <summary>分頁參數</summary>
        public PagerInfo PagerInfo { get; set; }
    }
}
