using BASE.Models.DB;

namespace BASE.Areas.Backend.Models.Extend
{
    public class UserExtend
    {


        /// <summary>使用者資訊</summary>
        public TbUserInfo User { get; set; }

        /// <summary>照片</summary>
        public TbFileInfo UserPhoto { get; set; }


    }
}
