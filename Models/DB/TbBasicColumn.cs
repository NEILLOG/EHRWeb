using System;
using System.Collections.Generic;

namespace BASE.Models.DB
{
    public partial class TbBasicColumn
    {
        public string BacolId { get; set; } = null!;
        public string LangId { get; set; } = null!;
        public string? BacolCode { get; set; }
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string? FileId { get; set; }
        public int? Order { get; set; }
        public bool IsActive { get; set; }
        public string? CreateUser { get; set; }
        public DateTime? CreateDate { get; set; }
        public string? ModifyUser { get; set; }
        public DateTime? ModifyDate { get; set; }
    }
}
