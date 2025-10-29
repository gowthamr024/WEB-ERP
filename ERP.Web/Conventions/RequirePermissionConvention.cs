using ERP.Web.Attributes;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace ERP.Web.Conventions
{
    public class RequirePermissionConvention : IApplicationModelConvention 
    {
        public void Apply(ApplicationModel application)
        {
            foreach (var controller in application.Controllers)
            {
                foreach (var action in controller.Actions)
                {
                    bool hasPermissionAttribute = action.Attributes.Any(a => a is ScreenPermissionAttribute);
                    if (!hasPermissionAttribute)
                    {
                        throw new InvalidOperationException($"❌ ScreenPermissionAttribute missing on {controller.ControllerName}.{action.ActionName}");
                    }
                }
            }
        }
    }
}
