using System;
using System.Collections.Generic;

namespace BASE.Models.DB
{
    /// <summary>
    /// 場次資訊子表(含簽到記錄)，此表於報名完成時即產生
    /// </summary>
    public partial class TbActivityRegisterSection
    {
        /// <summary>
        /// 流水號
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// 活動編號
        /// </summary>
        public string ActivityId { get; set; } = null!;
        /// <summary>
        /// 活動註冊主表編號
        /// </summary>
        public long RegisterId { get; set; }
        /// <summary>
        /// 活動註冊場次編號
        /// </summary>
        public long RegisterSectionId { get; set; }
        /// <summary>
        /// 參加場次；線上、實體
        /// </summary>
        public string RegisterSectionType { get; set; } = null!;
        /// <summary>
        /// 是否為素食者；true: 素食者; false:葷食者
        /// </summary>
        public bool IsVegin { get; set; }
        /// <summary>
        /// 是否已簽到(上午場)
        /// </summary>
        public bool IsSigninAm { get; set; }
        /// <summary>
        /// 是否已簽到(下午場)
        /// </summary>
        public bool IsSigninPm { get; set; }
        /// <summary>
        /// 簽到日期(上午)
        /// </summary>
        public DateTime? SigninDateAm { get; set; }
        /// <summary>
        /// 簽到日期(下午)
        /// </summary>
        public DateTime? SigninDatePm { get; set; }
        public DateTime? ModifyDate { get; set; }
        public string? ModifyUser { get; set; }
    }
}
