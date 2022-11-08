using System;
using System.Collections.Generic;

namespace BASE.Models.DB
{
    public partial class TbHrArticle
    {
        /// <summary>
        /// 流水號主鍵
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// 標題
        /// </summary>
        public string Title { get; set; } = null!;
        /// <summary>
        /// 文章所屬類別
        /// </summary>
        public string Category { get; set; } = null!;
        /// <summary>
        /// 內容
        /// </summary>
        public string Contents { get; set; } = null!;
        public bool IsPublish { get; set; }
    }
}
