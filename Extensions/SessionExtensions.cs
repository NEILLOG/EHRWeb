using System.Text.Json;

namespace BASE.Extensions
{
    /// <summary>
    /// Session 原生擴充
    /// sample code:
    /// HttpContext.Session.Get<DateTime>(SessionKeyTime);
    /// HttpContext.Session.Set<DateTime>(SessionKeyTime, currentTime);
    /// </summary>
    public static class SessionExtensions
    {
        public static void Set<T>(this ISession session, string key, T value)
        {
            session.SetString(key, JsonSerializer.Serialize(value));
        }

        public static T? Get<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default : JsonSerializer.Deserialize<T>(value);
        }
    }

}
