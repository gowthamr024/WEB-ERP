using ERP.Infrastructure.Models.Entities;
using ERP.Infrastructure.Repositories.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Security.AccessControl;

namespace ERP.Web.Controllers
{
    public class BaseController : Controller
    {
        private readonly IAuthRepository _authRepository;
        public BaseController(IAuthRepository authRepository)
        {
            _authRepository = authRepository;
        }
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId != null)
            {
                string _prevarea = HttpContext.Session.GetString("CurrentArea")!;
                string area = RouteData.Values["area"]?.ToString() ?? string.Empty;
                ViewData["CurrentArea"] = area;

                if (!string.IsNullOrEmpty(area) && _prevarea != area)
                {
                    var modulesJson = HttpContext.Session.GetString("ModulesWithScreensJson");
                    if (string.IsNullOrEmpty(modulesJson))
                    {
                        var moduless = await _authRepository.GetModulesWithScreensByUserIDAsync(userId);
                        modulesJson = JsonConvert.SerializeObject(moduless);
                        HttpContext.Session.SetString("ModulesWithScreensJson", modulesJson);
                    }
                    if (!string.IsNullOrEmpty(modulesJson))
                    {
                        var modules = JsonConvert.DeserializeObject<List<Module>>(modulesJson);
                        var selectedModule = modules?.Find(m => string.Equals(m.ModuleName, area, StringComparison.OrdinalIgnoreCase));

                        if (selectedModule != null && !string.IsNullOrEmpty(selectedModule.Area))
                        {
                            HttpContext.Session.SetString("CurrentArea", area);
                            HttpContext.Session.SetInt32("SelectedModuleID", selectedModule!.ModuleID);
                            HttpContext.Session.SetString("SelectedModule", JsonConvert.SerializeObject(selectedModule));
                            HttpContext.Session.SetString("SelectedModuleName", selectedModule.ModuleName);
                        }
                    }
                }

            }
            else
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
            await next();
        }
    }
}

