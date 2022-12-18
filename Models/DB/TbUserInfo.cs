using System;
using System.Collections.Generic;

namespace BASE.Models.DB
{
    /// <summary>
    /// 使用者(後台帳號)
    /// </summary>
    public partial class TbUserInfo
    {
        public TbUserInfo()
        {
            TbBackendOperateLog = new HashSet<TbBackendOperateLog>();
            TbUserInGroup = new HashSet<TbUserInGroup>();
            TbUserRight = new HashSet<TbUserRight>();
        }

        /// <summary>
        /// 代碼
        /// </summary>
        public string UserId { get; set; } = null!;
        /// <summary>
        /// 帳號
        /// </summary>
        public string Account { get; set; } = null!;
        /// <summary>
        /// 密碼
        /// </summary>
        public string? Aua8 { get; set; }
        /// <summary>
        /// 使用者名稱
        /// </summary>
        public string UserName { get; set; } = null!;
        public string? Phone { get; set; }
        /// <summary>
        /// 信箱
        /// </summary>
        public string? Email { get; set; }
        /// <summary>
        /// 照片
        /// </summary>
        public string? Photo { get; set; }
        /// <summary>
        /// 是否啟用
        /// </summary>
        public bool IsActive { get; set; }
        /// <summary>
        /// 生效日期
        /// </summary>
        public DateTime? EffectiveStartdate { get; set; }
        /// <summary>
        /// 失效日期
        /// </summary>
        public DateTime? EffectiveEnddate { get; set; }
        public string? Seq { get; set; }
        public string? UnitA { get; set; }
        public string? UnitB { get; set; }
        public string? Title { get; set; }
        public string? AccountTypeCode { get; set; }
        public string? AccountStatusCode { get; set; }
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
        /// <summary>
        /// 手機
        /// </summary>
        public string? CellPhone { get; set; }
        /// <summary>
        /// 性別
        /// </summary>
        public string? Sex { get; set; }
        /// <summary>
        /// 身分證
        /// </summary>
        public string? IdNumber { get; set; }
        /// <summary>
        /// 產/行業
        /// </summary>
        public string? Industry { get; set; }
        /// <summary>
        /// 服務單位
        /// </summary>
        public string? ServiceUnit { get; set; }
        /// <summary>
        /// 通訊地址
        /// </summary>
        public string? ContactAddr { get; set; }
        /// <summary>
        /// 戶籍地址
        /// </summary>
        public string? PermanentAddr { get; set; }
        /// <summary>
        /// 學歷
        /// </summary>
        public string? Education { get; set; }
        /// <summary>
        /// 專長
        /// </summary>
        public string? Expertise { get; set; }
        /// <summary>
        /// 職稱
        /// </summary>
        public string? JobTitle { get; set; }
        /// <summary>
        /// 專業領域, 使用逗號分隔；可能包含以下值: 組織經營,組織轉型,人才培育,職能分析,員工職涯發展,人力資源管理,勞資關係、法令
        /// </summary>
        public string? Skill { get; set; }
        /// <summary>
        /// 登入失敗次數
        /// </summary>
        public int? ErrorCount { get; set; }
        /// <summary>
        /// 帳號鎖定時間
        /// </summary>
        public DateTime? LockTime { get; set; }
        /// <summary>
        /// 忘記密碼隨機碼
        /// </summary>
        public string? Token { get; set; }
        public virtual ICollection<TbBackendOperateLog> TbBackendOperateLog { get; set; }
        public virtual ICollection<TbUserInGroup> TbUserInGroup { get; set; }
        public virtual ICollection<TbUserRight> TbUserRight { get; set; }
    }
}
