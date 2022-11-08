using System;
using System.Collections.Generic;

namespace BASE.Models.DB
{
    /// <summary>
    /// 活動時段
    /// </summary>
    public partial class TbActivitySection
    {
        /// <summary>
        /// 時段流水號主鍵
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// 活動主鍵編號
        /// </summary>
        public string ActivityId { get; set; } = null!;
        /// <summary>
        /// 場次日期
        /// </summary>
        public DateTime Day { get; set; }
        /// <summary>
        /// 場次開始時間，EF內請使用TimeSpan儲存
        /// </summary>
        public TimeSpan StartTime { get; set; }
        /// <summary>
        /// 場次結束時間，EF內請使用TimeSpan儲存
        /// </summary>
        public TimeSpan EndTime { get; set; }
        /// <summary>
        /// 活動參與模式: 實體, 線上, 實體加線上
        /// </summary>
        public string SectionType { get; set; } = null!;
    }
}
