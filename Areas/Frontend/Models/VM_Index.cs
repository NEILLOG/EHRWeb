using BASE.Areas.Frontend.Models.Extend;
using BASE.Models.DB;
using BASE.Models.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BASE.Areas.Frontend.Models
{
    public class VM_Index
    {
        public List<AdExtend>? Ads { get; set; }
        public List<NewsExtend>? News { get; set; }
        public List<ActivityExtend>? Activities { get; set; }
        public List<RelationLinkExtend>? RelationLinks { get; set; }
    }

}
