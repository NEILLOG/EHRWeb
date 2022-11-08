using System;
using System.Collections.Generic;

namespace BASE.Models.DB
{
    /// <summary>
    /// 聯絡我們
    /// </summary>
    public partial class TbContactUs
    {
        public long Id { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Response { get; set; } = null!;
        public DateTime CreateDate { get; set; }
    }
}
