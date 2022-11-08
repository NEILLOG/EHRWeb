using System;
using System.Collections.Generic;

namespace BASE.Models.DB
{
    /// <summary>
    /// 計劃管理主檔
    /// </summary>
    public partial class TbProject
    {
        public string Id { get; set; } = null!;
        /// <summary>
        /// 計畫名稱
        /// </summary>
        public string Name { get; set; } = null!;
        /// <summary>
        /// 計畫所屬類別
        /// </summary>
        public string Category { get; set; } = null!;
        /// <summary>
        /// 計畫連結
        /// </summary>
        public string Link { get; set; } = null!;
        /// <summary>
        /// 目的
        /// </summary>
        public string Purpose { get; set; } = null!;
        /// <summary>
        /// 申請對象
        /// </summary>
        public string Target { get; set; } = null!;
        /// <summary>
        /// 內容
        /// </summary>
        public string Contents { get; set; } = null!;
        /// <summary>
        /// 聯繫窗口
        /// </summary>
        public string Contact { get; set; } = null!;
        public bool IsDelete { get; set; }
        public string CreateUser { get; set; } = null!;
        public DateTime CreateDate { get; set; }
        public string? ModifyUser { get; set; }
        public DateTime? ModifyDate { get; set; }
    }
}
