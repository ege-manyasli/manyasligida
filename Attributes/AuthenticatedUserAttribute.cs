using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using manyasligida.Models;

namespace manyasligida.Attributes
{
    public class AuthenticatedUserAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var session = context.HttpContext.Session;
            var userId = session.GetString(ApplicationConstants.SessionKeys.UserId);
            
            if (string.IsNullOrEmpty(userId))
            {
                context.Result = new RedirectToActionResult(ApplicationConstants.Actions.Login, ApplicationConstants.Controllers.Account, null);
                return;
            }
            
            base.OnActionExecuting(context);
        }
    }
}