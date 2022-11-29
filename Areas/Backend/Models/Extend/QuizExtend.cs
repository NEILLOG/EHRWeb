using BASE.Models.DB;
using BASE.Models.Enums;
using BASE.Extensions;

namespace BASE.Areas.Backend.Models.Extend
{
    public class QuizExtend
    {
        public TbQuiz Header { get; set; } = null!;

        public List<TbQuizOption> Lines { get; set; } = null!;
    }
}
