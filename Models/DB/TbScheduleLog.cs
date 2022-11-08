using System;
using System.Collections.Generic;

namespace BASE.Models.DB
{
    public partial class TbScheduleLog
    {
        /// <summary>
        /// 主鍵 (yyMMddHHmm)
        /// </summary>
        public int Slid { get; set; }
        /// <summary>
        /// 排程名稱
        /// </summary>
        public string ScheduleName { get; set; } = null!;
        /// <summary>
        /// 執行時間
        /// </summary>
        public DateTime ProcessingTime { get; set; }
        /// <summary>
        /// 對應設定檔ID，TB_SY_MemberSetting.SettingID
        /// </summary>
        public long SettingId { get; set; }
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSuccess { get; set; }
        /// <summary>
        /// 回傳結果
        /// </summary>
        public string? ResponseMessage { get; set; }
    }
}
