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

        [Display(Name = "審核狀態")]
        public string IsValid { get; set; }
    }
}
