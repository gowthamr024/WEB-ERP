using ERP.Infrastructure.Repositories.Auth;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Services
{
    public class PermissionHelper
    {
        private readonly IAuthRepository _authRepo;
        public PermissionHelper(IAuthRepository authRepo)
        {
            _authRepo = authRepo;
        }
        public async Task<IActionResult?> RequirePermissionAsync(Controller controller, int? userId, string permissionType)
        {
            var controllerName = controller.RouteData.Values["controller"]?.ToString() ?? string.Empty;
            var actionName = controller.RouteData.Values["action"]?.ToString() ?? string.Empty;

            var hasPermission = await _authRepo.CheckPermissionAsync(userId, controllerName, actionName, permissionType);
            if (!hasPermission)
            {
                return controller.RedirectToAction("AccessDenied", "Home", null);
            }
            return null; // Grant access
        }
    }
}
