using System;
using System.Collections.Generic;

namespace BASE.Models.DB
{
    public partial class TbProfession
    {
        /// <summary>
        /// 使用者主鍵
        /// </summary>
        public string UserId { get; set; } = null!;
        /// <summary>
        /// 領域主鍵
        /// </summary>
        public string BacolId { get; set; } = null!;
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
