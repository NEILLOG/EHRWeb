using BASE.Areas.Frontend.Models.Extend;
using BASE.Models.DB;
using BASE.Models.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BASE.Areas.Frontend.Models
{
    public class VM_ActivityReigster
    {
        /// <summary>活動主檔資料</summary>
        public TbActivity Header { get; set; }

        //註冊主表
        public TbActivityRegister? Main { get; set; }

        //註冊場次清單
        public List<TbActivityRegisterSection>? RegisterSection { get; set; }

        //活動場次清單
        public List<TbActivitySection>? Sections { get; set; }


        public List<SelectListItem> ddlCompanyLocation 
        { 
            get 
            {
                return new List<SelectListItem>()
                {
                    new SelectListItem() { Text = "請選擇", Value = "" },
                    new SelectListItem() { Text = "桃園市", Value = "桃園市" },
                    new SelectListItem() { Text = "新竹市", Value = "新竹市" },
                    new SelectListItem() { Text = "新竹縣", Value = "新竹縣" },
                    new SelectListItem() { Text = "苗栗縣", Value = "苗栗縣" },
                    new SelectListItem() { Text = "其他地區", Value = "其他地區" }
                };
            } 
        }

        public List<SelectListItem> ddlCompanyType
        {
            get
            {
                return new List<SelectListItem>()
                {
                    new SelectListItem() { Text = "請選擇", Value = "" },
                    new SelectListItem() { Text = "A大類「農、林、魚、牧業」", Value = "A大類「農、林、魚、牧業」" },
                    new SelectListItem() { Text = "B大類「礦業及土石採取業」", Value = "B大類「礦業及土石採取業」" },
                    new SelectListItem() { Text = "C大類「製造業」", Value = "C大類「製造業」" },
                    new SelectListItem() { Text = "D大類「電力及燃氣供應業」", Value = "D大類「電力及燃氣供應業」" },
                    new SelectListItem() { Text = "E大類「用水供應及汙染整治業」", Value = "E大類「用水供應及汙染整治業」" },
                    new SelectListItem() { Text = "F大類「營建工程業」", Value = "F大類「營建工程業」" },
                    new SelectListItem() { Text = "G大類「批發及零售業」", Value = "G大類「批發及零售業」" },
                    new SelectListItem() { Text = "H大類「運輸及倉儲業」", Value = "H大類「運輸及倉儲業」" },
                    new SelectListItem() { Text = "I大類「住宿及餐飲業」", Value = "I大類「住宿及餐飲業」" },
                    new SelectListItem() { Text = "J大類「出版、影音製作、傳播及資通訊服務業」", Value = "J大類「出版、影音製作、傳播及資通訊服務業」" },
                    new SelectListItem() { Text = "K大類「金融及保險業」", Value = "K大類「金融及保險業」" },
                    new SelectListItem() { Text = "L大類「不動產業」", Value = "L大類「不動產業」" },
                    new SelectListItem() { Text = "M大類「專業、科學及技術服務業」", Value = "M大類「專業、科學及技術服務業」" },
                    new SelectListItem() { Text = "N大類「支援服務業」", Value = "N大類「支援服務業」" },
                    new SelectListItem() { Text = "O大類「公共行政及國防；強制性社會安全」", Value = "O大類「公共行政及國防；強制性社會安全」" },
                    new SelectListItem() { Text = "P大類「教育業」", Value = "P大類「教育業」" },
                    new SelectListItem() { Text = "Q大類「醫療保健及社會工作服務業」", Value = "Q大類「醫療保健及社會工作服務業」" },
                    new SelectListItem() { Text = "R大類「藝術、娛樂及休閒服務業」", Value = "R大類「藝術、娛樂及休閒服務業」" },
                    new SelectListItem() { Text = "S大類「其他服務業」", Value = "S大類「其他服務業」" }
                };
            }
        }

        public List<SelectListItem> ddlCompanyEmpAmount
        {
            get
            {
                return new List<SelectListItem>()
                {
                    new SelectListItem() { Text = "請選擇", Value = "" },
                    new SelectListItem() { Text = "5人以下", Value = "5人以下" },
                    new SelectListItem() { Text = "6-10人", Value = "6-10人" },
                    new SelectListItem() { Text = "11-50人", Value = "11-50人" },
                    new SelectListItem() { Text = "51-100人", Value = "51-100人" },
                    new SelectListItem() { Text = "101-200人", Value = "101-200人" },
                    new SelectListItem() { Text = "201人以上", Value = "201人以上" }
                };
            }
        }

        public List<String> ckbsInfoFrom { get; set; }

        /// <summary>
        /// 回傳頁面主鍵
        /// </summary>
        public String id { get; set; }

        /// <summary>
        /// 驗證碼
        /// </summary>
        public string VerifyCode { get; set; }
    }

}
