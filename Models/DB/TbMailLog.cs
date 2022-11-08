using System;
using System.Collections.Generic;

namespace BASE.Models.DB
{
    public partial class TbMailLog
    {
        public int Pid { get; set; }
        public string Sender { get; set; } = null!;
        public string Receiver { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string? Attachment { get; set; }
        public bool IsSuccess { get; set; }
        public string? Message { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
