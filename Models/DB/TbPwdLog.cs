using System;
using System.Collections.Generic;

namespace BASE.Models.DB
{
    public partial class TbPwdLog
    {
        /// <summary>
        /// 主鍵
        /// </summary>
        public long Plid { get; set; }
        /// <summary>
        /// 使用者主鍵
        /// </summary>
        public string UserId { get; set; } = null!;
        /// <summary>
        /// 密碼
        /// </summary>
        public string Password { get; set; } = null!;
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
