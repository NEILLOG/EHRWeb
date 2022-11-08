using System;
using System.Collections.Generic;

namespace BASE.Models.DB
{
    /// <summary>
    /// 活動參加問卷回覆
    /// </summary>
    public partial class TbActivityQuizResponse
    {
        /// <summary>
        /// 流水號主鍵
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// 活動報名主鍵
        /// </summary>
        public long RegisterId { get; set; }
        /// <summary>
        /// 問卷編號
        /// </summary>
        public string QuizId { get; set; } = null!;
        /// <summary>
        /// 選項主鍵編號
        /// </summary>
        public long QuizOptionId { get; set; }
        /// <summary>
        /// 簡答題文字
        /// </summary>
        public string? ResponseText { get; set; }
        /// <summary>
        /// 使用者回覆選項；格式: {選項}|{選項}|...；採用落地儲存；不論單選、複選均適用
        /// </summary>
        public string? SelctedOption { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
