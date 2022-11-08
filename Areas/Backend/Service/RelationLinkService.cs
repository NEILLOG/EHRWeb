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
    public class RelationLinkService : ServiceBase
    {
        public RelationLinkService(DBContext context) : base(context)
        {
        }

        /// <summary>
        /// RelationLink列表
        /// </summary>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public IQueryable<RelationLinkExtend>? GetRelationLinkExtendList(ref String ErrMsg)
        {
            try
            {
                IQueryable<RelationLinkExtend> dataList = (from rel in _context.TbRelationLink
                                                           where rel.IsDelete == false
                                                           select new RelationLinkExtend
                                                           {
                                                               RelationLink = rel
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
        /// RelationLink列表
        /// </summary>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public int? GetRelationLinkMAXSort(ref String ErrMsg)
        {
            try
            {
                var dataList = (from rel in _context.TbRelationLink
                                where rel.IsDelete == false
                                select rel.Sort);

                return dataList.Max();
            }
            catch (Exception ex)
            {
                ErrMsg = ex.ToString();
                return null;
            }
        }

        /// <summary>
        /// RelationLink項目
        /// </summary>
        /// <param name="ErrMsg"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public IQueryable<RelationLinkExtend>? GetRelationLinkExtendItem(ref string ErrMsg, int id)
        {
            try
            {
                IQueryable<RelationLinkExtend>? dataList = (from rel in _context.TbRelationLink
                                                         where rel.IsDelete == false && rel.Id == id
                                                         select new RelationLinkExtend
                                                         {
                                                             RelationLink = rel
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
