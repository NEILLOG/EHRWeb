using System;
using System.Collections.Generic;

namespace BASE.Models.DB
{
    /// <summary>
    /// 後台選單
    /// </summary>
    public partial class TbMenuBack
    {
        public TbMenuBack()
        {
            TbGroupRight = new HashSet<TbGroupRight>();
            TbUserRight = new HashSet<TbUserRight>();
        }

        /// <summary>
        /// 選單代碼
        /// </summary>
        public string MenuId { get; set; } = null!;
        /// <summary>
        /// 名稱
        /// </summary>
        public string MenuName { get; set; } = null!;
        /// <summary>
        /// 父階層
        /// </summary>
        public string MenuParent { get; set; } = null!;
        /// <summary>
        /// 超連結
        /// </summary>
        public string MenuUrl { get; set; } = null!;
        /// <summary>
        /// 層級
        /// </summary>
        public int MenuLevel { get; set; }
        /// <summary>
        /// 排序
        /// </summary>
        public int MenuOrder { get; set; }
        /// <summary>
        /// 是否刪除
        /// </summary>
        public bool IsDelete { get; set; }
        /// <summary>
        /// 是否顯示在左側選單
        /// </summary>
        public bool ShowInMainSideBar { get; set; }
        /// <summary>
        /// 是否針對此功能項設定權限
        /// </summary>
        public bool SettingFlag { get; set; }
        /// <summary>
        /// 該功能是否有&quot;檢視&quot;功能
        /// </summary>
        public bool ViewEnabled { get; set; }
        /// <summary>
        /// 該功能是否有&quot;新增&quot;功能
        /// </summary>
        public bool AddEnabled { get; set; }
        /// <summary>
        /// 該功能是否有&quot;修改&quot;功能
        /// </summary>
        public bool ModifyEnabled { get; set; }
        /// <summary>
        /// 該功能是否有&quot;刪除&quot;功能
        /// </summary>
        public bool DeleteEnabled { get; set; }
        /// <summary>
        /// 該功能是否有&quot;下載&quot;功能
        /// </summary>
        public bool DownloadEnabled { get; set; }
        /// <summary>
        /// 該功能是否有&quot;上傳&quot;功能
        /// </summary>
        public bool UploadEnabled { get; set; }
        /// <summary>
        /// 選單小圖示
        /// </summary>
        public string? Icon { get; set; }
        /// <summary>
        /// 標籤名稱
        /// </summary>
        public string? TagName { get; set; }
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
        public virtual ICollection<TbUserRight> TbUserRight { get; set; }
    }
}
