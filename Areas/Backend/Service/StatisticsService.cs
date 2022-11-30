using BASE.Areas.Backend.Models.Extend;
using BASE.Areas.Backend.Models;
using BASE.Models.DB;
using BASE.Service.Base;
using System.Linq.Expressions;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BASE.Areas.Backend.Service
{
    public class StatisticsService : ServiceBase
    {
        string _Msg = string.Empty;
        public StatisticsService(DBContext context) : base(context)
        {
        }

        /// <summary>
        /// 取得活動數據資料
        /// </summary>
        /// <returns></returns>
        public ChartInfo getActivityData(string id, string filter)
        {
            ChartInfo data = new ChartInfo();
            data.ChartValue = new List<int>();
            var registers = Lookup<TbActivityRegister>(ref _Msg, x => x.ActivityId == id).ToList();

            if (filter == "1")
            {
                var CompanyLocation = registers.Select(x => x.CompanyLocation).ToList();
                data.ChartData = CompanyLocation.Distinct().ToList();
                for (int i =0; i < data.ChartData.Count; i++)
                {
                    int cnt = registers.Where(x => x.CompanyLocation == data.ChartData[i]).Count();
                    data.ChartValue.Add(cnt);
                }
            }
            else if (filter == "2")
            {
                var CompanyType = registers.Select(x => x.CompanyType).ToList();
                data.ChartData = CompanyType.Distinct().ToList();
                for (int i = 0; i < data.ChartData.Count; i++)
                {
                    int cnt = registers.Where(x => x.CompanyType == data.ChartData[i]).Count();
                    data.ChartValue.Add(cnt);
                }
            }
            else if (filter == "3")
            {
                var CompanyEmpAmount = registers.Select(x => x.CompanyEmpAmount).ToList();
                data.ChartData = CompanyEmpAmount.Distinct().ToList();
                for (int i = 0; i < data.ChartData.Count; i++)
                {
                    int cnt = registers.Where(x => x.CompanyEmpAmount == data.ChartData[i]).Count();
                    data.ChartValue.Add(cnt);
                }
            }
            else if (filter == "4") 
            { 
                var InfoFrom = registers.Select(x => x.InfoFrom).ToList();
                data.ChartData = InfoFrom.Distinct().ToList();
                for (int i = 0; i < data.ChartData.Count; i++)
                {
                    int cnt = registers.Where(x => x.InfoFrom == data.ChartData[i]).Count();
                    data.ChartValue.Add(cnt);
                }
            }

            return data;
        }

        /// <summary>
        /// 取得諮詢輔導家數(透過年度資訊)
        /// </summary>
        /// <returns></returns>
        public int getTotalCount(int? Year)
        {
            int Data = 0;
            
            if (Year != 0)
            {
                Data = Lookup<TbConsultRegister>(ref _Msg, x => x.CreateDate.Year == Year).ToList().Count;              
            }

            return Data;
        }

        /// <summary>
        /// 取得活動數據資料
        /// </summary>
        /// <returns></returns>
        public ChartInfo getConsultData(int Year)
        {
            ChartInfo data = new ChartInfo();
            data.ChartData = new List<string>();
            data.ChartValue = new List<int>();
            data.ChartData.Add("組織經營");
            data.ChartData.Add("組織轉型");
            data.ChartData.Add("人才培育");
            data.ChartData.Add("職能分析");
            data.ChartData.Add("員工職涯發展");
            data.ChartData.Add("人力資源管理");
            data.ChartData.Add("勞資關係、法令");

            List<string> Filter = new List<string>();
            Filter.Add("Bacol00001");
            Filter.Add("Bacol00002");
            Filter.Add("Bacol00003");
            Filter.Add("Bacol00004");
            Filter.Add("Bacol00005");
            Filter.Add("Bacol00006");
            Filter.Add("Bacol00007");

            var registers = Lookup<TbConsultRegister>(ref _Msg, x => x.CreateDate.Year == Year).ToList();

            foreach(var item in Filter)
            {
                int cnt = 0;
                var cntVal = registers.Where(x => x.ConsultSubjects.Contains(item));
                if (cntVal != null)
                {
                    cnt = cntVal.Count();
                }
                
                data.ChartValue.Add(cnt);
            }

            return data;
        }

        #region 下拉選單
        /// <summary>
        /// 取得活動清單(透過年度資訊)
        /// </summary>
        /// <returns></returns>
        public List<SelectListItem> SetDDL_ActivityName(int? Year)
        {
            List<SelectListItem> Data = new List<SelectListItem>();
            Data.Add(new SelectListItem() { Text = "請選擇", Value = "" });

            if (Year != 0)
            {
                List<String> listID = Lookup<TbActivitySection>(ref _Msg, x => x.Day.Year == Year).Distinct().Select(x => x.ActivityId).ToList();
                List<TbActivity> ListData = Lookup<TbActivity>(ref _Msg, x => listID.Contains(x.Id)).ToList();

                foreach (var item in ListData)
                {
                    Data.Add(new SelectListItem() { Text = item.Title, Value = item.Id.ToString() });
                }
            }
            
            return Data;
        }

        #endregion
    }
}
