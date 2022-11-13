using BASE.Areas.Frontend.Models.Extend;
using BASE.Models.DB;
using BASE.Models.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BASE.Areas.Frontend.Models
{
    public class VM_HRPackage
    {
        /// <summary>列表_查詢參數</summary>
        public VM_HRPackageQueryParam Search { get; set; } = new VM_HRPackageQueryParam();
        /// <summary>列表</summary>
        public List<HRPackageExtend>? ExtendList { get; set; }
    }

    /// <summary>列表_查詢參數</summary>
    public class VM_HRPackageQueryParam : VM_BaseQueryParam
    {
        public VM_HRPackageQueryParam()
        {
            PagerInfo.m_iPageCount = 9;
        }

        public string? Keyword { get; set; }
    }

}
