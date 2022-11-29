using BASE.Areas.Backend.Models;
using BASE.Areas.Backend.Models.Extend;
using BASE.Extensions;
using BASE.Models.DB;
using BASE.Models.Enums;
using BASE.Service.Base;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BASE.Areas.Backend.Service
{
    public class QuizService : ServiceBase
    {
        public QuizService(DBContext context) : base(context)
        {
        }

        public IQueryable<QuizExtend>? GetExtendList(ref String ErrMsg, VM_QuizQueryParam? vmParam, Expression<Func<QuizExtend, bool>>? filter = null)
        {
            try
            {
                IQueryable<QuizExtend> dataList = (from quiz in _context.TbQuiz
                                                   where quiz.IsDelete == false
                                                   select new QuizExtend
                                                   {
                                                       Header = quiz,
                                                       Lines = _context.TbQuizOption.Where(x => x.QuizId == quiz.Id).ToList()
                                                   });

                if (filter != null)
                {
                    dataList = dataList.Where(filter);
                }

                if (vmParam != null)
                {
                    if (!string.IsNullOrEmpty(vmParam.Keyword))
                    {
                        dataList = dataList.Where(x => x.Header.Name.Contains(vmParam.Keyword));
                    }
                }
                return dataList;
            }
            catch (Exception ex)
            {
                ErrMsg = ex.ToString();
                return null;
            }
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
