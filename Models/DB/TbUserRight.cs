using System;
using System.Collections.Generic;

namespace BASE.Models.DB
{
    /// <summary>
    /// 使用者權限(後台帳號)
    /// </summary>
    public partial class TbUserRight
    {
        /// <summary>
        /// 使用者代碼
        /// </summary>
        public string UserId { get; set; } = null!;
        /// <summary>
        /// 選單代碼
        /// </summary>
        public string MenuId { get; set; } = null!;
        public bool Enabled { get; set; }
        /// <summary>
        /// 檢視
        /// </summary>
        public bool ViewEnabled { get; set; }
        /// <summary>
        /// 新增
        /// </summary>
        public bool AddEnabled { get; set; }
        /// <summary>
        /// 刪除
        /// </summary>
        public bool DeleteEnabled { get; set; }
        /// <summary>
        /// 修改
        /// </summary>
        public bool ModifyEnabled { get; set; }
        /// <summary>
        /// 下載
        /// </summary>
        public bool DownloadEnabled { get; set; }
        /// <summary>
        /// 上傳
        /// </summary>
        public bool UploadEnabled { get; set; }
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

        public virtual TbMenuBack Menu { get; set; } = null!;
        public virtual TbUserInfo User { get; set; } = null!;
    }
}
