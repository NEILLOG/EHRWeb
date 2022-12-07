using BASE.Areas.Backend.Models.Extend;
using BASE.Areas.Backend.Models;
using BASE.Models.DB;
using BASE.Service.Base;
using System.Linq.Expressions;
using System.Linq;
using BASE.Models.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using BASE.Service;
using Microsoft.EntityFrameworkCore;

namespace BASE.Areas.Backend.Service
{
    public class MemberService : ServiceBase
    {
        string _Msg = string.Empty;
        public MemberService(DBContext context) : base(context)
        {
        }

        /// <summary>
        /// 帳號列表
        /// </summary>
        /// <returns></returns>
        public IQueryable<MemberExtend>? GetAccountExtendList(ref String ErrMsg, VM_MemberQueryParam? vmParam, Expression<Func<MemberExtend, bool>>? filter = null)
        {
            try
            {
                IQueryable<MemberExtend> dataList = (from account in _context.TbUserInfo

                                                      join gp in _context.TbGroupInfo
                                                      on account.TbUserInGroup.FirstOrDefault().GroupId equals gp.GroupId

                                                      where account.IsDelete == false
                                                      select new MemberExtend
                                                      {
                                                          userinfo = account,
                                                          userinfoGroup = account.TbUserInGroup.FirstOrDefault(),
                                                          groupInfo = gp
                                                      });

                if (filter != null)
                {
                    dataList = dataList.Where(filter);
                }

                if (vmParam != null)
                {
                    if (!string.IsNullOrEmpty(vmParam.Keyword))
                    {
                        //關鍵字搜尋：使用者名稱
                        dataList = dataList.Where(x => x.userinfo.UserName.Contains(vmParam.Keyword));
                    }

                    if (!string.IsNullOrEmpty(vmParam.sGroup))
                    {
                        //關鍵字搜尋：群組
                        dataList = dataList.Where(x => x.userinfo.TbUserInGroup.FirstOrDefault().GroupId == vmParam.sGroup);
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


        /// <summary>
        ///帳號項目
        /// </summary>
        /// <param name="ErrMsg"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public IQueryable<MemberExtend>? GetAccountExtendItem(ref string ErrMsg, string id)
        {
            try
            {
                IQueryable<MemberExtend>? dataList = (from account in _context.TbUserInfo

                                                       join gp in _context.TbGroupInfo
                                                      on account.TbUserInGroup.FirstOrDefault().GroupId equals gp.GroupId

                                                       where account.IsDelete == false && account.UserId == id
                                                       select new MemberExtend
                                                       {
                                                           userinfo = account,
                                                           userinfoGroup = account.TbUserInGroup.FirstOrDefault(),
                                                           groupInfo = gp
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
        /// 使用者操作歷程記錄
        /// </summary>
        /// <returns></returns>
        public IQueryable<OperateExtend>? GetOperateExtendList(ref String ErrMsg, VM_MemberQueryParam? vmParam, Expression<Func<OperateExtend, bool>>? filter = null)
        {
            try
            {
                IQueryable<OperateExtend> dataList = (from operateLog in _context.TbBackendOperateLog

                                                      join userInfo in _context.TbUserInfo.Where(x=>x.IsDelete != true)
                                                      on operateLog.UserId equals userInfo.UserId
                                                      where operateLog.Action != "檢視"
                                                      select new OperateExtend
                                                      {
                                                          operateLog  = operateLog,
                                                          User = userInfo
                                                      });
                if (filter != null)
                {
                    dataList = dataList.Where(filter);
                }

                if (vmParam != null)
                {
                    if (!string.IsNullOrEmpty(vmParam.sMember))
                    {
                        //關鍵字搜尋：操作帳號
                        dataList = dataList.Where(x => x.User.UserId.Contains(vmParam.sMember));
                    }

                    if (!string.IsNullOrEmpty(vmParam.sTime))
                    {
                        //關鍵字搜尋：時間起
                        DateTime startTime = Convert.ToDateTime(vmParam.sTime);
                        dataList = dataList.Where(x => x.operateLog.CreateDate >= startTime);
                    }

                    if (!string.IsNullOrEmpty(vmParam.eTime))
                    {
                        //關鍵字搜尋：時間訖
                        DateTime endTime = Convert.ToDateTime(vmParam.eTime).AddDays(1);
                        dataList = dataList.Where(x => x.operateLog.CreateDate <= endTime);
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


        #region 下拉選單

        /// <summary>
        /// 取得群組列表
        /// </summary>
        /// <param name="Type">0: 無項目 1: 請選擇 2: 全部 3: 不拘</param>
        /// <returns></returns>
        public List<SelectListItem> SetDDL_Group(int Type,string UserGroupID = "")
        {
            List<SelectListItem> Data = new List<SelectListItem>();
            List<TbGroupInfo> listGroup = Lookup<TbGroupInfo>(ref _Msg).Where(x => x.IsShowInApply).ToList();

            if (UserGroupID == "G000000001")
            {
                Data.Add(new SelectListItem() { Text = "分署承辦(系統管理者)", Value = "G000000001" });
                return Data;
            }
            switch (Type)
            {
                case 1:
                    Data.Add(new SelectListItem() { Text = "--- 請選擇 ---", Value = "" });
                    break;
                case 2:
                    Data.Add(new SelectListItem() { Text = "--- 全部 ---", Value = "" });
                    break;
                case 3:
                    Data.Add(new SelectListItem() { Text = "不拘", Value = "" });
                    break;
                default:
                    break;
            }

            foreach (var item in listGroup)
            {
                Data.Add(new SelectListItem() { Text = item.GroupName, Value = item.GroupId });
            }

            return Data;
        }

        /// <summary>
        /// 取得專業領域列表
        /// </summary>
        /// <param name="Type">0: 無項目 1: 請選擇 2: 全部 3: 不拘</param>
        /// <returns></returns>
        public List<SelectListItem> SetDDL_ProfessionalField(int Type)
        {
            List<SelectListItem> Data = new List<SelectListItem>();
            List<TbBasicColumn> listProfessionalFiel = Lookup<TbBasicColumn>(ref _Msg).Where(x => x.BacolCode == "professionalField" && x.IsActive && x.LangId == "tw").OrderBy(x => x.Order).ToList();

            switch (Type)
            {
                case 1:
                    Data.Add(new SelectListItem() { Text = "--- 請選擇 ---", Value = "" });
                    break;
                case 2:
                    Data.Add(new SelectListItem() { Text = "--- 全部 ---", Value = "" });
                    break;
                case 3:
                    Data.Add(new SelectListItem() { Text = "不拘", Value = "" });
                    break;
                default:
                    break;
            }

            foreach (var item in listProfessionalFiel)
            {
                Data.Add(new SelectListItem() { Text = item.Title, Value = item.BacolId });
            }

            return Data;
        }

        /// <summary>
        /// 取得帳號列表
        /// </summary>
        /// <param name="Type">0: 無項目 1: 請選擇 2: 全部 3: 不拘</param>
        /// <returns></returns>
        public List<SelectListItem> SetDDL_Account(int Type)
        {
            List<SelectListItem> Data = new List<SelectListItem>();
            List<TbUserInfo> listUserInfo = Lookup<TbUserInfo>(ref _Msg).Where(x => !x.IsDelete).ToList();

            switch (Type)
            {
                case 1:
                    Data.Add(new SelectListItem() { Text = "--- 請選擇 ---", Value = "" });
                    break;
                case 2:
                    Data.Add(new SelectListItem() { Text = "--- 全部 ---", Value = "" });
                    break;
                case 3:
                    Data.Add(new SelectListItem() { Text = "不拘", Value = "" });
                    break;
                default:
                    break;
            }

            foreach (var item in listUserInfo)
            {
                Data.Add(new SelectListItem() { Text = item.Account, Value = item.UserId });
            }

            return Data;
        }

        #endregion
    }
}
