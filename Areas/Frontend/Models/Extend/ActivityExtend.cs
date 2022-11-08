using BASE.Models.DB;

namespace BASE.Areas.Frontend.Models.Extend
{
    public class ActivityExtend
    {
        /// <summary>表頭</summary>
        public TbActivity? Header { get; set; }

        /// <summary>報名時段</summary>
        public List<TbActivitySection>? Sections { get; set; }

        /// <summary>活動照片</summary>
        public TbFileInfo? FileInfo { get; set; }
    }
}
