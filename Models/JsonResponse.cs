namespace BASE.Models
{
    public class JsonResponse<T>
    {
        public JsonResponse()
        {
            alert_plugin = "swal";
            alert_type = "success";
            IsSuccess = true;
        }

        /// <summary>執行結果</summary>
        public Boolean IsSuccess { get; set; }

        /// <summary>訊息</summary>
        public String? Message { get; set; }
        /// <summary>訊息詳情</summary>
        public String? MessageDetail { get; set; }

        /// <summary>回傳資料</summary>
        public T? Datas { get; set; }

        // alert套件，[swal, toastr]
        public string alert_plugin { get; set; }
        // 通知類型，[success, info, warning, error]
        public string alert_type { get; set; }
        // 重新導向 URL
        public string redirect_url { get; set; }
    }
}
