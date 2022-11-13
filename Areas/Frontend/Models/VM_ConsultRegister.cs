using BASE.Areas.Frontend.Models.Extend;
using BASE.Models.DB;
using BASE.Models.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BASE.Areas.Frontend.Models
{
    public class VM_ConsultRegister
    {
        public TbConsultRegister? ExtendItem { get; set; }

        public List<String> CheckedSubjects { get; set; }

        public List<SelectListItem> ddlLocation
        {
            get
            {
                return new List<SelectListItem>()
                {
                    new SelectListItem(){ Text = "請選擇", Value = ""},
                    new SelectListItem(){ Text = "桃園市", Value = "桃園市"},
                    new SelectListItem(){ Text = "新竹市", Value = "新竹市"},
                    new SelectListItem(){ Text = "新竹縣", Value = "新竹縣"},
                    new SelectListItem(){ Text = "苗栗縣", Value = "苗栗縣"}
                };
            }
        }
    }

 

}
