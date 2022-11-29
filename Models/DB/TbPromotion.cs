using System;
using System.Collections.Generic;

namespace BASE.Models.DB
{
    /// <summary>
    /// 推廣管理
    /// </summary>
    public partial class TbPromotion
    {
        public long Id { get; set; }
        /// <summary>
        /// 公司名稱
        /// </summary>
        public string CompanyName { get; set; } = null!;
        /// <summary>
        /// 信箱
        /// </summary>
        public string Email { get; set; } = null!;
        /// <summary>
        /// 企業人數
        /// </summary>
        public int EmpoyeeAmount { get; set; }
        /// <summary>
        /// 計畫
        /// </summary>
        public string Project { get; set; } = null!;
        /// <summary>
        /// 統一編號
        /// </summary>
        public string BusinessId { get; set; } = null!;
        /// <summary>
        /// 企業所在地
        /// </summary>
        public string CompanyLocation { get; set; } = null!;
        /// <summary>
        /// 申請人
        /// </summary>
        public string Applicant { get; set; } = null!;
    }
}
