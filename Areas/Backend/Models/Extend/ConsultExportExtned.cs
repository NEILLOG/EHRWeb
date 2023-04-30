using System.ComponentModel.DataAnnotations;

namespace BASE.Areas.Backend.Models.Extend
{
    public class ConsultExportExtned
    {
        [Display(Name = "企業所在地")]
        public string Location { get; set; }

        [Display(Name = "企業名稱")]
        public string Name { get; set; }

        [Display(Name = "企業統編")]
        public string BusinessId { get; set; }

        [Display(Name = "企業登記地址")]
        public string Address { get; set; }

        [Display(Name = "諮詢主題")]
        public string ConsultSubject { get; set; }

        [Display(Name = "問題陳述")]
        public string Description { get; set; }

        [Display(Name = "諮詢時間")]
        public string ConsultTime { get; set; }

        [Display(Name = "輔導地址")]
        public string ConsultAddress { get; set; }

        [Display(Name = "聯繫人姓名")]
        public string ContactName { get; set; }

        [Display(Name = "聯繫人職稱")]
        public string ContactJobTitle { get; set; }

        [Display(Name = "聯絡人電話")]
        public string ContactPhone { get; set; }

        [Display(Name = "聯絡人EMAIL")]
        public string ContactEmail { get; set; }

        [Display(Name = "顧問")]
        public string Adviser { get; set; }

        [Display(Name = "輔導助理")]
        public string AdviserAssistant { get; set; }

        [Display(Name = "輔導日期")]
        public string ReAssignDate { get; set; }

        [Display(Name = "狀態")]
        public string IsClose { get; set; }
    }
}
