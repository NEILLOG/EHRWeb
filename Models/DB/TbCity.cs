using System;
using System.Collections.Generic;

namespace BASE.Models.DB
{
    /// <summary>
    /// 縣市鄉鎮表
    /// </summary>
    public partial class TbCity
    {
        public string CityId { get; set; } = null!;
        /// <summary>
        /// 縣市鄉鎮名稱
        /// </summary>
        public string CityName { get; set; } = null!;
        /// <summary>
        /// 縣市鄉鎮類型(1縣市2行政區)
        /// </summary>
        public int CityLevel { get; set; }
        /// <summary>
        /// 父類別
        /// </summary>
        public string ParentId { get; set; } = null!;
        public string? PostCode { get; set; }
        public string? CountryId { get; set; }
        /// <summary>
        /// 是否刪除
        /// </summary>
        public bool IsDelete { get; set; }
        /// <summary>
        /// 建立者
        /// </summary>
        public string? CreateUser { get; set; }
        /// <summary>
        /// 建立日期
        /// </summary>
        public DateTime? CreateDate { get; set; }
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
