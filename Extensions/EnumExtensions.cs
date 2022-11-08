using BASE.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
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
    /// 取得 string value：PublishStatus.Pending.GetEnumMemberValue();
    /// 取得 Display Name：PublishStatus.Pending.GetDisplayName();
    /// 取得 EnumList：EnumExtensions.GetList<PublishStatus>();
    /// </summary>
    public static class EnumExtensions
    {
        public static string? GetEnumMemberValue<T>(this T value)
        where T : Enum
        {
            return typeof(T)
                .GetTypeInfo()
                .DeclaredMembers
                .SingleOrDefault(x => x.Name == value.ToString())
                ?.GetCustomAttribute<EnumMemberAttribute>(false)
                ?.Value;
        }
        public static string? GetEnumMemberValue(this Enum enumValue)
        {
            return enumValue.GetAttribute<EnumMemberAttribute>()?.Value;
            //return typeof(T)
            //    .GetTypeInfo()
            //    .DeclaredMembers
            //    .SingleOrDefault(x => x.Name == value.ToString())
            //    ?.GetCustomAttribute<EnumMemberAttribute>(false)
            //    ?.Value;
        }

        /// <summary>
        ///     A generic extension method that aids in reflecting 
        ///     and retrieving any attribute that is applied to an `Enum`.
        /// </summary>
        public static TAttribute GetAttribute<TAttribute>(this Enum enumValue)
                where TAttribute : Attribute
        {
            return enumValue.GetType()
                            .GetMember(enumValue.ToString())
                            .First()
                            .GetCustomAttribute<TAttribute>();
        }

        /// <summary>
        /// Get enum display attr.
        /// </summary>
        /// <param name="enumValue"></param>
        /// <returns></returns>
        public static string GetDisplayName(this Enum enumValue)
        {
            return enumValue.GetAttribute<DisplayAttribute>().GetName();
        }

        /// <summary>
        /// Get enum display attr.
        /// </summary>
        /// <param name="enumValue"></param>
        /// <returns></returns>
        public static int? GetOrder(this Enum enumValue)
        {
            return enumValue.GetAttribute<DisplayAttribute>().GetOrder();
        }

        /// <summary>
        /// 取得 Enum DDL，1: 請選擇 2: 全部
        /// </summary>
        /// <param name="selected_keys">選擇的key</param>
        /// <returns></returns>
        public static List<SelectListItem> SetDDL<TAttribute>(int Type, int? selected_key = null, Func<SelectListItem, bool>? filter = null)
        {
            List<SelectListItem> result = new List<SelectListItem>();

            IEnumerable<int>? selected_keys = null;

            if (selected_key != null)
            {
                selected_keys = new List<int>() { selected_key.Value };
            }

            var enumList = GetList<TAttribute>(selected_keys);

            foreach (var enumType in enumList)
            {
                result.Add(new SelectListItem()
                {
                    Text = (string)enumType.Text,
                    Value = enumType.Value,
                    Selected = enumType.IsSelected
                });
            }

            if (filter != null)
            {
                result = result.Where(filter).ToList();
            }


            switch (Type)
            {
                case 1:
                    result.Insert(0, new SelectListItem() { Text = "--- 請選擇 ---", Value = "" });
                    break;
                case 2:
                    result.Insert(0, new SelectListItem() { Text = "--- 全部 ---", Value = "" });
                    break;
                default:
                    break;
            }

            return result;
        }

        /// <summary>
        /// 取得 Enum DDL，1: 請選擇 2: 全部
        /// </summary>
        /// <param name="selected_keys">選擇的key</param>
        /// <returns></returns>
        public static List<SelectListItem> SetDDL_MultiSelect<TAttribute>(int Type, IEnumerable<int>? selected_keys = null)
        {
            List<SelectListItem> result = new List<SelectListItem>();

            var enumList = GetList<TAttribute>(selected_keys);

            foreach (var enumType in enumList)
            {
                result.Add(new SelectListItem()
                {
                    Text = (string)enumType.Text,
                    Value = enumType.Value,
                    Selected = enumType.IsSelected
                });
            }


            switch (Type)
            {
                case 1:
                    result.Insert(0, new SelectListItem() { Text = "--- 請選擇 ---", Value = "" });
                    break;
                case 2:
                    result.Insert(0, new SelectListItem() { Text = "--- 全部 ---", Value = "" });
                    break;
                default:
                    break;
            }

            return result;
        }

        /// <summary>
        /// 取得 Enum List 模型
        /// </summary>
        /// <param name="selected_keys">選擇的key</param>
        /// <returns></returns>
        public static List<BaseModel> GetList<TAttribute>(IEnumerable<int>? selected_keys = null)
        {
            List<BaseModel> result = new List<BaseModel>();

            var enumList = Enum.GetValues(typeof(TAttribute));

            if (selected_keys == null)
            {
                foreach (Enum enumType in enumList)
                {
                    string display_name = enumType.GetDisplayName();
                    int enum_value = (int)Convert.ChangeType(enumType, enumType.GetTypeCode());
                    string? member_value = enumType.GetEnumMemberValue();
                    int? order = enumType.GetOrder();


                    result.Add(new BaseModel()
                    {
                        Key = enum_value,
                        Text = display_name,
                        Value = member_value == null ? enum_value.ToString() : member_value,
                        Order = order
                    });
                }
            }
            else
            {
                foreach (Enum enumType in enumList)
                {
                    string display_name = enumType.GetDisplayName();
                    int enum_value = (int)Convert.ChangeType(enumType, enumType.GetTypeCode());
                    string? member_value = enumType.GetEnumMemberValue();
                    int? order = enumType.GetOrder();

                    if (selected_keys.Contains(enum_value))
                    {
                        result.Add(new BaseModel()
                        {
                            Key = enum_value,
                            Text = display_name,
                            Value = member_value == null ? enum_value.ToString() : member_value,
                            IsSelected = true,
                            Order = order
                        });
                    }
                    else
                    {
                        result.Add(new BaseModel()
                        {
                            Key = enum_value,
                            Text = display_name,
                            Value = member_value == null ? enum_value.ToString() : member_value,
                            IsSelected = false,
                            Order = order
                        });
                    }

                }
            }

            return result.OrderBy(x => x.Order).ToList();
        }

        /// <summary>
        /// Get enum display attr. by member value or enum value.
        /// </summary>
        /// <param name="value">Enum Index or Member Value</param>
        /// <returns></returns>
        public static string GetDisplayName<TAttribute>(string value) where TAttribute : Enum
        {
            var enumList = GetList<TAttribute>();
            return enumList.Where(x => x.Value == value).Select(x => (string)x.Text).FirstOrDefault();
        }

        /// <summary>
        /// EnumPropertyName 轉 Enum
        /// </summary>
        /// <typeparam name="TAttribute"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TAttribute ToEnum<TAttribute>(this string value) where TAttribute : Enum
        {
            var result = (TAttribute)Enum.Parse(typeof(TAttribute), value, true);

            return result;
        }

        /// <summary>
        /// Enum value(int) 轉 Enum
        /// </summary>
        /// <typeparam name="TAttribute"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TAttribute ToEnum<TAttribute>(this int value) where TAttribute : Enum
        {
            var name = Enum.GetName(typeof(TAttribute), value);

            return name.ToEnum<TAttribute>();
        }

        /// <summary>
        /// Enum value(short) 轉 Enum
        /// </summary>
        /// <typeparam name="TAttribute"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TAttribute ToEnum<TAttribute>(this short value) where TAttribute : Enum
        {
            var name = Enum.GetName(typeof(TAttribute), value);

            return name.ToEnum<TAttribute>();
        }
    }

}
