using BASE.Areas.Backend.Models.Extend;
using BASE.Models.DB;

namespace BASE.Areas.Backend.Models
{
    public class VM_Breadcrumb
    {
        public VM_Breadcrumb()
        {

        }

        /// <summary>麵包屑路徑</summary>
        public List<TreeMenu> BreadcrumbList { get; set; }

        /// <summary>選單名稱</summary>
        public TbMenuBack currentmenu { get; set; }



    }
}
