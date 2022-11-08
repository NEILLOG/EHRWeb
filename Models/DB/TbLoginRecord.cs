using System;
using System.Collections.Generic;

namespace BASE.Models.DB
{
    /// <summary>
    /// 登入記錄
    /// </summary>
    public partial class TbLoginRecord
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
        /// 會員編號
        /// </summary>
        public string? Account { get; set; }
        /// <summary>
        /// 會員主鍵
        /// </summary>
        public string? MemberId { get; set; }
        /// <summary>
        /// 輸入密碼
        /// </summary>
        public string? Inputaua8 { get; set; }
        /// <summary>
        /// IP
        /// </summary>
        public string? Ip { get; set; }
        public string? UserId { get; set; }
        /// <summary>
        /// 登入時間
        /// </summary>
        public DateTime LoginTime { get; set; }
        /// <summary>
        /// 訊息
        /// </summary>
        public string LoginMsg { get; set; } = null!;
        /// <summary>
        /// 登入裝置
        /// </summary>
        public string? UserAgent { get; set; }
        /// <summary>
        /// 是否SSO登入
        /// </summary>
        public bool Sso { get; set; }
        /// <summary>
        /// SSO回傳XMLResult
        /// </summary>
        public string? Ssoresult { get; set; }
    }
}
