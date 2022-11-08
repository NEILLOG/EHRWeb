using BASE.Models.DB;

namespace BASE.Areas.Frontend.Models.Extend
{
    public class NewsExtend
    {
        /// <summary>
        /// 最新消息
        /// </summary>
        public TbNews? Header { get; set; }

        /// <summary>
        /// 相片
        /// </summary>
        public TbFileInfo? FileInfo { get; set; }
    }
}
