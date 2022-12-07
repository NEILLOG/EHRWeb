using System.ComponentModel.DataAnnotations;

namespace BASE.Areas.Backend.Models.Extend
{
    public class MemberExportExtend
    {
        [Display(Name = "姓名")]
        public string UserName { get; set; } = null!;

        [Display(Name = "群組")]
        public string GroupName { get; set; } = null!;

        [Display(Name = "帳號")]
        public string UserInfo { get; set; }

        [Display(Name = "帳號狀態")]
        public string Status { get; set; }
    }
}
