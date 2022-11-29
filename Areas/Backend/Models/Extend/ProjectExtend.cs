using BASE.Models.DB;
using BASE.Models.Enums;
using BASE.Extensions;

namespace BASE.Areas.Backend.Models.Extend
{
    public class ProjectExtend
    {
        public TbProject Project { get; set; } = null!;

        public string ReID { get; set; } = null!;
    }
}
