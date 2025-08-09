using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using manyasligida.Models;
using System.Security.Claims;

namespace manyasligida.Attributes
{
    public class AdminAuthorizationAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var session = context.HttpContext.Session;
            var userId = session.GetString(ApplicationConstants.SessionKeys.UserId);
            var isAdmin = session.GetString(ApplicationConstants.SessionKeys.IsAdmin);
            
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(isAdmin) || isAdmin != "True")
            {
                // Fallback to claims
                var principal = context.HttpContext.User;
                if (principal?.Identity?.IsAuthenticated == true && principal.IsInRole("Admin"))
                {
                    base.OnActionExecuting(context);
                    return;
                }
                context.Result = new RedirectToActionResult(ApplicationConstants.Actions.Login, ApplicationConstants.Controllers.Admin, null);
                return;
            }
            
            base.OnActionExecuting(context);
        }
    }
}