using BASE.Areas.Backend.Models.Base;
using BASE.Areas.Backend.Models.Extend;
using BASE.Models.DB;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BASE.Areas.Backend.Models
{
    public class VM_Consult
    {
        /// <summary> 諮詢輔導服務介紹 </summary>
        public TbOnePage consultIntro { get; set; }

        /// <summary> 諮詢輔導列表 </summary>
        public List<ConsultExtend> ConsultExtendList { get; set; }

        /// <summary> 諮詢輔導項目 </summary>
        public ConsultExtend ConsultExtendItem { get; set; }

        /// <summary> 歷史輔導紀錄列表 </summary>
        public List<CounselingHistoryExtend> CounselingHistoryExtendList { get; set; }

        /// <summary>諮詢輔導服務_查詢參數</summary>
        public VM_ConsultQueryParam Search { get; set; } = new VM_ConsultQueryParam();
        
        /// <summary> 群組Id </summary>
        public string GroupId { get; set; }

        /// <summary> 諮詢輔導報名ID </summary>
        public string ConsultRegisterId { get; set; }

        #region 下拉選單
        /// <summary> 顧問清單 </summary>
        public List<SelectListItem> ddlConsult { get; set; }

        /// <summary> 助理清單 </summary>
        public List<SelectListItem> ddlAssistant { get; set; }

        /// <summary> 審核狀態 </summary>
        public List<SelectListItem> ddlAudit { get; set; }

        /// <summary> 結案狀態 </summary>
        public List<SelectListItem> ddlClose { get; set; }
        #endregion

        #region 檔案

        /// <summary> 輔導紀錄 </summary>
        public IFormFile? CounselingLogFile { get; set; }

        /// <summary> 已填寫完之簽到表 </summary>
        public IFormFile? SigninFormFile { get; set; }

        #endregion
    }

    public class VM_ConsultQueryParam : VM_BaseQueryParam
    {
        /// <summary>關鍵字</summary>
        public string? Keyword { get; set; }
    }
}
