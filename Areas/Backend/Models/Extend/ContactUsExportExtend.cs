using System.ComponentModel.DataAnnotations;

namespace BASE.Areas.Backend.Models.Extend
{
    public class ContactUsExportExtend
    {
        [Display(Name = "姓名")]
        public string Name { get; set; } = null!;

        [Display(Name = "信箱")]
        public string Email { get; set; } = null!;

        [Display(Name = "建議內容")]
        public string Response { get; set; }

        [Display(Name = "留言時間")]
        public DateTime CreateDate { get; set; }
    }
}
