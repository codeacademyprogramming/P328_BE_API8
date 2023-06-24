using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Shop.UI.Filters
{
    public class AuthFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.HttpContext.Request.Cookies["auth_token"] == null)
            {
                context.Result = new RedirectResult("/account/login");
                return;
            }
        }
        public void OnActionExecuted(ActionExecutedContext context)
        {
            
        }
    }
}
