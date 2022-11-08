using System;
using System.Collections.Generic;

namespace BASE.Models.DB
{
    /// <summary>
    /// HR材料包
    /// </summary>
    public partial class TbHrPackage
    {
        public string Id { get; set; } = null!;
        public string Title { get; set; } = null!;
        public DateTime DisplayDate { get; set; }
        public int Sort { get; set; }
        /// <summary>
        /// 是否上架
        /// </summary>
        public bool IsPublish { get; set; }
    }
}
