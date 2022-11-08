namespace BASE.Models
{
    /// <summary>
    /// 分頁類別
    /// </summary>
    [Serializable]
    public class PagerInfo
    {
        public PagerInfo()
        {
            m_iPageIndex = 1;
            m_iPageCount = 30;
            m_iDataCount = 1;
            m_iPageRange = 4;
            this.SetPageCountList();
        }

        public PagerInfo(int pageCount)
        {
            m_iPageIndex = 1;
            m_iPageCount = pageCount;
            m_iDataCount = 1;
            m_iPageRange = 4;
            this.SetPageCountList();
        }

        /// <summary>設定分頁資料數量集合
        /// 
        /// </summary>
        /// <param name="listPageCount"></param>
        public PagerInfo(List<int> listPageCount)
        {
            m_iPageIndex = 1;
            //m_iDataCount = 1;

            m_iPageCountList = listPageCount;
            m_iPageCountList.Sort();
            m_iPageCount = listPageCount[0];

        }

        /// <summary>
        /// 設定分頁資料數量集合
        /// </summary>
        private void SetPageCountList()
        {
            m_iPageCountList = new List<int>();
            m_iPageCountList.Add(5);
            m_iPageCountList.Add(10);
            m_iPageCountList.Add(15);
            m_iPageCountList.Add(30);
            m_iPageCountList.Add(50);
            m_iPageCountList.Add(100);

            m_iPageCountList.Sort();
        }

        /// <summary>
        /// 查詢時用來重置頁數
        /// </summary>
        public void SetDedault()
        {
            m_iPageIndex = 1;
        }

        /// <summary>
        /// 目前頁碼 
        /// </summary>
        public int m_iPageIndex { get; set; }
        /// <summary>
        /// 每頁資料數量
        /// </summary>
        public int m_iPageCount { get; set; }
        /// <summary>
        /// 資料總數量
        /// </summary>
        public int m_iDataCount { get; set; }
        /// <summary>
        /// 總頁數
        /// </summary>
        public int m_iPageTotal { get; set; }
        /// <summary>
        /// 上一頁
        /// </summary>
        public int m_iPrePage { get; set; }
        /// <summary>
        /// 下一頁
        /// </summary>
        public int m_iNextPage { get; set; }
        /// <summary>
        /// 分頁資料數量集合
        /// </summary>
        public List<int> m_iPageCountList { get; set; }
        /// <summary>
        /// 搜尋偵測
        /// </summary>
        public bool m_Search { get; set; }
        /// <summary>
        /// 頁碼區間(前後推N頁)
        /// </summary>
        public int m_iPageRange { get; set; }
        /// <summary>
        /// 最小頁
        /// </summary>
        public int m_iPageMin 
        {
            get 
            {
                return (m_iPageIndex - m_iPageRange >= 1) ? m_iPageIndex - m_iPageRange : 1;
            }
        }
        /// <summary>
        /// 最大頁
        /// </summary>
        public int m_iPageMax
        {
            get 
            {
                return (m_iPageIndex + m_iPageRange <= m_iPageTotal) ? m_iPageIndex + m_iPageRange : m_iPageTotal;
            }
        }
        /// <summary>
        /// 是否顯示"首頁"
        /// </summary>
        public bool isShowFirst 
        {
            get 
            {
                return (m_iPageIndex != 1 && m_iDataCount > 0);
            }
        }
        /// <summary>
        /// 是否顯示"下一頁"
        /// </summary>
        public bool isShowPrevious 
        {
            get
            {
                return (m_iPageIndex > 1 && m_iDataCount > 0);
            }
        }
        /// <summary>
        /// 是否顯示"上一頁"
        /// </summary>
        public bool isShowNext 
        {
            get
            {
                return (m_iPageIndex < m_iPageTotal && m_iDataCount > 0);
            }
        }
        /// <summary>
        /// 是否顯示"末頁"
        /// </summary>
        public bool isShowLast 
        {
            get
            {
                return (m_iPageIndex != m_iPageTotal && m_iDataCount > 0);
            }
        }

    }
}
