using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace Utils
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true)]
    public class UserIdFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var httpContext = context.HttpContext;

            if (httpContext.Items.TryGetValue("accountId", out var accountId))
            {
                context.ActionArguments["accountId"] = accountId?.ToString();
            }
            else
            {
                context.Result = new UnauthorizedResult();
            }
        }
    }
}
