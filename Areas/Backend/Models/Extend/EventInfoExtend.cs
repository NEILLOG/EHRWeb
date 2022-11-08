using BASE.Models.DB;

namespace BASE.Areas.Backend.Models.Extend
{
    public class EventInfoExtend
    {
        /// <summary> 活動主表 </summary>
        public TbActivity activity {get;set;}

        /// <summary> 活動報名場次列表 </summary>
        public List<SectionExtend> sectionExtendList { get; set; } = new List<SectionExtend>();

        /// <summary> 活動日期列表 </summary>
        public string activityDateList { get; set; }

    }

    /// <summary> 活動場次extend </summary>
    public class SectionExtend
    {
        /// <summary> 日期 </summary>
        public DateTime sectionDay { get; set; }
        /// <summary> 時段起 </summary>
        public DateTime startTime { get; set; }
        /// <summary> 時段訖 </summary>
        public DateTime endTime { get; set; }

        /// <summary> 活動參與模式 </summary>
        public string sectionType { get; set; }

    }
}
