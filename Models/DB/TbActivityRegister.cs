using System;
using System.Collections.Generic;

namespace BASE.Models.DB
{
    /// <summary>
    /// 活動報名資訊
    /// </summary>
    public partial class TbActivityRegister
    {
        /// <summary>
        /// 流水號主鍵
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// 活動主鍵編號
        /// </summary>
        public string ActivityId { get; set; } = null!;
        /// <summary>
        /// 企業名稱
        /// </summary>
        public string CompanyName { get; set; } = null!;
        /// <summary>
        /// 企業所在地
        /// </summary>
        public string CompanyLocation { get; set; } = null!;
        /// <summary>
        /// 產業別
        /// </summary>
        public string CompanyType { get; set; } = null!;
        /// <summary>
        /// 姓名
        /// </summary>
        public string Name { get; set; } = null!;
        /// <summary>
        /// 職稱
        /// </summary>
        public string JobTitle { get; set; } = null!;
        /// <summary>
        /// 連絡電話
        /// </summary>
        public string Phone { get; set; } = null!;
        /// <summary>
        /// 手機
        /// </summary>
        public string CellPhone { get; set; } = null!;
        /// <summary>
        /// 電子郵件
        /// </summary>
        public string Email { get; set; } = null!;
        /// <summary>
        /// 公司員工人數
        /// </summary>
        public string CompanyEmpAmount { get; set; } = null!;
        /// <summary>
        /// 訊息來源
        /// </summary>
        public string InfoFrom { get; set; } = null!;
        /// <summary>
        /// 是否審核通過(null預設；true通過；false不通過)
        /// </summary>
        public bool? IsValid { get; set; }
        public DateTime CreateDate { get; set; }
        public string? ModifyUser { get; set; }
        public DateTime? ModifyDate { get; set; }
    }
}
