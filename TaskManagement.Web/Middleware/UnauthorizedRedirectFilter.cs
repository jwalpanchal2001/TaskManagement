using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace TaskManagement.Web.Middleware;

public class UnauthorizedRedirectFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is UnauthorizedAccessException)
        {
            context.Result = new RedirectToActionResult("Login", "Account", new { message = "session_expired" });
            context.ExceptionHandled = true;
        }
    }
}
