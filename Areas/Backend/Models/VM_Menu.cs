using BASE.Areas.Backend.Models.Extend;

namespace BASE.Areas.Backend.Models
{
    public class VM_Menu
    {


        /// <summary>表單資料</summary>
        public List<TreeMenu> MenuList { get; set; }

        /// <summary>使用者資訊</summary>
        public UserExtend? UserExtendItem { get; set; }
    }

    public class TreeMenu
    {
        public TreeMenu()
        {
            MenuOpen = false;
        }

        /// <summary>MenuID</summary>
        public string MenuID { get; set; }

        /// <summary>上一層ID</summary>
        public string ParentID { get; set; }

        /// <summary>層級</summary>
        public int Level { get; set; }

        /// <summary>連結</summary>
        public String Url { get; set; }

        /// <summary>文字</summary>
        public String Title { get; set; }

        /// <summary>圖樣</summary>
        public String Icon { get; set; }

        /// <summary>標籤</summary>
        public String Tag { get; set; }


        /// <summary>目前選擇</summary>
        public bool Selected { get; set; }

        /// <summary>目錄狀態</summary>
        public bool MenuOpen { get; set; }

        public bool ShowInMenuSidebar { get; set; }

        ///// <summary>子連結清單</summary>
        //public List<TreeMenu> SubMenus { get; set; }



    }
}
