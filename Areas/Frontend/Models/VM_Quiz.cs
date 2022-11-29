using BASE.Areas.Frontend.Models.Extend;
using BASE.Models.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BASE.Areas.Frontend.Models
{
    public class VM_Quiz
    {
        public QuizExtend? ExtendItem { get; set; }

        //加密後報名編號
        public String RegisterID { get; set; }

        //加密後問卷編號
        public String QuizID { get; set; }

    }
}
