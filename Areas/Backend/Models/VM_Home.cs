namespace BASE.Areas.Backend.Models
{
    public class VM_Home
    {
        /// <summary>帳號</summary>
        public string? acct { get; set; }

        /// <summary>密碼</summary>
        public string? aua8 { get; set; }

        /// <summary>驗證碼</summary>
        public string? captcha { get; set; }

        /// <summary>上傳路徑</summary>
        public string? uploadurl { get; set; }
    }
}
