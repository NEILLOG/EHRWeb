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
        public IQueryable<ProjectModifyExtend>? GetProjectModifyExtendList(ref String ErrMsg, string id)
        {
            try
            {
                IQueryable<ProjectModifyExtend> dataList = (from ProjectModify in _context.TbProjectModify
                                                            join FileInfo in _context.TbFileInfo on ProjectModify.FileId equals FileInfo.FileId
                                                            into groupjoin from a in groupjoin.DefaultIfEmpty()
                                                            where ProjectModify.ProjectId == id
                                                            select new ProjectModifyExtend
                                                            {
                                                                ProjectModify = ProjectModify,
                                                                FilePath = a.FilePath,
                                                                FileName = a.FileName
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

        /// <summary>
        /// 取得檔案路徑
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string GetFilePath(string? id)
        {
            try
            {
                string strReturn = (from FileInfo in _context.TbFileInfo
                                    where FileInfo.FileId == id
                                    select FileInfo.FilePath).FirstOrDefault();

                return strReturn;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// 取得計畫所屬類別
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public TbProject GetProjectCATE(string? id)
        {
            try
            {
                var strReturn = (from Project in _context.TbProject
                                    where Project.Id == id
                                    select Project).FirstOrDefault();

                return strReturn;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
