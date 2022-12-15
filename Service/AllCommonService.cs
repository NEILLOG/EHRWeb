using BASE.Extensions;
using BASE.Models;
using BASE.Models.DB;
using BASE.Models.Enums;
using BASE.Service.Base;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BASE.Service
{
    public class AllCommonService : ServiceBase
    {
        private readonly IHttpContextAccessor _contextAccessor = null!;
        private readonly DBContext _dbContext;
        private readonly IConfiguration _conf;


        private string _Message = string.Empty;

        public AllCommonService(DBContext context,
                                IHttpContextAccessor contextAccessor,
                                IConfiguration configuration) : base(context)
        {
            _contextAccessor = contextAccessor;
            _dbContext = context;
            _conf = configuration;
        }

        /// <summary>產生新ID(範例prefix+00000001)
        /// 
        /// </summary>
        /// <param name="Prefix">前綴詞</param>
        /// <param name="TotalLength">ID總長度</param>
        /// <param name="MaxID">最大值ID</param>
        /// <returns>新ID</returns>
        public string IDGenerator(string Prefix, int TotalLength, string? MaxID)
        {
            string tmp = "";
            string final = "";

            if (!string.IsNullOrEmpty(MaxID))
            {
                ReadOnlySpan<char> input = MaxID.AsSpan();

                tmp = (int.Parse(input.Slice(Prefix.Length, TotalLength - Prefix.Length)) + 1).ToString();
            }
            else
            {
                tmp = "1";
            }

            final = Prefix + tmp.PadLeft(TotalLength - Prefix.Length, '0');

            return final;
        }

        /// <summary>產生新ID(範例prefix+00000001)
        /// 
        /// </summary>
        /// <param name="Prefix">前綴詞</param>
        /// <param name="TotalLength">ID總長度</param>
        /// <returns>新ID</returns>
        public async Task<string> IDGenerator<T>() where T : class
        {
            IDbContextTransaction transaction;
            // 傳入型別名稱
            string table_name = typeof(T).Name;
            bool has_transaction = _dbContext.Database.CurrentTransaction != null;

            if (!has_transaction)
            {
                transaction = _dbContext.Database.BeginTransaction();
            }
            else
            {
                transaction = _dbContext.Database.CurrentTransaction;
            }



            // 鎖定 table
            await RowLock<TbIdSummary>(transaction, string.Format("WHERE TableName = '{0}'", table_name));

            // 目前記錄 id 編號最大值
            TbIdSummary? max = await Lookup<TbIdSummary>(ref _Message, x => x.TableName == table_name).SingleOrDefaultAsync();

            string max_id = string.Empty;

            if (max == null)
            {
                if (!has_transaction)
                {
                    transaction.Commit();
                }

                throw new Exception("請至 TbIdSummary 表中建立正確資料");
            }
            else
            {
                // 取出序號 +1
                max_id = max.MaxId++.ToString();

                ActionResultModel<TbIdSummary> result = await Update(max, transaction);

                if (!result.IsSuccess)
                {
                    throw new Exception(result.Message);
                }
                else
                {
                    _dbContext.Entry<TbIdSummary>(max).State = EntityState.Detached;

                    if (!has_transaction)
                    {
                        transaction.Commit();
                    }


                    return max.Prefix + max_id.PadLeft(max.Length - max.Prefix.Length, '0');
                }
            }


        }

        /// <summary>錯誤訊息記錄</summary>
        /// <param name="request">請求JSON</param>
        /// <param name="response">回傳JSON</param>
        /// <param name="route_path">API_URL</param>
        /// <returns>新ID</returns>
        public async Task<bool> Error_Record<T>(string Platform, string Action, T? response) where T : class
        {
            string _Message = string.Empty;
            string? UserAgent = string.Empty;

            #region Get UserAgent
            //try
            //{
            //    UserAgent = _contextAccessor.HttpContext.Request.Headers["User-Agent"].ToString();
            //}
            //catch (Exception ex)
            //{
            //    UserAgent = null;
            //}
            #endregion

            TbLog log = new TbLog()
            {
                Ip = GetIPAddress_IPv4(),
                Action = Action,
                Date = DateTime.Now,
                Message = JsonSerializer.Serialize(response, new JsonSerializerOptions()
                {
                    ReferenceHandler = ReferenceHandler.IgnoreCycles,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                }),
                Platform = Platform,
                Url = GetAbsoluteUrl(),
                UserAgent = GetUserAgent()
            };

            ActionResultModel<TbLog> result = await base.Insert(log);

            return result.IsSuccess;
        }

        /// <summary>測試</summary>
        /// <param name="msg">回傳JSON</param>
        /// <returns>新ID</returns>
        public async Task<bool> TestRecord(string Platform, string Action, string? msg)
        {
            string _Message = string.Empty;


            TbLog log = new TbLog()
            {
                Ip = "",
                Action = Action,
                Date = DateTime.Now,
                Message = msg,
                Platform = Platform,
                Url = "",
                UserAgent = ""
            };

            ActionResultModel<TbLog> result = await base.Insert(log);

            return result.IsSuccess;
        }

        /// <summary>API 請求回傳紀錄</summary>
        /// <param name="request">請求JSON</param>
        /// <param name="response">回傳JSON</param>
        /// <returns>新ID</returns>
        public async Task<bool> API_Record<TRequest, TResponse>(TRequest? request, TResponse? response) where TRequest : class
                                                                                                       where TResponse : class
        {
            string _Message = string.Empty;

            TbApiLog log = new TbApiLog()
            {
                Ipaddr = GetIPAddress_IPv4(),
                CreateDate = DateTime.Now,
                Request = JsonSerializer.Serialize(request, new JsonSerializerOptions()
                {
                    ReferenceHandler = ReferenceHandler.IgnoreCycles,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                }),
                Response = JsonSerializer.Serialize(response, new JsonSerializerOptions()
                {
                    ReferenceHandler = ReferenceHandler.IgnoreCycles,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                }),
                RoutePath = GetAbsoluteUrl(),
            };

            ActionResultModel<TbApiLog> result = await base.Insert(log);

            return result.IsSuccess;
        }


        public string GetIPAddress_IPv4()
        {
            string sIPAddress = string.Empty;

            try
            {
                sIPAddress = _contextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
            }
            catch (Exception ex)
            {
                sIPAddress = string.Empty;
            }

            return sIPAddress;

        }
        public string GetAbsoluteUrl()
        {
            try
            {
                HttpRequest request = _contextAccessor.HttpContext.Request;

                string url = new StringBuilder()
                            .Append(request.Scheme)
                            .Append("://")
                            .Append(request.Host)
                            .Append(request.PathBase)
                            .Append(request.Path)
                            .Append(request.QueryString)
                            .ToString();

                return url;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }

        }
        public string GetUserAgent()
        {
            try
            {
                int max_len = 500;

                string user_agent = _contextAccessor.HttpContext.Request.Headers["User-Agent"].ToString();

                if (user_agent.Length > max_len)
                {
                    ReadOnlySpan<char> input = user_agent.AsSpan();

                    user_agent = input.Slice(0, max_len).ToString();
                }

                return user_agent;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 登入紀錄
        /// </summary>
        /// <param name="Platform">平台(Frontend,Backend...等)</param>
        /// <param name="LoginMessage">登入訊息</param>
        /// <param name="Account"></param>
        /// <param name="IsSso"></param>
        /// <param name="SsoResult"></param>
        /// <returns></returns>
        public async Task<bool> LoginRecord(string Platform, string LoginMessage, string Account, string? UserID = null, bool IsSso = false, string? SsoResult = null)
        {
            TbLoginRecord log = new TbLoginRecord
            {
                Platform = Platform,
                UserId = UserID,
                Account = Account,
                Ip = GetIPAddress_IPv4(),
                LoginTime = DateTime.Now,
                LoginMsg = LoginMessage,
                UserAgent = GetUserAgent(),
                Sso = IsSso,
                Ssoresult = (SsoResult == null) ? null : JsonSerializer.Serialize(SsoResult, new JsonSerializerOptions()
                {
                    ReferenceHandler = ReferenceHandler.IgnoreCycles,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                })
            };
            ActionResultModel<TbLoginRecord> result = await base.Insert(log);
            return result.IsSuccess;
        }

        #region 共用下拉選單
        /// <summary>
        ///  0: 無項目 1: 請選擇 2: 全部
        /// </summary>
        /// <param name="Type"> 0: 無項目 1: 請選擇 2: 全部</param>
        /// <param name="SelectedValue">預設選項(0: 否，1: 是)，參考 BASE.Models.Enums.BooleanValue</param>
        /// <param name="StringOfTrueStatement">True 值顯示文字，預設為"是"</param>
        /// <param name="StringOfFalseStatement">False 值顯示文字，預設為"否"</param>
        /// <returns></returns>
        public List<SelectListItem> SetDDL_TF(int Type, int? SelectedValue = null, string? StringOfTrueStatement = null, string? StringOfFalseStatement = null)
        {
            List<SelectListItem> Data = EnumExtensions.SetDDL<BooleanValue>(Type, SelectedValue);

            if (!string.IsNullOrEmpty(StringOfTrueStatement))
            {
                Data.Where(x => x.Value == (BooleanValue.True).ToString()).ToList().ForEach(x => x.Text = StringOfTrueStatement);
            }

            if (!string.IsNullOrEmpty(StringOfFalseStatement))
            {
                Data.Where(x => x.Value == (BooleanValue.False).ToString()).ToList().ForEach(x => x.Text = StringOfFalseStatement);
            }

            return Data;
        }

        /// <summary>
        ///  0: 無項目 1: 請選擇 2: 全部
        /// </summary>
        /// <param name="Type"> 0: 無項目 1: 請選擇 2: 全部</param>
        /// <param name="SelectedList">預設選項(0: 否，1: 是)，參考 BASE.Models.Enums.BooleanValue</param>
        /// <param name="StringOfTrueStatement">True 值顯示文字，預設為"是"</param>
        /// <param name="StringOfFalseStatement">False 值顯示文字，預設為"否"</param>
        /// <returns></returns>
        public List<SelectListItem> SetDDL_TF_MultiSelect(int Type, IEnumerable<BooleanValue>? SelectedList = null, string? StringOfTrueStatement = null, string? StringOfFalseStatement = null)
        {
            List<int>? SelectedItems = null;

            if (SelectedList != null)
            {
                SelectedItems = SelectedList.Select(x => (int)x).ToList();
            }

            List<SelectListItem> Data = EnumExtensions.SetDDL_MultiSelect<BooleanValue>(Type, SelectedItems);

            if (!string.IsNullOrEmpty(StringOfTrueStatement))
            {
                Data.Where(x => x.Value == (BooleanValue.True).ToString()).ToList().ForEach(x => x.Text = StringOfTrueStatement);
            }

            if (!string.IsNullOrEmpty(StringOfFalseStatement))
            {
                Data.Where(x => x.Value == (BooleanValue.False).ToString()).ToList().ForEach(x => x.Text = StringOfFalseStatement);
            }

            return Data;
        }

        /// <summary>
        ///  0: 無項目 1: 請選擇 2: 全部
        /// </summary>
        /// <param name="Type">預設選項(0: 女，1: 男)，參考 BASE.Models.Enums.Sex</param>
        /// <param name="SelectedValue"></param>
        /// <returns></returns>
        public List<SelectListItem> SetDDL_ActiveStatus(int Type, int? SelectedValue = null)
        {
            List<SelectListItem> Data = EnumExtensions.SetDDL<ActiveStatus>(Type, SelectedValue).ToList();

            return Data;
        }

        /// <summary>
        ///  0: 無項目 1: 請選擇 2: 全部
        /// </summary>
        /// <param name="Type">預設選項(0: 女，1: 男)，參考 BASE.Models.Enums.Sex</param>
        /// <param name="SelectedValue"></param>
        /// <returns></returns>
        public List<SelectListItem> SetDDL_Sex(int Type, string? SelectedValue = null)
        {
            List<SelectListItem> Data = EnumExtensions.SetDDL<Sex>(Type);

            if (!string.IsNullOrEmpty(SelectedValue))
            {
                Data.Where(x => x.Value == SelectedValue).ToList().ForEach(x => x.Selected = true);
            }

            return Data;
        }


        /// <summary>
        ///  0: 無項目 1: 請選擇 2: 全部 3: 不拘
        /// </summary>
        /// <param name="Type"></param>
        /// <returns></returns>
        public List<SelectListItem> SetDDL_Default(int Type)
        {
            List<SelectListItem> Data = new List<SelectListItem>();
            switch (Type)
            {
                case 1:
                    Data.Add(new SelectListItem() { Text = "--- 請選擇 ---", Value = "" });
                    break;
                case 2:
                    Data.Add(new SelectListItem() { Text = "--- 全部 ---", Value = "" });
                    break;
                case 3:
                    Data.Add(new SelectListItem() { Text = "不拘", Value = "" });
                    break;
                default:
                    break;
            }
            return Data;
        }

        #endregion
    }
}
