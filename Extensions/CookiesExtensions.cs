using BASE.Service;
using System.Text.Json;
using System.Web;

namespace BASE.Extensions
{
    public static class CookiesExtensions
    {
        public static void Set<T>(this IResponseCookies cookies, string key, T value, CookieOptions? options = null)
        {
            var jsonValue = JsonSerializer.Serialize(value);
            if (!string.IsNullOrEmpty(jsonValue))
            {
                var encryptValue = EncryptService.AES.RandomizedEncrypt(jsonValue);
                cookies.Append(key, encryptValue, (options != null) ? options : new CookieOptions
                {
                    Expires = DateTime.Now.AddHours(12),
                    IsEssential = true,
                    Secure = true,
                    HttpOnly = true,
                    SameSite = SameSiteMode.Strict,
                });
            }
        }

        public static T? Get<T>(this IRequestCookieCollection cookies, string key)
        {
            var value = cookies[key];
            if (string.IsNullOrEmpty(value))
                return default;
            else
            {
                string dValue = EncryptService.AES.RandomizedDecrypt(HttpUtility.HtmlDecode(value));
                return dValue == null ? default : JsonSerializer.Deserialize<T>(dValue);
            }
        }

    }
}
