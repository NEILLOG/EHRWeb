using BASE.Areas.Frontend.Models.Extend;
using BASE.Models.DB;
using BASE.Models.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BASE.Areas.Frontend.Models
{
    public class VM_QuizView
    {
        public TbQuiz Header { get; set; } = null!;

        public List<TbActivityQuizResponse> Responses { get; set; } = null!;

    }
}
