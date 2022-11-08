using System;
using System.Collections.Generic;

namespace BASE.Models.DB
{
    /// <summary>
    /// 檔案(共用)
    /// </summary>
    public partial class TbFileInfo
    {
        /// <summary>
        /// 檔案代碼
        /// </summary>
        public string FileId { get; set; } = null!;
        /// <summary>
        /// 名稱
        /// </summary>
        public string? FileName { get; set; }
        /// <summary>
        /// 實際檔名
        /// </summary>
        public string? FileRealName { get; set; }
        /// <summary>
        /// 檔案描述
        /// </summary>
        public string? FileDescription { get; set; }
        /// <summary>
        /// 檔案路徑
        /// </summary>
        public string FilePath { get; set; } = null!;
        /// <summary>
        /// 檔案描述
        /// </summary>
        public string? FilePathM { get; set; }
        public string? FilePathS { get; set; }
        public byte? Order { get; set; }
        /// <summary>
        /// 是否刪除
        /// </summary>
        public bool IsDelete { get; set; }
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
    }
}
