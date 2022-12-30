using BASE.Areas.Backend.Models.Extend;
using BASE.Areas.Backend.Models;
using BASE.Models.DB;
using BASE.Service.Base;
using System.Linq.Expressions;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

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
            data.ChartData = new List<string>();
            var registers = Lookup<TbActivityRegister>(ref _Msg, x => x.ActivityId == id).ToList();

            if (filter == "1")
            {
                data.ChartData.Add("桃園市");
                data.ChartData.Add("新竹市");
                data.ChartData.Add("新竹縣");
                data.ChartData.Add("苗栗縣");
                data.ChartData.Add("其他地區");

                foreach (var item in data.ChartData)
                {
                    int cnt = registers.Where(x => x.CompanyLocation == item).Count();
                    data.ChartValue.Add(cnt);
                }
            }
            else if (filter == "2")
            {
                data.ChartData.Add("A大類「農、林、魚、牧業」");
                data.ChartData.Add("B大類「礦業及土石採取業」");
                data.ChartData.Add("C大類「製造業」");
                data.ChartData.Add("D大類「電力及燃氣供應業」");
                data.ChartData.Add("E大類「用水供應及汙染整治業」");
                data.ChartData.Add("F大類「營建工程業」");
                data.ChartData.Add("G大類「批發及零售業」");
                data.ChartData.Add("H大類「運輸及倉儲業」");
                data.ChartData.Add("I大類「住宿及餐飲業」");
                data.ChartData.Add("J大類「出版、影音製作、傳播及資通訊服務業」");
                data.ChartData.Add("K大類「金融及保險業」");
                data.ChartData.Add("L大類「不動產業」");
                data.ChartData.Add("M大類「專業、科學及技術服務業」");
                data.ChartData.Add("N大類「支援服務業」");
                data.ChartData.Add("O大類「公共行政及國防；強制性社會安全」");
                data.ChartData.Add("P大類「教育業」");
                data.ChartData.Add("Q大類「醫療保健及社會工作服務業」");
                data.ChartData.Add("R大類「藝術、娛樂及休閒服務業」");
                data.ChartData.Add("S大類「其他服務業」");

                foreach (var item in data.ChartData)
                {
                    int cnt = registers.Where(x => x.CompanyType == item).Count();
                    data.ChartValue.Add(cnt);
                }
            }
            else if (filter == "3")
            {
                data.ChartData.Add("5人以下");
                data.ChartData.Add("6-10人");
                data.ChartData.Add("11-50人");
                data.ChartData.Add("51-100人");
                data.ChartData.Add("101-200人");
                data.ChartData.Add("201人以上");

                foreach (var item in data.ChartData)
                {
                    int cnt = registers.Where(x => x.CompanyEmpAmount == item).Count();
                    data.ChartValue.Add(cnt);
                }
            }
            else if (filter == "4") 
            {
                data.ChartData.Add("桃分署/就業中心");
                data.ChartData.Add("中小企總官網/EDM");
                data.ChartData.Add("工業區");
                data.ChartData.Add("公(工)協會");
                data.ChartData.Add("朋友/同事/社團介紹");
                data.ChartData.Add("報章雜誌");
                data.ChartData.Add("網路廣告");
                data.ChartData.Add("其他");

                foreach (var item in data.ChartData)
                {
                    int cnt = 0;
                    var cntVal = registers.Where(x => x.InfoFrom.Contains(item));
                    if (cntVal != null)
                    {
                        cnt = cntVal.Count();
                    }

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
                List<TbActivity> ListData = Lookup<TbActivity>(ref _Msg, x => listID.Contains(x.Id) && x.IsDelete == false).ToList();

                foreach (var item in ListData)
                {
                    Data.Add(new SelectListItem() { Text = item.Title, Value = item.Id.ToString() });
                }
            }
            
            return Data;
        }

        /// <summary>
        /// 取得年度資訊
        /// </summary>
        /// <returns></returns>
        public List<SelectListItem> SetDDL_Year()
        {
            List<SelectListItem> Data = new List<SelectListItem>();
            Data.Add(new SelectListItem() { Text = "請選擇", Value = "" });

            var DayMax = Lookup<TbActivitySection>(ref _Msg).OrderByDescending(x => x.Day).Select(x => x.Day).FirstOrDefault();
            if (DayMax.Year == 2022)
            {
                Data.Add(new SelectListItem() { Text = "111", Value = "111" });
            }
            else
            {
                for (int i = 111; i <= (DayMax.Year - 1911); i++)
                {
                    Data.Add(new SelectListItem() { Text = i.ToString(), Value = i.ToString() });
                }
            }

            return Data;
        }

        #endregion
    }
}
