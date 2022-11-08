using System;
using System.Collections.Generic;

namespace BASE.Models.DB
{
    /// <summary>
    /// 後臺操作紀錄
    /// </summary>
    public partial class TbActionLog
    {
        /// <summary>
        /// 主鍵
        /// </summary>
        public long Pid { get; set; }
        /// <summary>
        /// 位置
        /// </summary>
        public string Platform { get; set; } = null!;
        /// <summary>
        /// 動作
        /// </summary>
        public string Action { get; set; } = null!;
        /// <summary>
        /// 當下異動的資料
        /// </summary>
        public string? Data { get; set; }
        /// <summary>
        /// 操作者
        /// </summary>
        public string? UserId { get; set; }
        /// <summary>
        /// 操作時間
        /// </summary>
        public DateTime Date { get; set; }
        /// <summary>
        /// IP
        /// </summary>
        public string? Ip { get; set; }
    }
}
