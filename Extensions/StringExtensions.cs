using BASE.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace BASE.Extensions
{
    /// <summary>
    /// Enum 原生擴充
    /// sample code:
    /// 檢查傳入 string 是否有為 null or empty 的：StringExtensions.CheckIsNullOrEmpty(param1, param2, ...);
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// 檢查傳入 string 是否有為 null or empty 的
        /// </summary>
        public static bool CheckIsNullOrEmpty(params string[] values)
        {
            bool result = false;

            foreach(string value in values)
            {
                if (string.IsNullOrEmpty(value))
                {
                    result = true;
                }
            }

            return result;
        }

        /// <summary>
        /// 以 separator 串聯傳進的字串，略過 empty/null
        /// </summary>
        public static string Join_SkipNullOrEmpty(string separator, params string?[] values)
        {
            List<string> tmpList = new List<string>();

            foreach (string? value in values)
            {
                if (!string.IsNullOrEmpty(value))
                {
                    tmpList.Add(value);
                }
            }

            return string.Join(separator, tmpList);
        }

        /// <summary>
        /// 把 ID 做簡單包裝
        /// </summary>
        public static string GetEID(this string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return string.Empty;
            }
            else
            {
                /* 將ID中的 0 均取代掉，並加密 */
                id = Service.EncryptService.AES.Encrypt(id.Replace("0", ""));

                return id;
            }
        }
    }

}
