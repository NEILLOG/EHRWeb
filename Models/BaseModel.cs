namespace BASE.Models
{
    public class BaseModel
    {
        /// <summary>
        /// 鍵
        /// </summary>
        public object Key { get; set; } = null!;

        /// <summary>
        /// 顯示文字
        /// </summary>
        public object Text { get; set; } = null!;

        /// <summary>
        /// 值
        /// </summary>
        public string Value { get; set; } = null!;

        /// <summary>
        /// 排序
        /// </summary>
        public int? Order { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public bool IsSelected { get; set; }
    }
}
