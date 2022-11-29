using BASE.Areas.Frontend.Models.Extend;
using BASE.Models.DB;
using BASE.Models.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BASE.Areas.Frontend.Models
{
    /// <summary>
    /// 健康聲明調查表上傳
    /// </summary>
    public class VM_OtherUpload
    {
        public IFormFile? ModifyFile { get; set; }
    }

}
