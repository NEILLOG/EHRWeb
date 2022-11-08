namespace BASE.Areas.Backend.Models
{
    public class SessionStruct
    {
        public struct Login
        {
            ///<summary>目前登入使用者資訊</summary>
            public static string UserInfo = "UserInfo";
            ///<summary>目前登入使用者menu資訊</summary>
            public static string Menu = "Menu";
        }
    }

    public class UserSessionModel
    {
        /// <summary>
        /// 使用者編號
        /// </summary>
        public string UserID { get; set; } = null!;

        /// <summary> 
        /// 使用者名稱
        /// </summary>
        public string? UserName { get; set; }

        /// <summary>
        /// 群組ID
        /// </summary>
        public string? GroupID { get; set; }
    }
}
