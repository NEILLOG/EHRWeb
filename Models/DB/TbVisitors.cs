using System;
using System.Collections.Generic;

namespace BASE.Models.DB
{
    /// <summary>
    /// 瀏覽人次
    /// </summary>
    public partial class TbVisitors
    {
        public long VisitorsId { get; set; }
        public DateTime VisitDate { get; set; }
        public string Ip { get; set; } = null!;
    }
}
