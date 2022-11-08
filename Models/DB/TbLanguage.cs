using System;
using System.Collections.Generic;

namespace BASE.Models.DB
{
    public partial class TbLanguage
    {
        public string LangId { get; set; } = null!;
        public string LangCode { get; set; } = null!;
        public string LangName { get; set; } = null!;
        public bool IsActive { get; set; }
    }
}
