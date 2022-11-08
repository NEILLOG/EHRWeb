namespace BASE.Models
{
    public class ActionResultModel<T>
    {
        public ActionResultModel()
        {
            IsSuccess = true;
        }

        /// <summary>是否成功</summary>
        public bool IsSuccess { get; set; }

        /// <summary>回傳描述（使用者看到訊息）</summary>
        public string Description { get; set; } = null!;

        /// <summary>錯誤訊息（開發者看到訊息）</summary>
        public string? Message { get; set; }
        
        /// <summary>物件</summary>
        public T? Data { get; set; }
    }
}
