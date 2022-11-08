using System;
using System.Collections.Generic;

namespace BASE.Models.DB
{
    /// <summary>
    /// 後台帳號所屬群組
    /// </summary>
    public partial class TbUserInGroup
    {
        /// <summary>
        /// 使用者代碼
        /// </summary>
        public string UserId { get; set; } = null!;
        /// <summary>
        /// 群組代碼
        /// </summary>
        public string GroupId { get; set; } = null!;
        /// <summary>
        /// 建立者
        /// </summary>
        public string? CreateUser { get; set; }
        /// <summary>
        /// 建立日期
        /// </summary>
        public DateTime? CreateDate { get; set; }
        /// <summary>
        /// 修改者
        /// </summary>
        public string? ModifyUser { get; set; }
        /// <summary>
        /// 修改日期
        /// </summary>
        public DateTime? ModifyDate { get; set; }

        public virtual TbGroupInfo Group { get; set; } = null!;
        public virtual TbUserInfo User { get; set; } = null!;
    }
}
