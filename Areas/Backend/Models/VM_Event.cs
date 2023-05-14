using BASE.Areas.Backend.Models.Base;
using BASE.Areas.Backend.Models.Extend;
using BASE.Models.DB;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BASE.Areas.Backend.Models
{
    public class VM_Event
    {
        /// <summary> 活動訊息extend </summary>
        public EventInfoExtend EventInfoItem { get; set; }
        
        /// <summary> 活動訊息列表 </summary>
        public List<EventInfoExtend> EventInfoList { get; set; }

        /// <summary>活動管理_查詢參數</summary>
        public VM_EventQueryParam Search { get; set; } = new VM_EventQueryParam();

        /// <summary> 活動訊息主表 </summary>
        public TbActivity ActivityItem { get; set; }

        /// <summary> 活動報名名單 </summary>
        public List<RegistrationExtend> RegistrationExtendList { get; set; }


        /// <summary> 報名企業詳細資訊 </summary>
        public RegistrationExtend RegistrationExtendItem { get; set; }

        /// <summary> 群組Id </summary>
        public string GroupId { get; set; }

        /// <summary>
        /// 是否填寫問卷
        /// </summary>
        public bool isFillQuiz { get; set; }

        #region 下拉選單
        /// <summary> 類型 </summary>
        public List<SelectListItem> ddlCategory { get; set; }

        /// <summary> 滿意度問卷 </summary>
        public List<SelectListItem> ddlQuiz { get; set; }

        /// <summary> 活動參與模式 </summary>
        public List<SelectListItem> ddlEventType { get; set; }

        /// <summary> 活動時段 </summary>
        public List<SelectListItem> ddlSection { get; set; }

        /// <summary> 審核狀態 </summary>
        public List<SelectListItem> ddlVerify { get; set; }

        #endregion

        #region 檔案

        /// <summary>活動圖片</summary>
        public IFormFile? ActivityImageFile { get; set; }

        /// <summary>上傳行前通知信壓縮檔(for實體參與者)</summary>
        public IFormFile? EntityFile { get; set; }

        /// <summary>上傳行前通知信壓縮檔(for線上參與者)</summary>
        public IFormFile? OnlineFile { get; set; }

        /// <summary>講義</summary>
        public IFormFile? HandoutFile { get; set; }

        /// <summary>講義(刪除用)</summary>
        public List<string>? DelFileList { get; set; }

        #endregion
    }

    public class VM_EventQueryParam : VM_BaseQueryParam
    {
        /// <summary>關鍵字</summary>
        public string? Keyword { get; set; }

        /// <summary> 類型 </summary>
        public string? sCategory { get; set; }

        /// <summary> 標題 </summary>
        public string? sTitle { get; set; }

        /// <summary> 主題 </summary>
        public string? sSubject { get; set; }

        /// <summary> 時間區間起 </summary>
        public string? sTime { get; set; }
        /// <summary> 時間區間訖 </summary>
        public string? eTime { get; set; }


        /// <summary> 活動訊息主表ID </summary>
        public string activityId { get; set; }

        /// <summary> 活動時段 </summary>
        public string sSection { get; set; }

        /// <summary> 企業名稱 </summary>
        public string sCompanyName { get; set; }

    }
}
