using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace BASE.Models.Enums
{
    /// <summary>
    /// Banner 狀態
    /// </summary>
    public enum PublishStatus
    {
        /// <summary>
        /// 待上架
        /// </summary>
        [Display(Name = "待上架")]
        [EnumMember(Value = "PENDING")]
        Pending = 1,

        /// <summary>
        /// 下架
        /// </summary>
        [Display(Name = "下架")]
        [EnumMember(Value = "OFF")]
        Off = 2,

        /// <summary>
        /// 上架
        /// </summary>
        [Display(Name = "上架")]
        [EnumMember(Value = "ON")]
        On = 3,

    }

    /// <summary>
    /// 啟用/停用
    /// </summary>
    public enum ActiveStatus
    {
        /// <summary>
        /// 停用
        /// </summary>
        [Display(Name = "停用", Order = 1)]
        [EnumMember(Value = "False")]
        False = 0,

        /// <summary>
        /// 啟用
        /// </summary>
        [Display(Name = "啟用", Order = 0)]
        [EnumMember(Value = "True")]
        True = 1,

    }

    /// <summary>
    /// 審核狀態
    /// </summary>
    public enum ReviewStatus
    {
        /// <summary>
        /// 待審核
        /// </summary>
        [Display(Name = "待審核")]
        Pending = -1,

        /// <summary>
        /// 審核不通過
        /// </summary>
        [Display(Name = "審核不通過")]
        Reject = 0,

        /// <summary>
        /// 審核通過
        /// </summary>
        [Display(Name = "審核通過")]
        Approval = 1,
    }

    /// <summary>
    /// 性別
    /// </summary>
    public enum Sex
    {
        /// <summary>
        /// 女
        /// </summary>
        [Display(Name = "女")]
        [EnumMember(Value = "F")]
        Female = 0,

        /// <summary>
        /// 男
        /// </summary>
        [Display(Name = "男")]
        [EnumMember(Value = "M")]
        Male = 1,

    }

    /// <summary>
    /// 是或否
    /// </summary>
    public enum BooleanValue
    {
        /// <summary>
        /// 否
        /// </summary>
        [Display(Name = "否", Order = 1)]
        [EnumMember(Value = "False")]
        False = 0,

        /// <summary>
        /// 男
        /// </summary>
        [Display(Name = "是", Order = 0)]
        [EnumMember(Value = "True")]
        True = 1,

    }

    /// <summary>
    /// 國內/外
    /// </summary>
    public enum RegionType
    {
        /// <summary>
        /// 否
        /// </summary>
        [Display(Name = "國內")]
        [EnumMember(Value = "Home")]
        Home = 0,

        /// <summary>
        /// 男
        /// </summary>
        [Display(Name = "海外")]
        [EnumMember(Value = "Abroad")]
        Abroad = 1,

    }

    /// <summary>
    /// 信件狀態
    /// </summary>
    public enum MailStatus
    {
        /// <summary>
        /// 待處理
        /// </summary>
        [Display(Name = "待處理")]
        Pending = -1,

        /// <summary>
        /// 發送失敗
        /// </summary>
        [Display(Name = "發送失敗")]
        Failed = 0,

        /// <summary>
        /// 發送成功
        /// </summary>
        [Display(Name = "發送成功")]
        Success = 1,

    }

    public enum Language
    {
        [Display(Name = "繁體中文")]
        tw,

        [Display(Name = "英文")]
        en
    }

    public enum AddressType
    {
        /// <summary>
        /// 通訊地址
        /// </summary>
        [Display(Name = "通訊地址")]
        Current = 0,
        /// <summary>
        /// 永久地址
        /// </summary>
        [Display(Name = "永久地址")]
        Permanent = 1,
        /// <summary>
        /// 自訂地址
        /// </summary>
        [Display(Name = "自訂地址")]
        Custom = 2
    }

    public enum PageStatus
    {
        /// <summary>
        /// 待處理
        /// </summary>
        Pending = -1,
        
        /// <summary>
        /// 未通過
        /// </summary>
        Reject = 0,

        /// <summary>
        ///  已通過
        /// </summary>
        Processed = 1
    }
}
