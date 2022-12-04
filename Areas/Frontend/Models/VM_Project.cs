using BASE.Areas.Frontend.Models.Extend;
using BASE.Models.DB;
using BASE.Models.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BASE.Areas.Frontend.Models
{
    public class VM_Project
    {
        /// <summary>列表_查詢參數</summary>
        public VM_ProjectQueryParam Search { get; set; } = new VM_ProjectQueryParam();
        /// <summary>列表</summary>
        public List<TbProject>? ExtendList { get; set; }
        /// <summary>項目</summary>
        public TbProject? ExtendItem { get; set; }


        public TbProjectModify? ModifyItem { get; set; }

        public IFormFile? ModifyFile { get; set; }

        /// <summary>是否顯示變更管理區塊</summary>
        public Boolean IsShowModifyArea { get; set; }
        /// <summary>
        /// 變更申請檔案下載連結
        /// </summary>
        public String SampleFilePath { get; set; }

    }

    /// <summary>列表_查詢參數</summary>
    public class VM_ProjectQueryParam : VM_BaseQueryParam
    {
        public VM_ProjectQueryParam()
        {
            PagerInfo.m_iPageCount = 9;
        }
        public string? Category { get; set; }
    }

}
