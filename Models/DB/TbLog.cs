using System;
using System.Collections.Generic;

namespace BASE.Models.DB
{
    /// <summary>
    /// 系統紀錄
    /// </summary>
    public partial class TbLog
    {
        /// <summary>
        /// 主鍵
        /// </summary>
        public int Pid { get; set; }
        /// <summary>
        /// 位置
        /// </summary>
        public string Platform { get; set; } = null!;
        /// <summary>
        /// 動作
        /// </summary>
        public string Action { get; set; } = null!;
        /// <summary>
        /// 訊息
        /// </summary>
        public string? Message { get; set; }
        /// <summary>
        /// 時間
        /// </summary>
        public DateTime Date { get; set; }
        /// <summary>
        /// IP
        /// </summary>
        public string? Ip { get; set; }
        public string? Url { get; set; }
        /// <summary>
        /// 登入裝置
        /// </summary>
        public string? UserAgent { get; set; }
    }
}
