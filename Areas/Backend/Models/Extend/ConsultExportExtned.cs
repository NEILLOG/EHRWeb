using System.ComponentModel.DataAnnotations;

namespace BASE.Areas.Backend.Models.Extend
{
    public class ConsultExportExtned
    {
        [Display(Name = "企業名稱")]
        public string Name { get; set; }

        [Display(Name = "諮詢主題")]
        public string ConsultSubject { get; set; }

        [Display(Name = "顧問")]
        public string Adviser { get; set; }

        [Display(Name = "輔導助理")]
        public string AdviserAssistant { get; set; }

        [Display(Name = "狀態")]
        public string IsClose { get; set; }
    }
}
