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
        public bool IsDelete { get; set; }
        /// <summary>
        /// 是否上架
        /// </summary>
        public bool IsPublish { get; set; }
        public string? FileId { get; set; }
        public string CreateUser { get; set; } = null!;
        public DateTime CreateDate { get; set; }
        public string? ModifyUser { get; set; }
        public DateTime? ModifyDate { get; set; }
    }
}
