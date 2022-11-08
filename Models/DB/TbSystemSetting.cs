using System;
using System.Collections.Generic;

namespace BASE.Models.DB
{
    public partial class TbSystemSetting
    {
        public int Pid { get; set; }
        public string Key { get; set; } = null!;
        public string Value { get; set; } = null!;
        public string? Remark { get; set; }
    }
}
