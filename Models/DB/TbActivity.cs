using System;
using System.Collections.Generic;

namespace BASE.Models.DB
{
    /// <summary>
    /// 活動訊息
    /// </summary>
    public partial class TbActivity
    {
        /// <summary>
        /// 主鍵
        /// </summary>
        public string Id { get; set; } = null!;
        /// <summary>
        /// 類型: 課程, 講座, 活動
        /// </summary>
        public string Category { get; set; } = null!;
        /// <summary>
        /// 問卷主鍵編號
        /// </summary>
        public string Qid { get; set; } = null!;
        /// <summary>
        /// 報名開始日期
        /// </summary>
        public DateTime RegStartDate { get; set; }
        /// <summary>
        /// 報名結束日期
        /// </summary>
        public DateTime RegEndDate { get; set; }
        /// <summary>
        /// 活動標題
        /// </summary>
        public string Title { get; set; } = null!;
        /// <summary>
        /// 活動主題
        /// </summary>
        public string Subject { get; set; } = null!;
        /// <summary>
        /// 活動時長；半日, 全日
        /// </summary>
        public string DateType { get; set; } = null!;
        /// <summary>
        /// 講師資訊
        /// </summary>
        public string? LecturerInfo { get; set; }
        /// <summary>
        /// 活動簡介
        /// </summary>
        public string Description { get; set; } = null!;
        /// <summary>
        /// 報名名額
        /// </summary>
        public string Quota { get; set; } = null!;
        /// <summary>
        /// 報名對象
        /// </summary>
        public string RegisterFor { get; set; } = null!;
        /// <summary>
        /// 活動地點
        /// </summary>
        public string Place { get; set; } = null!;
        /// <summary>
        /// 是否已註記刪除
        /// </summary>
        public bool IsDelete { get; set; }
        /// <summary>
        /// 是否開放報名
        /// </summary>
        public bool IsPublish { get; set; }
        /// <summary>
        /// 是否審核通過
        /// </summary>
        public bool IsValid { get; set; }
        public string CreateUser { get; set; } = null!;
        public DateTime CreateDate { get; set; }
        public string? ModifyUser { get; set; }
        public DateTime? ModifyDate { get; set; }
        /// <summary>
        /// 活動圖片
        /// </summary>
        public string? ActivityImage { get; set; }
        /// <summary>
        /// 上傳行前通知信壓縮檔(for實體參與者)
        /// </summary>
        public string? FileForEntity { get; set; }
        /// <summary>
        /// 上傳行前通知信壓縮檔(for線上參與者)
        /// </summary>
        public string? FileForOnline { get; set; }
        /// <summary>
        /// 上傳講義
        /// </summary>
        public string? HandoutFile { get; set; }
    }
}
