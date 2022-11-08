using System;
using System.Collections.Generic;

namespace BASE.Models.DB
{
    public partial class TbMenuFront
    {
        /// <summary>
        /// 選單Key
        /// </summary>
        public string MenuId { get; set; } = null!;
        public string LangId { get; set; } = null!;
        public string? MenuName { get; set; }
        /// <summary>
        /// 父選單Key
        /// </summary>
        public string MenuParent { get; set; } = null!;
        /// <summary>
        /// 選單路徑
        /// </summary>
        public string? MenuUrl { get; set; }
        public string? MenuIcon { get; set; }
        /// <summary>
        /// 選單層級
        /// </summary>
        public int MenuLevel { get; set; }
        /// <summary>
        /// 選單排序
        /// </summary>
        public int MenuOrder { get; set; }
        /// <summary>
        /// 是否啟動
        /// </summary>
        public bool IsActive { get; set; }
    }
}
