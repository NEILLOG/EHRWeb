using BASE.Areas.Backend.Models.Extend;
using BASE.Areas.Backend.Models;
using BASE.Models.DB;
using BASE.Service.Base;
using System.Linq.Expressions;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using NPOI.SS.Formula.Functions;

namespace BASE.Areas.Backend.Service
{
    public class EventService : ServiceBase
    {
        string _Msg = string.Empty;
        public EventService(DBContext context) : base(context)
        {
        }

        /// <summary>
        /// 列表
        /// </summary>
        /// <returns></returns>
        public List<EventInfoExtend>? GetEventInfoExtendList(ref String ErrMsg, VM_EventQueryParam? vmParam, Expression<Func<EventInfoExtend, bool>>? filter = null)
        {
            try
            {
                List<EventInfoExtend> dataList = (from activity in _context.TbActivity

                                                  where activity.IsDelete == false
                                                  select new EventInfoExtend
                                                  {
                                                      activity = activity,
                                                  }).ToList();

                // 取得活動日期字串
                foreach (var item in dataList)
                {
                    List<TbActivitySection> listActivitySection = new List<TbActivitySection>();
                    listActivitySection = Lookup<TbActivitySection>(ref _Msg, x => x.ActivityId == item.activity.Id).OrderBy(x=>x.Day).Distinct().ToList();
                    string strActivityDate = "";
                    foreach (var itemActivityDate in listActivitySection)
                    {
                        item.listActivitySection.Add(itemActivityDate);
                        strActivityDate = String.IsNullOrEmpty(strActivityDate) ? strActivityDate + itemActivityDate.Day.ToString("yyyy-MM-dd") : String.Concat(strActivityDate, "、", itemActivityDate.Day.ToString("yyyy-MM-dd"));
                    }
                    item.activityDateList = strActivityDate;
                }


                if (vmParam != null)
                {
                    if (!string.IsNullOrEmpty(vmParam.sCategory))
                    {
                        //關鍵字搜尋：類型
                        dataList = dataList.Where(x => x.activity.Category == vmParam.sCategory).ToList();
                    }

                    if (!string.IsNullOrEmpty(vmParam.sTitle))
                    {
                        //關鍵字搜尋：標題
                        dataList = dataList.Where(x => x.activity.Title.Contains(vmParam.sTitle)).ToList();
                    }

                    if (!string.IsNullOrEmpty(vmParam.sSubject))
                    {
                        //關鍵字搜尋：主題
                        dataList = dataList.Where(x => x.activity.Subject.Contains(vmParam.sSubject)).ToList();
                    }

                    // 日期搜尋僅有開始日期
                    if (!string.IsNullOrEmpty(vmParam.sTime) && string.IsNullOrEmpty(vmParam.eTime))
                    {
                        List<string> listContainActivity = new List<string>();
                        DateTime startDate = Convert.ToDateTime(vmParam.sTime);
                        
                        foreach (var item in dataList)
                        {
                            foreach (var itemSection in item.listActivitySection)
                            {
                                if (itemSection.Day >= startDate)
                                {
                                    listContainActivity.Add(item.activity.Id);
                                    break;
                                }
                            }
                        }

                        dataList = dataList.Where(x => listContainActivity.Contains(x.activity.Id)).ToList();
                    }

                    // 日期搜尋僅有結束日期
                    if (string.IsNullOrEmpty(vmParam.sTime) && !string.IsNullOrEmpty(vmParam.eTime))
                    {
                        List<string> listContainActivity = new List<string>();

                        DateTime endDate = Convert.ToDateTime(vmParam.eTime);
                        foreach (var item in dataList)
                        {
                            foreach (var itemSection in item.listActivitySection)
                            {
                                if (itemSection.Day <= endDate)
                                {
                                    listContainActivity.Add(item.activity.Id);
                                    break;
                                }
                            }
                        }
                        dataList = dataList.Where(x => listContainActivity.Contains(x.activity.Id)).ToList();

                    }

                    // 日期搜尋僅有結束日期
                    if (!string.IsNullOrEmpty(vmParam.sTime) && !string.IsNullOrEmpty(vmParam.eTime))
                    {
                        List<string> listContainActivity = new List<string>();

                        DateTime startDate = Convert.ToDateTime(vmParam.sTime);
                        DateTime endDate = Convert.ToDateTime(vmParam.eTime);
                        foreach (var item in dataList)
                        {
                            foreach (var itemSection in item.listActivitySection)
                            {
                                if (itemSection.Day >= startDate && itemSection.Day <= endDate)
                                {
                                    listContainActivity.Add(item.activity.Id);
                                    break;
                                }
                            }
                        }
                        dataList = dataList.Where(x => listContainActivity.Contains(x.activity.Id)).ToList();

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
        /// 取得活動訊息
        /// </summary>
        /// <param name="ErrMsg"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public EventInfoExtend? GetEventInfoExtendItem(ref string ErrMsg, string id)
        {
            try
            {
                EventInfoExtend? dataList = (from activity in _context.TbActivity

                                             where activity.IsDelete == false && activity.Id == id
                                             select new EventInfoExtend
                                             {
                                                 activity = activity
                                             }).FirstOrDefault();

                // 活動場次
                List<TbActivitySection> listSection = Lookup<TbActivitySection>(ref _Msg, x => x.ActivityId == id).ToList();
                foreach (var item in listSection)
                {
                    SectionExtend temp = new SectionExtend();
                    temp.sectionDay = item.Day;
                    temp.startTime = item.Day + item.StartTime;
                    temp.endTime = item.Day + item.EndTime;
                    temp.sectionType = item.SectionType;

                    dataList.sectionExtendList.Add(temp);
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
        /// 取得報名名單
        /// </summary>
        /// <returns></returns>
        public List<RegistrationExtend>? GetRegistrationExtendList(ref String ErrMsg, VM_EventQueryParam? vmParam, Expression<Func<RegistrationExtend, bool>>? filter = null)
        {
            try
            {
                long sectionId = Convert.ToInt64(vmParam.sSection);
                List<RegistrationExtend> dataList = (from register in _context.TbActivityRegister.Where(x => x.ActivityId == vmParam.activityId)

                                                     join registerSection in _context.TbActivityRegisterSection.Where(x => x.ActivityId == vmParam.activityId && x.RegisterSectionId == sectionId)
                                                      on register.Id equals registerSection.RegisterId

                                                     select new RegistrationExtend
                                                     {
                                                         register = register,
                                                         registerSection = registerSection,
                                                         verifyStatus = registerSection.IsValid.HasValue ? (registerSection.IsValid.Value ? "1" : "2") : "-1",
                                                     }).ToList();

                if (vmParam != null)
                {
                    if (!string.IsNullOrEmpty(vmParam.sCompanyName))
                    {
                        //關鍵字搜尋：企業名稱
                        dataList = dataList.Where(x => x.register.CompanyName.Contains(vmParam.sCompanyName)).ToList();
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
        /// 取得報名企業詳細資訊
        /// </summary>
        /// <returns></returns>
        public IQueryable<RegistrationExtend>? GetRegistrationExtendItem(ref string ErrMsg, string id)
        {
            try
            {
                long registrationId = Convert.ToInt64(id);
                IQueryable<RegistrationExtend> dataList = (from register in _context.TbActivityRegister

                                                           join registerSection in _context.TbActivityRegisterSection
                                                           on register.Id equals registerSection.RegisterId

                                                           join FileInfo in _context.TbFileInfo 
                                                           on register.FileIdHealth equals FileInfo.FileId into FileInfo1
                                                           from FileInfo in FileInfo1.DefaultIfEmpty()
                                                           where registerSection.Id == registrationId
                                                           select new RegistrationExtend
                                                           {
                                                               register = register,
                                                               registerSection = registerSection,
                                                               File_Health = FileInfo
                                                           });

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
        /// 取得類型列表
        /// </summary>
        /// <param name="Type">0: 無項目 1: 請選擇 2: 全部 3: 不拘</param>
        /// <returns></returns>
        public List<SelectListItem> SetDDL_Category(int Type)
        {
            List<SelectListItem> Data = new List<SelectListItem>();
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

            Data.Add(new SelectListItem() { Text = "課程", Value = "課程" });
            Data.Add(new SelectListItem() { Text = "講座", Value = "講座" });
            Data.Add(new SelectListItem() { Text = "活動", Value = "活動" });

            return Data;
        }

        /// <summary>
        /// 取得滿意度問卷
        /// </summary>
        /// <param name="Type">0: 無項目 1: 請選擇 2: 全部 3: 不拘</param>
        /// <returns></returns>
        public List<SelectListItem> SetDDL_quiz(int Type)
        {
            List<SelectListItem> Data = new List<SelectListItem>();
            List<TbQuiz> dataQuiz = Lookup<TbQuiz>(ref _Msg, x => x.IsDelete == false).ToList();
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

            foreach (var item in dataQuiz)
            {
                Data.Add(new SelectListItem() { Text = item.Name, Value = item.Id });
            }

            return Data;
        }

        /// <summary>
        /// 取得活動參與模式
        /// </summary>
        /// <param name="Type">0: 無項目 1: 請選擇 2: 全部 3: 不拘</param>
        /// <returns></returns>
        public List<SelectListItem> SetDDL_eventType(int Type)
        {
            List<SelectListItem> Data = new List<SelectListItem>();
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

            Data.Add(new SelectListItem() { Text = "實體", Value = "實體" });
            Data.Add(new SelectListItem() { Text = "線上", Value = "線上" });
            Data.Add(new SelectListItem() { Text = "實體+線上", Value = "實體+線上" });

            return Data;
        }

        /// <summary>
        /// 取得活動日期
        /// </summary>
        /// <param name="Type">0: 無項目 1: 請選擇 2: 全部 3: 不拘</param>
        /// <returns></returns>
        public List<SelectListItem> SetDDL_section(int Type, string activityId)
        {
            List<SelectListItem> Data = new List<SelectListItem>();
            List<TbActivitySection> listSection = Lookup<TbActivitySection>(ref _Msg, x => x.ActivityId == activityId).ToList();

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

            foreach (var item in listSection)
            {
                Data.Add(new SelectListItem() { Text = item.Day.Date.ToString("yyyy-MM-dd"), Value = item.Id.ToString() });
            }

            return Data;
        }

        /// <summary>
        /// 取得驗證狀態
        /// </summary>
        /// <returns></returns>
        public List<SelectListItem> SetDDL_VerifyStatus()
        {
            List<SelectListItem> Data = new List<SelectListItem>();
            Data.Add(new SelectListItem() { Text = "請選擇", Value = "-1" });
            Data.Add(new SelectListItem() { Text = "通過", Value = "1" });
            Data.Add(new SelectListItem() { Text = "不通過", Value = "2" });
            return Data;
        }

        #endregion
    }
}
