using System;
using System.Collections.Generic;

namespace BASE.Models.DB
{
    /// <summary>
    /// 輪播廣告
    /// </summary>
    public partial class TbAdvertise
    {
        /// <summary>
        /// 流水號主鍵
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// 圖片標題
        /// </summary>
        public string? Title { get; set; }
        /// <summary>
        /// 超連結
        /// </summary>
        public string? Link { get; set; }
        public int Sort { get; set; }
        /// <summary>
        /// 是否註記刪除
        /// </summary>
        public bool IsDelete { get; set; }
        /// <summary>
        /// 是否上架(true:是, false: 否)
        /// </summary>
        public bool IsPublish { get; set; }
        public string? FileId { get; set; }
        public string CreateUser { get; set; } = null!;
        public DateTime CreateDate { get; set; }
        public string? ModifyUser { get; set; }
        public DateTime? ModifyDate { get; set; }
    }
}
