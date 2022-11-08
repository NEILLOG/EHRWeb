using System;
using System.Collections.Generic;

namespace BASE.Models.DB
{
    /// <summary>
    /// 問卷主檔
    /// </summary>
    public partial class TbQuiz
    {
        /// <summary>
        /// 主鍵
        /// </summary>
        public string Id { get; set; } = null!;
        /// <summary>
        /// 問卷名稱
        /// </summary>
        public string Name { get; set; } = null!;
        /// <summary>
        /// 問卷描述
        /// </summary>
        public string Description { get; set; } = null!;
    }
}
