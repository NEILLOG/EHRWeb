using System;
using System.Collections.Generic;

namespace BASE.Models.DB
{
    /// <summary>
    /// FAQ資料表
    /// </summary>
    public partial class NtuFaq
    {
        /// <summary>
        /// FAQ+流水碼*7
        /// </summary>
        public string Fid { get; set; } = null!;
        /// <summary>
        /// 問題 (程式控管200個字)
        /// </summary>
        public string Question { get; set; } = null!;
        /// <summary>
        /// 答案
        /// </summary>
        public string Answer { get; set; } = null!;
        /// <summary>
        /// 排序
        /// </summary>
        public int Order { get; set; }
        /// <summary>
        /// 是否刪除：是、否
        /// </summary>
        public bool IsDelete { get; set; }
        /// <summary>
        /// 建立者
        /// </summary>
        public string CreateUser { get; set; } = null!;
        /// <summary>
        /// 建立日期
        /// </summary>
        public DateTime CreateDate { get; set; }
        /// <summary>
        /// 修改者
        /// </summary>
        public string? ModifyUser { get; set; }
        /// <summary>
        /// 修改日期
        /// </summary>
        public DateTime? ModifyDate { get; set; }
    }
}
