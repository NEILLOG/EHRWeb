using System;
using System.Collections.Generic;

namespace BASE.Models.DB
{
    /// <summary>
    /// YouTube影片
    /// </summary>
    public partial class TbYouTubeVideo
    {
        /// <summary>
        /// 流水號主鍵
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// 標題
        /// </summary>
        public string Title { get; set; } = null!;
        /// <summary>
        /// 顯示日期
        /// </summary>
        public DateTime DisplayDate { get; set; }
        /// <summary>
        /// YouTube連結ID，如:https://www.youtube.com/watch?v=_ztCw228eSU中的_ztCw228eSU部分
        /// </summary>
        public string YouTubeId { get; set; } = null!;
        /// <summary>
        /// 是否註記刪除
        /// </summary>
        public bool IsDelete { get; set; }
        /// <summary>
        /// 是否上架
        /// </summary>
        public bool IsPublish { get; set; }
        public string CreateUser { get; set; } = null!;
        public DateTime CreateDate { get; set; }
        public string? ModifyUser { get; set; }
        public DateTime? ModifyDate { get; set; }
    }
}
