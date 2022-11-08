using BASE.Models;
using Microsoft.EntityFrameworkCore;

namespace BASE.Service
{
    [Serializable]
    public static class PagerInfoService
    {
        /// <summary>
        /// 分頁函式
        /// </summary>
        /// <typeparam name="T">指定類別</typeparam>
        /// <param name="sErrMsg">錯誤訊息</param>
        /// <param name="_modules">結果資料</param>
        /// <param name="pager">分頁資訊</param>
        /// <returns>回傳分頁結果</returns>
        public static async Task<List<T>?> GetRange<T>(IQueryable<T> _modules, PagerInfo pager) where T : class
        {
            if (_modules == null)
            {
                return new List<T>();
            }

            try
            {
                if (pager.m_Search == true)
                {
                    pager.SetDedault();
                }
                else
                {
                    pager.m_Search = true;
                }


                //設定分頁狀態
                pager.m_iDataCount = _modules.Count();
                pager.m_iPageTotal = pager.m_iDataCount / pager.m_iPageCount;

                if ((pager.m_iDataCount % pager.m_iPageCount) > 0)
                {
                    pager.m_iPageTotal += 1;
                }

                if (pager.m_iPageIndex == 0)
                {
                    pager.m_iPageIndex = 1;
                }

                pager.m_iPrePage = pager.m_iPageIndex - 1;
                pager.m_iNextPage = pager.m_iPageIndex + 1;

                if (pager.m_iPageIndex >= pager.m_iPageTotal)
                {
                    pager.m_iPageIndex = pager.m_iPageTotal;
                    pager.m_iNextPage = pager.m_iPageIndex;
                }

                if (pager.m_iPrePage < 1)
                {
                    pager.m_iPrePage = 1;
                }

                Int32 firstIndex = (pager.m_iPageIndex - 1) * pager.m_iPageCount;

                if (pager.m_iPageTotal == 0)
                {
                    pager.m_iPageTotal = 1;
                    firstIndex = 0;
                }

                Int32 lastIndex = firstIndex + pager.m_iPageCount;
                if (lastIndex >= pager.m_iDataCount)
                {
                    lastIndex = pager.m_iDataCount;
                }

                try
                {
                    var result = await _modules.Skip(firstIndex).Take(pager.m_iPageCount).ToListAsync();
                    return result;
                }
                catch (Exception ex)
                {
                    var result = _modules.ToList();

                    result = result.Skip(firstIndex).Take(pager.m_iPageCount).ToList();
                    return result;
                }

            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
