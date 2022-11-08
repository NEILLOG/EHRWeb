using System;
using System.Collections.Generic;

namespace BASE.Models.DB
{
    /// <summary>
    /// 活動花絮
    /// </summary>
    public partial class TbAlbum
    {
        /// <summary>
        /// 主鍵
        /// </summary>
        public string Id { get; set; } = null!;
        /// <summary>
        /// 標題
        /// </summary>
        public string Title { get; set; } = null!;
        /// <summary>
        /// 顯示日期
        /// </summary>
        public DateTime DisplayDate { get; set; }
        public bool IsDelete { get; set; }
        public string? FileId { get; set; }
        public string CreateUser { get; set; } = null!;
        public DateTime CreateDate { get; set; }
        public string? ModifyUser { get; set; }
        public DateTime? ModifyDate { get; set; }
    }
}
