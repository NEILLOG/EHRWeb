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
    }
}
