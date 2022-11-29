using BASE.Areas.Backend.Models.Extend;
using BASE.Areas.Backend.Models;
using BASE.Models.DB;
using BASE.Service.Base;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc.Rendering;
using NPOI.SS.Formula.PTG;

namespace BASE.Areas.Backend.Service
{
    public class PromoteService : ServiceBase
    {
        string _Msg = string.Empty;

        public PromoteService(DBContext context) : base(context)
        {
        }

        /// <summary>
        /// 取得推管管理列表
        /// </summary>
        /// <returns></returns>
        public List<TbPromotion>? GetPromotionList(ref String ErrMsg, VM_PromoteQueryParam? vmParam, Expression<Func<TbPromotion, bool>>? filter = null)
        {
            try
            {
                List<TbPromotion> dataList = Lookup<TbPromotion>(ref _Msg).ToList();

                if (vmParam != null)
                {
                    //關鍵字搜尋：企業人數
                    if (!string.IsNullOrEmpty(vmParam.NumCompanies) && vmParam.NumCompanies != "-1")
                    {
                        
                        if (vmParam.NumCompanies == "50")
                            // 51人以下(不包含51)
                            dataList = dataList.Where(x => x.EmpoyeeAmount < 51).ToList();
                        else
                            // 51人以上(包含51)
                            dataList = dataList.Where(x => x.EmpoyeeAmount >= 51).ToList();
                    }

                    //關鍵字搜尋：企業人數
                    if (!string.IsNullOrEmpty(vmParam.Plan) && vmParam.Plan != "-1")
                        dataList = dataList.Where(x => x.Project == vmParam.Plan).ToList();
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
        /// 取得企業人數
        /// </summary>
        /// <returns></returns>
        public List<SelectListItem> SetDDL_NumCompanies()
        {
            List<SelectListItem> Data = new List<SelectListItem>();
            Data.Add(new SelectListItem() { Text = "請選擇", Value = "-1" });
            Data.Add(new SelectListItem() { Text = "51人以下", Value = "50" });
            Data.Add(new SelectListItem() { Text = "51人以上", Value = "51" });
            return Data;
        }

        /// <summary>
        /// 取得計畫
        /// </summary>
        /// <returns></returns>
        public List<SelectListItem> SetDDL_Plan(int Type)
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

            List<string> planList = Lookup<TbPromotion>(ref _Msg).Select(x => x.Project).Distinct().ToList();

            foreach (var item in planList)
            {
                Data.Add(new SelectListItem() { Text = item, Value = item });
            }

            return Data;
        }

        #endregion

    }
}
