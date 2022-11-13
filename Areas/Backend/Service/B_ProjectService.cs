using BASE.Areas.Backend.Models;
using BASE.Areas.Backend.Models.Extend;
using BASE.Extensions;
using BASE.Models.DB;
using BASE.Models.Enums;
using BASE.Service.Base;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BASE.Areas.Backend.Service
{
    public class B_ProjectService : ServiceBase
    {
        public B_ProjectService(DBContext context) : base(context)
        {
        }

        /// <summary>
        /// Project列表
        /// </summary>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public IQueryable<ProjectExtend>? GetProjectExtendList(ref String ErrMsg)
        {
            try
            {
                IQueryable<ProjectExtend> dataList = (from Project in _context.TbProject
                                                      where Project.IsDelete == false
                                                      select new ProjectExtend
                                                      {
                                                          Project = Project
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
        /// Project項目
        /// </summary>
        /// <param name="ErrMsg"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public IQueryable<ProjectExtend>? GetProjectExtendItem(ref string ErrMsg, string id)
        {
            try
            {
                IQueryable<ProjectExtend>? dataList = (from Project in _context.TbProject
                                                       where Project.IsDelete == false && Project.Id == id
                                                       select new ProjectExtend
                                                       {
                                                           Project = Project
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
        /// 取得聯繫窗口清單
        /// </summary>
        /// <returns></returns>
        public List<SelectListItem> SetDDL_Contact()
        {
            string _Msg = string.Empty;
            List<SelectListItem> Data = new List<SelectListItem>();
            List<TbUserInfo> listUser = (from User in _context.TbUserInfo
                                         join UserGroup in _context.TbUserInGroup on User.UserId equals UserGroup.UserId
                                         join Group in _context.TbGroupInfo on UserGroup.GroupId equals Group.GroupId
                                         where Group.GroupName == "彙管承辦"
                                         select User).ToList();
            Data.Add(new SelectListItem() { Text = "請選擇", Value = "" });


            foreach (var item in listUser)
            {
                Data.Add(new SelectListItem() { Text = item.UserName, Value = item.UserId });
            }

            return Data;
        }

        /// <summary>
        /// OnePage項目
        /// </summary>
        /// <param name="ErrMsg"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public IQueryable<OnePageExtend>? GetOnePageExtendItem(ref string ErrMsg, string id)
        {
            try
            {
                IQueryable<OnePageExtend>? dataList = (from OnePage in _context.TbOnePage
                                                       where  OnePage.Id == id
                                                       select new OnePageExtend
                                                       {
                                                           OnePage = OnePage
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
        /// OnePage項目
        /// </summary>
        /// <param name="ErrMsg"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public string GetOnePageId(ref string ErrMsg, string Description)
        {
            try
            {
                string strId = (from OnePage in _context.TbOnePage
                                   where OnePage.Description == Description
                                   select OnePage.Id).FirstOrDefault().ToString();

                return strId;
            }
            catch (Exception ex)
            {
                ErrMsg = ex.ToString();
                return null;
            }
        }

    }
}
