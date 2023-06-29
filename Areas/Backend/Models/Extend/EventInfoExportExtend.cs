using System.ComponentModel.DataAnnotations;

namespace BASE.Areas.Backend.Models.Extend
{
    public class EventInfoExportExtend
    {
        [Display(Name = "企業名稱")]
        public string CompanyName { get; set; }

        [Display(Name = "企業所在地")]
        public string CompanyLocation { get; set; }

        [Display(Name = "產業別")]
        public string CompanyType { get; set; }

        [Display(Name = "姓名")]
        public string Name { get; set; }

        [Display(Name = "職稱")]
        public string JobTitle { get; set; }

        [Display(Name = "連絡電話")]
        public string Phone { get; set; }

        [Display(Name = "手機")]
        public string CellPhone { get; set; }

        [Display(Name = "電子郵件")]
        public string Email { get; set; }

        [Display(Name = "公司人數")]
        public string CompanyEmpAmount { get; set; }

        [Display(Name = "課程參與模式")]
        public string RegisterSectionType { get; set; }

        [Display(Name = "飲食選擇")]
        public string IsVegin { get; set; }

        [Display(Name = "審核狀態")]
        public string IsValid { get; set; }

        [Display(Name = "是否已簽到(上午場)")]
        public string IsSigninAM { get; set; }

        [Display(Name = "是否已簽到(下午場)")]
        public string IsSigninPM { get; set; }

        [Display(Name = "簽到日期(上午)")]
        public string SigninDate_AM { get; set; }

        [Display(Name = "簽到日期(下午)")]
        public string SigninDate_PM { get; set; }
    }
}
