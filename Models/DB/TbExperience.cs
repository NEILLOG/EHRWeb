using System;
using System.Collections.Generic;

namespace BASE.Models.DB
{
    public partial class TbExperience
    {
        /// <summary>
        /// 主鍵
        /// </summary>
        public long Exid { get; set; }
        /// <summary>
        /// 使用者主鍵
        /// </summary>
        public string UserId { get; set; } = null!;
        /// <summary>
        /// 服務單位
        /// </summary>
        public string? ServiceUnit { get; set; }
        /// <summary>
        /// 職稱
        /// </summary>
        public string? JobTitle { get; set; }
        /// <summary>
        /// 服務期間
        /// </summary>
        public string? Period { get; set; }
        /// <summary>
        /// 建立者
        /// </summary>
        public string? CreateUser { get; set; }
        /// <summary>
        /// 建立日期
        /// </summary>
        public DateTime? CreateDate { get; set; }
    }
}
