using System;
using System.Collections.Generic;

namespace BASE.Models.DB
{
    /// <summary>
    /// 最新消息
    /// </summary>
    public partial class TbNews
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
        /// 類型
        /// </summary>
        public string Category { get; set; } = null!;
        /// <summary>
        /// 顯示日期
        /// </summary>
        public DateTime DisplayDate { get; set; }
        /// <summary>
        /// 最新消息內文，包含HTML
        /// </summary>
        public string Contents { get; set; } = null!;
        /// <summary>
        /// 是否註記刪除
        /// </summary>
        public bool IsDelete { get; set; }
        /// <summary>
        /// 是否上架
        /// </summary>
        public bool IsPublish { get; set; }
        /// <summary>
        /// 是否置頂顯示；若有多個置頂，則以日期降冪排序
        /// </summary>
        public bool IsKeepTop { get; set; }
        /// <summary>
        /// 附件主鍵編號
        /// </summary>
        public string? FileId { get; set; }
        public string CreateUser { get; set; } = null!;
        public DateTime CreateDate { get; set; }
        public string? ModifyUser { get; set; }
        public DateTime? ModifyDate { get; set; }
    }
}
