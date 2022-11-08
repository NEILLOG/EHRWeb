namespace BASE.Areas.Backend.Models
{
    public class CK5_UploadResponse
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool uploaded { get; set; }

        /// <summary>
        /// 圖片路徑
        /// </summary>
        public string url { get; set; }

        /// <summary>
        /// 錯誤訊息
        /// </summary>
        public CK5_UploadError error { get; set; }
    };

    public class CK5_UploadError
    {
        public string message { get; set; }
    }

}
