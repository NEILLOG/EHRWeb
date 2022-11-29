using System;
using System.Collections.Generic;

namespace BASE.Models.DB
{
    /// <summary>
    /// 問卷選項
    /// </summary>
    public partial class TbQuizOption
    {
        public Guid Id { get; set; }
        public string QuizId { get; set; } = null!;
        /// <summary>
        /// 題目類型: 1:標題 2:簡答題 3:單選 4:複選
        /// </summary>
        public byte Type { get; set; }
        /// <summary>
        /// 若類型為標題，則用來儲存標題列文字；若類型為其他，則儲存問題描述
        /// </summary>
        public string QuizDescription { get; set; } = null!;
        /// <summary>
        /// 填寫說明
        /// </summary>
        public string? FillDirection { get; set; }
        /// <summary>
        /// 選項們；儲存格是: {選項}|{選項}|...；順序即代表顯示順序
        /// </summary>
        public string? Options { get; set; }
        public bool IsRequired { get; set; }
        public int Sort { get; set; }
    }
}
