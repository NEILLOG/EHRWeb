using System;
using System.Collections.Generic;

namespace BASE.Models.DB
{
    /// <summary>
    /// 課程變更管理
    /// </summary>
    public partial class TbProjectModify
    {
        /// <summary>
        /// 流水號主鍵
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// 外鍵: 計畫編號
        /// </summary>
        public string ProjectId { get; set; } = null!;
        /// <summary>
        /// 課程變更檔案
        /// </summary>
        public string FileId { get; set; } = null!;
        /// <summary>
        /// 聯絡信箱
        /// </summary>
        public string Email { get; set; } = null!;
        public DateTime CreateDate { get; set; }
        /// <summary>
        /// 是否同意; null: 尚未回覆, 1: 同意, 0:不同意
        /// </summary>
        public bool? IsApprove { get; set; }
        public string? ModifyUser { get; set; }
        /// <summary>
        /// 經辦
        /// </summary>
        public DateTime? ModifyDate { get; set; }
    }
}
