using System;
using System.Collections.Generic;

namespace BASE.Models.DB
{
    /// <summary>
    /// 使用者經歷
    /// </summary>
    public partial class TbUserInfoExperience
    {
        public long Id { get; set; }
        /// <summary>
        /// 使用者主鍵
        /// </summary>
        public string UserId { get; set; } = null!;
        /// <summary>
        /// 服務/輔導單位
        /// </summary>
        public string ServDepartment { get; set; } = null!;
        public string JobTitle { get; set; } = null!;
        public string Durinration { get; set; } = null!;
    }
}
