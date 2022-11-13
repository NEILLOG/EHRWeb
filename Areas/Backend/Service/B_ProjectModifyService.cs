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
    public class B_ProjectModifyService : ServiceBase
    {
        public B_ProjectModifyService(DBContext context) : base(context)
        {
        }

        /// <summary>
        /// ProjectModify列表
        /// </summary>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public IQueryable<ProjectModifyExtend>? GetProjectModifyExtendList(ref String ErrMsg ,string id)
        {
            try
            {
                IQueryable<ProjectModifyExtend> dataList = (from ProjectModify in _context.TbProjectModify
                                                            where ProjectModify.ProjectId == id
                                                            select new ProjectModifyExtend
                                                  {
                                                      ProjectModify = ProjectModify
                                                  });
                return dataList;
            }
            catch (Exception ex)
            {
                ErrMsg = ex.ToString();
                return null;
            }
        }


        /// <summary>
        /// ProjectModify項目
        /// </summary>
        /// <param name="ErrMsg"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public IQueryable<ProjectModifyExtend>? GetProjectModifyExtendItem(ref string ErrMsg, long id)
        {
            try
            {
                IQueryable<ProjectModifyExtend>? dataList = (from ProjectModify in _context.TbProjectModify
                                                         where ProjectModify.Id == id
                                                         select new ProjectModifyExtend
                                                         {
                                                             ProjectModify = ProjectModify
                                                         });

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
