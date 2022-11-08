using System;
using System.Collections.Generic;

namespace BASE.Models.DB
{
    public partial class TbScheduleLogDetail
    {
        /// <summary>
        /// TB_ScheduleLog.SLID
        /// </summary>
        public int Slid { get; set; }
        /// <summary>
        /// 受影響項目ID
        /// </summary>
        public string AffectedId { get; set; } = null!;
        public string? AffectedBefore { get; set; }
        public string? AffectedAfter { get; set; }
        /// <summary>
        /// 詳細資訊
        /// </summary>
        public string? Message { get; set; }
    }
}
