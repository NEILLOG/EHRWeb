using System;
using System.Collections.Generic;

namespace BASE.Models.DB
{
    /// <summary>
    /// 一頁式頁面內容
    /// </summary>
    public partial class TbOnePage
    {
        public string Id { get; set; } = null!;
        /// <summary>
        /// 內容
        /// </summary>
        public string Contents { get; set; } = null!;
        /// <summary>
        /// 是哪一個頁面的OnePage
        /// </summary>
        public string? Description { get; set; }
        public string? ModifyUser { get; set; }
        public DateTime? ModifyDate { get; set; }
    }
}
