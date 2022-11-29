using BASE.Areas.Backend.Models.Base;
using BASE.Models.DB;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BASE.Areas.Backend.Models
{
    public class VM_Promote
    {
        /// <summary> 推廣管理列表 </summary>
        public List<TbPromotion> PromotionList { get; set; }


        /// <summary>諮詢輔導服務_查詢參數</summary>
        public VM_PromoteQueryParam Search { get; set; } = new VM_PromoteQueryParam();

        #region 下拉選單
        /// <summary> 企業人數清單 </summary>
        public List<SelectListItem> ddlNumCompanies { get; set; }

        /// <summary> 計畫清單 </summary>
        public List<SelectListItem> ddlPlan { get; set; }

        /// <summary> 編輯信件:主旨 </summary>
        public string MailSubject { get; set; }

        /// <summary> 編輯信件:內容 </summary>
        public string MailContent { get; set; }

        #endregion

        #region 檔案
        /// <summary>檔案上傳</summary>
        public IFormFile ImportFile { get; set; }

        #endregion
    }
    public class VM_PromoteQueryParam : VM_BaseQueryParam
    {
        /// <summary>企業人數</summary>
        public string? NumCompanies { get; set; }
        /// <summary>計畫</summary>
        public string? Plan { get; set; }
    }
}
