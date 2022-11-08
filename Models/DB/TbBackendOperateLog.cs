using System;
using System.Collections.Generic;

namespace BASE.Models.DB
{
    public partial class TbBackendOperateLog
    {
        public long Pid { get; set; }
        public string UserId { get; set; } = null!;
        public string Feature { get; set; } = null!;
        public string Action { get; set; } = null!;
        public string? Ip { get; set; }
        public string? Url { get; set; }
        /// <summary>
        /// 登入裝置
        /// </summary>
        public string? UserAgent { get; set; }
        public bool IsSuccess { get; set; }
        /// <summary>
        /// 異動資料 Key 值
        /// </summary>
        public string? DataKey { get; set; }
        public string? Message { get; set; }
        public string? Request { get; set; }
        public string? Response { get; set; }
        public DateTime CreateDate { get; set; }

        public virtual TbUserInfo User { get; set; } = null!;
    }
}
