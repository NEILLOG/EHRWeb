using BASE.Areas.Backend.Models.Base;
using BASE.Areas.Backend.Models.Extend;
using BASE.Models.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BASE.Areas.Backend.Models
{
    public class VM_Quiz
    {
        public VM_QuizQueryParam Search { get; set; } = new VM_QuizQueryParam();

        public List<QuizExtend>? ExtendList { get; set; }
        public QuizExtend? ExtendItem { get; set; }

        /// <summary>排序ID List</summary>
        public List<string>? SortList { get; set; }

        public String? SelectedSampleQuizId { get; set; }

        public List<SelectListItem>? ddlExistQuiz { get; set; }
    }

    /// <summary>FAQ_查詢參數</summary>
    public class VM_QuizQueryParam : VM_BaseQueryParam
    {
        /// <summary>關鍵字</summary>
        public string? Keyword { get; set; }

        /// <summary> 時間區間起 </summary>
        public string? sTime { get; set; }
        /// <summary> 時間區間訖 </summary>
        public string? eTime { get; set; }

    }
}
