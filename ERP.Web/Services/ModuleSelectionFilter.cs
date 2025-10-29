using ERP.Infrastructure.Models.Entities;
using ERP.Infrastructure.Repositories.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;

namespace ERP.Web.Services
{
    public class ModuleSelectionFilter : IAsyncActionFilter
    {
        private readonly IAuthRepository _authRepo;

        public ModuleSelectionFilter(IAuthRepository authRepo)
        {
            _authRepo = authRepo;
        }
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (context.HttpContext.Session.GetString("SelectedModuleName") != null)
            {
                var moduleJson = context.HttpContext.Session.GetString("ModulesWithScreensJson");
                var selectmoduleid = context.HttpContext.Session.GetInt32("SelectedModuleID");

                var _module = JsonConvert.DeserializeObject<List<Module>>(moduleJson!);
                var _selectedModule = _module?.Find(m => m.ModuleID == selectmoduleid);
                var _screens = _selectedModule?.Screens ?? new List<Screen>();

                var newscreen = _screens.Find(s => s.ControllerName == context.ActionDescriptor.RouteValues["controller"]);
                if (newscreen != null)
                {
                    //.ForEach(m => m.Screens) .Any(s => s.ControllerName.Equals(context.ActionDescriptor.RouteValues["controller"], StringComparison.OrdinalIgnoreCase));
                    var modid = newscreen!.ModuleID;

                    context.HttpContext.Session.SetInt32("SelectedModuleID", modid);
                    context.HttpContext.Session.SetString("SelectedModule", JsonConvert.SerializeObject(_selectedModule));
                }
            }
            else
            {
                var modulesJson = context.HttpContext.Session.GetString("ModulesWithScreensJson");
                if (!string.IsNullOrEmpty(modulesJson))
                {
                    var modules = JsonConvert.DeserializeObject<List<Module>>(modulesJson);
                    var controllerName = context.ActionDescriptor.RouteValues["controller"];

                    var selectedModule = modules
                        .FirstOrDefault(m => m.Screens.Any(s =>
                            s.ControllerName.Equals(controllerName, StringComparison.OrdinalIgnoreCase)));

                    if (selectedModule != null)
                    {
                        context.HttpContext.Session.SetInt32("SelectedModuleID", selectedModule.ModuleID);
                        context.HttpContext.Session.SetString("SelectedModule", JsonConvert.SerializeObject(selectedModule));
                    }
                }
            }

            await next();
        }
    }
}
