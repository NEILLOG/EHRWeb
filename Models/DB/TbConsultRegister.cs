using System;
using System.Collections.Generic;

namespace BASE.Models.DB
{
    /// <summary>
    /// 諮詢輔導服務報名
    /// </summary>
    public partial class TbConsultRegister
    {
        public long Id { get; set; }
        /// <summary>
        /// 企業所在地
        /// </summary>
        public string Location { get; set; } = null!;
        /// <summary>
        /// 企業名稱全銜
        /// </summary>
        public string Name { get; set; } = null!;
        /// <summary>
        /// 企業統編
        /// </summary>
        public string BusinessId { get; set; } = null!;
        /// <summary>
        /// 企業登記地址
        /// </summary>
        public string Address { get; set; } = null!;
        /// <summary>
        /// 諮詢主題；若有多筆則以逗號串聯
        /// </summary>
        public string ConsultSubjects { get; set; } = null!;
        /// <summary>
        /// 問題陳述
        /// </summary>
        public string Description { get; set; } = null!;
        /// <summary>
        /// 預計可諮詢時間
        /// </summary>
        public string ConsultTime { get; set; } = null!;
        /// <summary>
        /// 輔導地址
        /// </summary>
        public string ConsultAddress { get; set; } = null!;
        /// <summary>
        /// 聯繫人姓名
        /// </summary>
        public string ContactName { get; set; } = null!;
        /// <summary>
        /// 聯繫人職稱
        /// </summary>
        public string ContactJobTitle { get; set; } = null!;
        public string ContactPhone { get; set; } = null!;
        /// <summary>
        /// 聯絡人EMAIL
        /// </summary>
        public string ContactEmail { get; set; } = null!;
        public DateTime CreateDate { get; set; }
        /// <summary>
        /// 指派顧問1
        /// </summary>
        public string? AssignAdviser1 { get; set; }
        /// <summary>
        /// 指派顧問2
        /// </summary>
        public string? AssignAdviser2 { get; set; }
        /// <summary>
        /// 指派顧問3
        /// </summary>
        public string? AssignAdviser3 { get; set; }
        /// <summary>
        /// 指派輔導助理
        /// </summary>
        public string? AssignAdviserAssistant { get; set; }
        /// <summary>
        /// 協調後可輔導的日期
        /// </summary>
        public DateTime? ReAssignDate { get; set; }
        /// <summary>
        /// 協調後可輔導的時段
        /// </summary>
        public TimeSpan? ReAssignTime { get; set; }
        /// <summary>
        /// 是否審核通過
        /// </summary>
        public bool? IsApprove { get; set; }
        public bool IsClose { get; set; }
        /// <summary>
        /// 輔導紀錄檔案
        /// </summary>
        public string? CounselingLogFile { get; set; }
        /// <summary>
        /// 已填寫簽到表
        /// </summary>
        public string? SigninFormFile { get; set; }
        /// <summary>
        /// 企業需求調查表回傳檔案
        /// </summary>
        public string? RequireSurveyFile { get; set; }
        /// <summary>
        /// 滿意度調查檔案
        /// </summary>
        public string? SatisfySurveyFile { get; set; }
        public string? ModifyUser { get; set; }
        public DateTime? ModifyDate { get; set; }
    }
}
