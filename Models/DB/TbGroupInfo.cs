using System;
using System.Collections.Generic;

namespace BASE.Models.DB
{
    /// <summary>
    /// 群組
    /// </summary>
    public partial class TbGroupInfo
    {
        public TbGroupInfo()
        {
            TbGroupRight = new HashSet<TbGroupRight>();
            TbUserInGroup = new HashSet<TbUserInGroup>();
        }

        /// <summary>
        /// 群組代碼
        /// </summary>
        public string GroupId { get; set; } = null!;
        /// <summary>
        /// 群組名稱
        /// </summary>
        public string GroupName { get; set; } = null!;
        /// <summary>
        /// 父節點
        /// </summary>
        public string? ParentId { get; set; }
        /// <summary>
        /// 層級
        /// </summary>
        public int? GroupLevel { get; set; }
        /// <summary>
        /// 是否顯示於&quot;功能權限申請&quot;裡
        /// </summary>
        public bool IsShowInApply { get; set; }
        /// <summary>
        /// 是否為預設群組
        /// 程式未開放修改，只能設定一筆
        /// </summary>
        public bool IsDefault { get; set; }
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

        public virtual ICollection<TbGroupRight> TbGroupRight { get; set; }
        public virtual ICollection<TbUserInGroup> TbUserInGroup { get; set; }
    }
}
