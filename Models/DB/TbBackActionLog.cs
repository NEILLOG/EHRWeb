using System;
using System.Collections.Generic;

namespace BASE.Models.DB
{
    public partial class TbBackActionLog
    {
        public int LogId { get; set; }
        public string Action { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string? Message { get; set; }
        public string? Data { get; set; }
        public string Ipaddress { get; set; } = null!;
        public string Url { get; set; } = null!;
        public string UserAgent { get; set; } = null!;
        public string? CreateUser { get; set; }
        public DateTime? CreateDate { get; set; }
    }
}
