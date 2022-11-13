using BASE.Areas.Backend.Models.Base;
using BASE.Areas.Backend.Models.Extend;
using BASE.Models.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BASE.Areas.Backend.Models
{
    public class VM_Project
    {
        /// <summary>FAQ_查詢參數</summary>
        public VM_ProjectQueryParam Search { get; set; } = new VM_ProjectQueryParam();

        /// <summary>Project列表</summary>
        public List<ProjectExtend>? ProjectExtendList { get; set; }

        /// <summary>Project項目</summary>
        public ProjectExtend? ProjectExtendItem { get; set; }

        /// <summary>OnePage項目</summary>
        public OnePageExtend? OnePageExtendItem { get; set; }

        /// <summary>排序ID List</summary>
        public List<string>? SortList { get; set; }

        /// <summary>六大計畫 List</summary>
        public List<string> MainProject = new List<string>() { "企業人力資源提升計畫", "充電起飛計畫", "充電再出發訓練計畫", "小型企業人力提升計畫", "在職中高齡者及高齡者穩定就業訓練補助實施計畫", "中高齡者退休後再就業準備訓練補助實施計畫" };

        /// <summary> 所屬類別 </summary>
        public List<SelectListItem> ddlCategory = new List<SelectListItem>() { new SelectListItem() { Text = "請選擇", Value = "" },
                                                                               new SelectListItem() { Text = "企業訓練資源", Value = "企業訓練資源" },
                                                                               new SelectListItem() { Text = "就業服務資源", Value = "就業服務資源" },
                                                                               new SelectListItem() { Text = "紓困資源", Value = "紓困資源" },
                                                                               new SelectListItem() { Text = "其他資源", Value = "其他資源" }};
        /// <summary> 聯繫窗口 </summary>
        public List<SelectListItem> ddlContact { get; set; }
}

    /// <summary>FAQ_查詢參數</summary>
    public class VM_ProjectQueryParam : VM_BaseQueryParam
    {
        /// <summary>關鍵字</summary>
        public string? Keyword { get; set; }

        /// <summary> 類型 </summary>
        public string? sCategory { get; set; }

        /// <summary> 計畫名稱 </summary>
        public string? sName { get; set; }

    }
}
