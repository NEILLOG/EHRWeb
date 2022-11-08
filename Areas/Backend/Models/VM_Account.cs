using BASE.Areas.Backend.Models.Base;
using BASE.Areas.Backend.Models.Extend;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BASE.Areas.Backend.Models
{
    public class VM_Account
    {
        /// <summary>帳號管理列表</summary>
        public List<AccountExtend>? AccountExtendList { get; set; }

        /// <summary>帳號管理項目</summary>
        public AccountExtend? AccountExtendItem { get; set; }

        /// <summary>帳號管理_查詢參數</summary>
        public VM_AccountQueryParam Search { get; set; } = new VM_AccountQueryParam();
        
        /// <summary> 使用者操作歷程記錄 </summary>
        public List<OperateExtend>? OperateExtendList { get; set; }

        /// <summary> 編輯頁面使用的AUA8 </summary>
        public string editAua8 { get; set; }

        #region 下拉選單
        /// <summary> 群組 </summary>
        public List<SelectListItem> ddlGroup { get; set; }
        /// <summary> 專業領域 </summary>
        public List<SelectListItem> chbProfessionalField { get; set; }

        /// <summary> 操作帳號 </summary>
        public List<SelectListItem> ddlAccount { get; set; }

        #endregion
    }

    public class VM_AccountQueryParam : VM_BaseQueryParam
    {
        /// <summary>關鍵字</summary>
        public string? Keyword { get; set; }

        /// <summary> 群組 </summary>
        public string? sGroup { get; set; }

        /// <summary> 群組 </summary>
        public string? sProfessionalField { get; set; }

        /// <summary> 操作帳號 </summary>
        public string? sAccount { get; set; }

        /// <summary> 時間區間起 </summary>
        public string? sTime { get; set; }
        /// <summary> 時間區間訖 </summary>
        public string? eTime { get; set; }

    }
}
