using BASE.Areas.Backend.Models.Extend;
using BASE.Areas.Backend.Models;
using BASE.Models.DB;
using BASE.Service.Base;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Win32;

namespace BASE.Areas.Backend.Service
{
    public class ConsultService : ServiceBase
    {
        string _Msg = string.Empty;
        public ConsultService(DBContext context) : base(context)
        {
        }
        /// <summary>
        /// 取得報名名單
        /// </summary>
        /// <returns></returns>
        public List<ConsultExtend>? GetConsultExtendList(ref String ErrMsg, VM_ConsultQueryParam? vmParam, Expression<Func<ConsultExtend, bool>>? filter = null)
        {
            try
            {
                List<ConsultExtend> dataList = (from consultRegister in _context.TbConsultRegister

                                                select new ConsultExtend
                                                {
                                                    ConsultRegister = consultRegister
                                                }).ToList();

                if (vmParam != null)
                {
                    foreach (var item in dataList)
                    {
                        // 諮詢主題
                        List<string> listSubjectKey = item.ConsultRegister.ConsultSubjects.Split(',').ToList();
                        List<string> listSubjectText = Lookup<TbBasicColumn>(ref _Msg, x => listSubjectKey.Contains(x.BacolId)).Select(x => x.Title).ToList();
                        string subjectText = "";
                        foreach (var itemSubject in listSubjectText)
                        {
                            subjectText = string.IsNullOrEmpty(subjectText) ? subjectText + itemSubject : subjectText + "," + itemSubject;
                        }
                        item.textOfSubject = subjectText;

                        // 顧問
                        List<string> listAdviserKey = new List<string>();
                        if (!string.IsNullOrEmpty(item.ConsultRegister.AssignAdviser1))
                            listAdviserKey.Add(item.ConsultRegister.AssignAdviser1);
                        if (!string.IsNullOrEmpty(item.ConsultRegister.AssignAdviser2))
                            listAdviserKey.Add(item.ConsultRegister.AssignAdviser2);
                        if (!string.IsNullOrEmpty(item.ConsultRegister.AssignAdviser3))
                            listAdviserKey.Add(item.ConsultRegister.AssignAdviser3);
                        List<string> listAdviserText = Lookup<TbUserInfo>(ref _Msg, x => listAdviserKey.Contains(x.UserId)).Select(x => x.UserName).ToList();
                        string adviserText = "";
                        foreach (var itemAdviser in listAdviserText)
                        {
                            adviserText = string.IsNullOrEmpty(adviserText) ? adviserText + itemAdviser : adviserText + "," + itemAdviser;
                        }
                        item.ConsultantList = adviserText;

                        //輔導助理
                        item.Assistant = Lookup<TbUserInfo>(ref _Msg, x => item.ConsultRegister.AssignAdviserAssistant == x.UserId).Select(x => x.UserName).FirstOrDefault();

                    }

                    if (!string.IsNullOrEmpty(vmParam.Keyword))
                    {
                        //關鍵字搜尋：企業名稱
                        dataList = dataList.Where(x => x.ConsultRegister.Name.Contains(vmParam.Keyword)).ToList();
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
        /// 取得詳細資訊
        /// </summary>
        /// <returns></returns>
        public ConsultExtend? GetConsultExtendItem(ref string ErrMsg, string id)
        {
            try
            {
                long CRId = Convert.ToInt64(id);
                ConsultExtend? dataItem = (from consultRegister in _context.TbConsultRegister

                                           join RequireSurveyFile in _context.TbFileInfo
                                           on consultRegister.RequireSurveyFile equals RequireSurveyFile.FileId into RequireSurveyFile1
                                           from RequireSurveyFile in RequireSurveyFile1.DefaultIfEmpty()

                                           join SatisfySurveyFile in _context.TbFileInfo
                                           on consultRegister.SatisfySurveyFile equals SatisfySurveyFile.FileId into SatisfySurveyFile1
                                           from SatisfySurveyFile in SatisfySurveyFile1.DefaultIfEmpty()

                                           where consultRegister.Id == CRId

                                           select new ConsultExtend
                                           {
                                               ConsultRegister = consultRegister,
                                               sApprove = consultRegister.IsApprove.HasValue ? (consultRegister.IsApprove.Value ? "Pass" : "Fail") : "-1",
                                               RequireSurveyFile = RequireSurveyFile,
                                               SatisfySurveyFile = SatisfySurveyFile
                                           }).FirstOrDefault();

                // 處理諮詢主題
                List<string> listSubject = dataItem.ConsultRegister.ConsultSubjects.Split(',').ToList();
                List<TbBasicColumn> datalist = Lookup<TbBasicColumn>(ref _Msg, x => x.BacolCode == "professionalField" && listSubject.Contains(x.BacolId)).ToList();

                string subjectTextList = "";
                foreach (var item in datalist)
                {
                    subjectTextList = string.IsNullOrEmpty(subjectTextList) ? subjectTextList + item.Title : subjectTextList + "," + item.Title;
                }
                dataItem.textOfSubject = subjectTextList;

                return dataItem;
            }
            catch (Exception ex)
            {
                ErrMsg = ex.ToString();
                return null;
            }
        }

        /// <summary>
        /// 取得歷史諮詢輔導報名列表
        /// </summary>
        /// <returns></returns>
        public List<CounselingHistoryExtend>? GetCounselingHistoryExtendList(ref string ErrMsg, string id)
        {
            try
            {
                List<CounselingHistoryExtend> dataList = (from consultRegister in _context.TbConsultRegister.Where(x => !string.IsNullOrEmpty(x.CounselingLogFile))

                                                          join FileInfo in _context.TbFileInfo
                                                          on consultRegister.CounselingLogFile equals FileInfo.FileId

                                                          where consultRegister.BusinessId == id

                                                          select new CounselingHistoryExtend
                                                          {
                                                              ConsultRegister = consultRegister,
                                                              FileInfo = FileInfo
                                                          }).OrderByDescending(x => x.ConsultRegister.CreateDate).ToList();

                return dataList;
            }
            catch (Exception ex)
            {
                ErrMsg = ex.ToString();
                return null;
            }
        }

        /// <summary>
        /// 取得諮詢輔導簽到表資料
        /// </summary>
        /// <returns></returns>
        public CounselingSigninExtend? GetCounselingSignin(ref string ErrMsg, string id)
        {
            try
            {
                CounselingSigninExtend dataItem = new CounselingSigninExtend();

                long longId = Convert.ToInt64(id);
                TbConsultRegister consultRegister = Lookup<TbConsultRegister>(ref _Msg, x => x.Id == longId).FirstOrDefault();

                dataItem.sheetName = consultRegister.ReAssignDate != null
                    ? consultRegister.ReAssignDate.Value.Month.ToString().PadLeft(2, '0') + consultRegister.ReAssignDate.Value.Day.ToString().PadLeft(2, '0')
                    : "尚未指派輔導時間";

                if (consultRegister.ReAssignDate != null)
                {
                    string sYear = (consultRegister.ReAssignDate.Value.Year - 1911).ToString();
                    string sMonth = (consultRegister.ReAssignDate.Value.Month).ToString();
                    string sDay = (consultRegister.ReAssignDate.Value.Day).ToString();
                    string sWeek = Week(consultRegister.ReAssignDate.Value);
                    dataItem.CounselingTime = string.Concat(sYear, "年", sMonth.PadLeft(2, '0'), "月", sDay.PadLeft(2, '0'), "日 (", sWeek, ") ", consultRegister.ReAssignTime);
                }
                if (!string.IsNullOrEmpty(consultRegister.ConsultAddress))
                {
                    dataItem.CounselingPlace = consultRegister.ConsultAddress;
                }

                // 塞入輔導對象
                List<CounselingObjectExtend> listObject = new List<CounselingObjectExtend>();
                CounselingObjectExtend tempObj = new CounselingObjectExtend();
                tempObj.Unit = consultRegister.Name;
                tempObj.Name = consultRegister.ContactName;
                tempObj.JobTitle = consultRegister.ContactJobTitle;
                listObject.Add(tempObj);

                // 取得顧問資料
                List<string> listConsultantID = new List<string>();
                if (!string.IsNullOrEmpty(consultRegister.AssignAdviser1))
                    listConsultantID.Add(consultRegister.AssignAdviser1);
                if (!string.IsNullOrEmpty(consultRegister.AssignAdviser2))
                    listConsultantID.Add(consultRegister.AssignAdviser2);
                if (!string.IsNullOrEmpty(consultRegister.AssignAdviser3))
                    listConsultantID.Add(consultRegister.AssignAdviser3);

                if (listConsultantID != null && listConsultantID.Any())
                {
                    List<TbUserInfo> listConsultant = Lookup<TbUserInfo>(ref _Msg, x => listConsultantID.Contains(x.UserId)).ToList();
                    foreach (var itemConsultant in listConsultant)
                    {
                        CounselingObjectExtend tempConsultant = new CounselingObjectExtend();
                        tempConsultant.Unit = itemConsultant.ServiceUnit;
                        tempConsultant.Name = itemConsultant.UserName;
                        tempConsultant.JobTitle = "顧問";
                        listObject.Add(tempConsultant);
                    }
                }
                // 取得輔導助理資料
                if (!string.IsNullOrEmpty(consultRegister.AssignAdviserAssistant))
                {
                    TbUserInfo assistantItem = Lookup<TbUserInfo>(ref _Msg, x =>x.UserId == consultRegister.AssignAdviserAssistant).FirstOrDefault();

                    CounselingObjectExtend tempAssistant = new CounselingObjectExtend();
                    tempAssistant.Unit = assistantItem.ServiceUnit;
                    tempAssistant.Name = assistantItem.UserName;
                    tempAssistant.JobTitle = "輔導助理";
                    listObject.Add(tempAssistant);
                }

                dataItem.listObject = listObject;

                return dataItem;
            }
            catch (Exception ex)
            {
                ErrMsg = ex.ToString();
                return null;
            }
        }

        #region 下拉選單

        /// <summary>
        /// 取得顧問列表
        /// </summary>
        /// <param name="Type">0: 無項目 1: 請選擇 2: 全部 3: 不拘</param>
        /// <returns></returns>
        public List<SelectListItem> SetDDL_Consult(int Type, string ConsultSubjects)
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

            // 取得權限為顧問的userID
            List<string> listUserIdInGroup = Lookup<TbUserInGroup>(ref _Msg, x => x.GroupId == "G000000004").Select(x => x.UserId).ToList();
            List<TbUserInfo> listUser = Lookup<TbUserInfo>(ref _Msg, x => listUserIdInGroup.Contains(x.UserId) && !x.IsDelete && x.IsActive).ToList();

            // 處理諮詢主題
            List<string> listSubject = ConsultSubjects.Split(',').ToList();

            // 找出符合諮詢主題的顧問
            foreach (var item in listSubject)
            {
                foreach (var userItem in listUser.Where(x => x.Skill.Contains(item)))
                {
                    Data.Add(new SelectListItem() { Text = userItem.UserName, Value = userItem.UserId });
                }
                listUser = listUser.Where(x => !x.Skill.Contains(item)).ToList();
            }

            return Data;
        }

        /// <summary>
        /// 取得輔導助理列表(彙館承辦)
        /// </summary>
        /// <param name="Type"></param>
        /// <returns></returns>
        public List<SelectListItem> SetDDL_Assistant(int Type)
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

            // 取得權限為彙館承辦的userID
            List<string> listUserIdInGroup = Lookup<TbUserInGroup>(ref _Msg, x => x.GroupId == "G000000002").Select(x => x.UserId).ToList();

            // 取得彙館承辦權限的帳號
            List<TbUserInfo> listUser = Lookup<TbUserInfo>(ref _Msg, x => listUserIdInGroup.Contains(x.UserId) && !x.IsDelete && x.IsActive).ToList();
            foreach (var item in listUser)
            {
                Data.Add(new SelectListItem() { Text = item.UserName, Value = item.UserId });
            }

            return Data;
        }

        /// <summary>
        /// 取得審核狀態
        /// </summary>
        /// <returns></returns>
        public List<SelectListItem> SetDDL_AuditStatus()
        {
            List<SelectListItem> Data = new List<SelectListItem>();
            Data.Add(new SelectListItem() { Text = "請選擇", Value = "-1" });
            Data.Add(new SelectListItem() { Text = "通過", Value = "1" });
            Data.Add(new SelectListItem() { Text = "不通過", Value = "2" });
            return Data;
        }

        /// <summary>
        /// 取得結案狀態
        /// </summary>
        /// <returns></returns>
        public List<SelectListItem> SetDDL_CloseStatus()
        {
            List<SelectListItem> Data = new List<SelectListItem>();
            Data.Add(new SelectListItem() { Text = "尚未結案", Value = "0" });
            Data.Add(new SelectListItem() { Text = "已結案", Value = "1" });
            return Data;
        }

        #endregion
        public string Week(DateTime ActDate)
        {
            string[] weekdays = { "星期日", "星期一", "星期二", "星期三", "星期四", "星期五", "星期六" };
            string week = weekdays[Convert.ToInt32(ActDate.DayOfWeek)];

            return week;
        }
    }
}
