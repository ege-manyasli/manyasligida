using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using manyasligida.Models;

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
                context.Result = new RedirectToActionResult(ApplicationConstants.Actions.Login, ApplicationConstants.Controllers.Admin, null);
                return;
            }
            
            base.OnActionExecuting(context);
        }
    }
}