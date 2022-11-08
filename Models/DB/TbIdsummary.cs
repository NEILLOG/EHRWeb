using System;
using System.Collections.Generic;

namespace BASE.Models.DB
{
    public partial class TbIdSummary
    {
        public string TableName { get; set; } = null!;
        public string Prefix { get; set; } = null!;
        public int Length { get; set; }
        public long MaxId { get; set; }
    }
}
