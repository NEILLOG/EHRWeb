using System;
using System.Collections.Generic;

namespace BASE.Models.DB
{
    /// <summary>
    /// 信件
    /// </summary>
    public partial class TbMailQueue
    {
        /// <summary>
        /// 主鍵
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// 寄件者
        /// </summary>
        public string MailFrom { get; set; } = null!;
        /// <summary>
        /// 收件者，限定一位
        /// </summary>
        public string MailTo { get; set; } = null!;
        /// <summary>
        /// 信件主旨
        /// </summary>
        public string Subject { get; set; } = null!;
        /// <summary>
        /// 信件內容，可包含HTML
        /// </summary>
        public string Contents { get; set; } = null!;
        /// <summary>
        /// 預計發送日期；若無排定日期，則盡快發送
        /// </summary>
        public DateTime? PlanSendDate { get; set; }
        /// <summary>
        /// 是否已發送
        /// </summary>
        public bool IsSend { get; set; }
        /// <summary>
        /// 實際發送日期
        /// </summary>
        public DateTime? SendDate { get; set; }
        /// <summary>
        /// 關聯資料主鍵；用於當例如取消活動時，要一併刪除使用
        /// </summary>
        public string? RelationId { get; set; }
        /// <summary>
        /// 附件檔案
        /// </summary>
        public string? FileId1 { get; set; }
        /// <summary>
        /// 附件檔案
        /// </summary>
        public string? FileId2 { get; set; }
        /// <summary>
        /// 附件檔案
        /// </summary>
        public string? FileId3 { get; set; }
        public DateTime CreateDate { get; set; }
        public string CreateUser { get; set; } = null!;
        public DateTime? ModifyDate { get; set; }
        public string? ModifyUser { get; set; }
    }
}
