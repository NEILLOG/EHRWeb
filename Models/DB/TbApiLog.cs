using System;
using System.Collections.Generic;

namespace BASE.Models.DB
{
    public partial class TbApiLog
    {
        public int Pid { get; set; }
        public string Request { get; set; } = null!;
        public string Response { get; set; } = null!;
        public string RoutePath { get; set; } = null!;
        public string? Ipaddr { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
