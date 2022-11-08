using System;
using System.Collections.Generic;

namespace BASE.Models.DB
{
    /// <summary>
    /// 國家資料
    /// </summary>
    public partial class TbCountry
    {
        /// <summary>
        /// 國家代碼
        /// </summary>
        public string CountryId { get; set; } = null!;
        /// <summary>
        /// 語系
        /// </summary>
        public string LangCode { get; set; } = null!;
        /// <summary>
        /// 名稱
        /// </summary>
        public string CountryName { get; set; } = null!;
        /// <summary>
        /// 國家編碼
        /// </summary>
        public string? CountryCode { get; set; }
        /// <summary>
        /// 是否刪除
        /// </summary>
        public bool IsDelete { get; set; }
        /// <summary>
        /// 建立者
        /// </summary>
        public string? CreateUser { get; set; }
        /// <summary>
        /// 建立日期
        /// </summary>
        public DateTime? CreateDate { get; set; }
        /// <summary>
        /// 修改者
        /// </summary>
        public string? ModifyUser { get; set; }
        /// <summary>
        /// 修改日期
        /// </summary>
        public DateTime? ModifyDate { get; set; }
    }
}
