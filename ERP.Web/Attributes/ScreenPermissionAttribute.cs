using ERP.Infrastructure.Repositories.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace ERP.Web.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class ScreenPermissionAttribute : Attribute, IAsyncActionFilter
    {
        private readonly string _permissionType;
        public ScreenPermissionAttribute(string permissionType)
        {
            _permissionType = permissionType;
        }
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (context.ActionDescriptor is not ControllerActionDescriptor cad) { await next(); return; }
            if (_permissionType == "-") { await next(); return; }

            var userIdString = context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdString, out int userId))
            {
                context.Result = new RedirectToActionResult("Login", "Home", null);
                return;
            }

            var controllerName = context.ActionDescriptor.RouteValues["controller"];
            var actionName = context.ActionDescriptor.RouteValues["action"];

            // get service
            var authRepo = context.HttpContext.RequestServices.GetService<IAuthRepository>();

            if (authRepo == null)
            {
                throw new InvalidOperationException("IAuthRepository not registered in DI.");
            }

            bool hasAccess = await authRepo.CheckPermissionAsync(userId, controllerName!, actionName!, _permissionType);

            if (!hasAccess)
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Home", routeValues: new { area = "" });
                return;
            }

            // ✅ if no error, continue to action
            await next();
        }
    }
}
