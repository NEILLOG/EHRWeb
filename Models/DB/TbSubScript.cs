using System;
using System.Collections.Generic;

namespace BASE.Models.DB
{
    /// <summary>
    /// 訂閱服務
    /// </summary>
    public partial class TbSubScript
    {
        /// <summary>
        /// 流水號主鍵
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// 訂閱信箱；若重複訂閱，則自動覆蓋成最後一次選的訂閱項目
        /// </summary>
        public string Email { get; set; } = null!;
        public bool IsSubscriptActivity { get; set; }
        public bool IsSubsrcriptProjectNews { get; set; }
    }
}
