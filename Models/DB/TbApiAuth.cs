using System;
using System.Collections.Generic;

namespace BASE.Models.DB
{
    /// <summary>
    /// API授權清單
    /// </summary>
    public partial class TbApiAuth
    {
        /// <summary>
        /// API代碼
        /// </summary>
        public string Aid { get; set; } = null!;
        /// <summary>
        /// 解密後
        /// </summary>
        public string AuthToken { get; set; } = null!;
        /// <summary>
        /// 解密前
        /// </summary>
        public string? EnAuthTk { get; set; }
        /// <summary>
        /// 單位
        /// </summary>
        public string? UseUnit { get; set; }
        /// <summary>
        /// 聯絡人
        /// </summary>
        public string? Contactor { get; set; }
        /// <summary>
        /// 聯絡人電話
        /// </summary>
        public string? Phone { get; set; }
        /// <summary>
        /// 是否啟用
        /// </summary>
        public bool IsActive { get; set; }
        /// <summary>
        /// 過期日
        /// </summary>
        public DateTime? ExpiredDate { get; set; }
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
