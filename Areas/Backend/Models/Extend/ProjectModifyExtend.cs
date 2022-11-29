﻿using BASE.Models.DB;
using BASE.Models.Enums;
using BASE.Extensions;

namespace BASE.Areas.Backend.Models.Extend
{
    public class ProjectModifyExtend
    {
        public TbProjectModify ProjectModify { get; set; } = null!;

        public string FileName { get; set; } = null!;
        public string FilePath { get; set; } = null!;
    }
}
