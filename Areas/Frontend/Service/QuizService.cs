using BASE.Areas.Frontend.Models;
using BASE.Areas.Frontend.Models.Extend;
using BASE.Models.DB;
using BASE.Models.Enums;
using BASE.Service;
using BASE.Service.Base;
using Microsoft.EntityFrameworkCore;

namespace BASE.Areas.Frontend.Service
{
    public class QuizService : ServiceBase
    {
        private readonly IConfiguration _conf;
        private readonly AllCommonService _allCommonService;
        public QuizService(DBContext context,
            AllCommonService allCommonService,
            IConfiguration configuration) : base(context)
        {
            _conf = configuration;
            _allCommonService = allCommonService;
        }

        public QuizExtend? GetExtendItem(ref string ErrMsg, string id)
        {
            try
            {
                IQueryable<QuizExtend>? dataList = (from quiz in _context.TbQuiz
                                                    where quiz.IsDelete == false && quiz.Id == id
                                                    select new QuizExtend
                                                    {
                                                        Header = quiz,
                                                        Lines = _context.TbQuizOption.Where(x => x.QuizId == quiz.Id).OrderBy(x => x.Sort).ToList()
                                                    });

                return dataList.FirstOrDefault();
            }
            catch (Exception ex)
            {
                ErrMsg = ex.ToString();
                return null;
            }
        }

        public IQueryable<TbQuizOption>? GetExistOptions(ref string ErrMsg, string id)
        {
            try
            {
                IQueryable<TbQuizOption>? dataList = (from option in _context.TbQuizOption
                                                      where option.QuizId.Equals(id)
                                                      select option);

                return dataList;
            }
            catch (Exception ex)
            {
                ErrMsg = ex.ToString();
                return null;
            }
        }

    }
}
