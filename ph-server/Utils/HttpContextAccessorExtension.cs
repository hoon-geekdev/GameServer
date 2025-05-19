namespace Utils
{
    static public class HttpContextAccessorExtension
    {
        public static string GetUserId(this IHttpContextAccessor httpContextAccessor)
        {
            HttpContext? context = httpContextAccessor.HttpContext;
            if (context == null)
                return "";

            string? accountId = context.Items["accountId"]?.ToString();
            if (accountId == null)
                return "";

            return accountId;
        }
    }
}
