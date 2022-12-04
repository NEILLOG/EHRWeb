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

        public struct VerifyCode
        {
            /// <summary>報名活動</summary>
            public static string Activity = "Activity";
            /// <summary>諮詢輔導</summary>
            public static string Consult = "Consult";
            /// <summary>訂閱服務</summary>
            public static string Subscript = "Subscript";
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
