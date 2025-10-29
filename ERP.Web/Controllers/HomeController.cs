using ERP.Infrastructure.Helpers;
using ERP.Infrastructure.Models.Entities;
using ERP.Infrastructure.Repositories.Auth;
using ERP.Infrastructure.Services;
using ERP.Web.Attributes;
using ERP.Web.Models;
using ERP.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Security.Claims;

namespace ERP.Web.Controllers
{
    public class HomeController : BaseController
    {
        public IAuthRepository _AuthRepo;
        private readonly ILogger<HomeController> _logger;
        private readonly IErrorLogger _errorLogger;
        private readonly PermissionHelper _permission;
        private readonly PermissionCache _permissionCache;

        public HomeController(ILogger<HomeController> logger, IAuthRepository authRepo, IErrorLogger errorLogger,
            PermissionHelper permHelper, PermissionCache permissionCache) : base(authRepo)
        {
            _logger = logger;
            _AuthRepo = authRepo ?? throw new ArgumentNullException(nameof(authRepo));
            _errorLogger = errorLogger ?? throw new ArgumentNullException(nameof(errorLogger));
            _permission = permHelper;
            _permissionCache = permissionCache;
        }

        [HttpGet("login")]
        [AllowAnonymous]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        [ScreenPermission("-")]
        public IActionResult Login(string? returnUrl = null)
        {
            if (returnUrl != null && returnUrl.Contains("Access Denied"))
                returnUrl = null;

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out int userId))
            {

            }
            else
            {
                ViewData["AlreadyLoggedInUser"] = User.Identity!.Name;
                ViewData["SelectedModuleID"] = HttpContext.Session.GetInt32("SelectedModuleID");
                return View("AlreadyLoggedIn");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel());
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [ScreenPermission("-")]
        public async Task<IActionResult> UserLogin(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View("Login", model);
            }

            var user = await _AuthRepo.ValidateUserAsync(model.Username, model.Password);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid username or password.");
                return View("Login", model);
            }
            else
            {
                HttpContext.Session.Clear();
                HttpContext.Session.SetInt32("UserId", user.UserId);
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString() ),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email ),
                    new Claim(ClaimTypes.Role, user.Role ?? "User")
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = false,
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal,
                    authProperties);

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                var modules = await _AuthRepo.GetModulesWithScreensByUserIDAsync(user.UserId);
                HttpContext.Session.SetString("ModulesWithScreensJson", JsonConvert.SerializeObject(modules));

                if (modules.Count == 0)
                {
                    return RedirectToAction("AccessDenied", "Home");
                }
                else if (modules.Count == 1)
                {
                    HttpContext.Session.SetInt32("SelectedModuleID", modules[0].ModuleID);
                    HttpContext.Session.SetString("SelectedModule", JsonConvert.SerializeObject(modules[0]));
                    HttpContext.Session.SetString("SelectedModuleName", modules[0].ModuleName);
                    HttpContext.Session.SetString("CurrentArea", modules[0].Area);
                    return RedirectToAction(modules[0].DefaultActionName, modules[0].DefaultControllerName, new { area = modules[0].Area });
                }
                else
                {
                    return RedirectToAction("SelectModule", "Home");
                }
            }
        }

        [HttpPost]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        [ScreenPermission("-")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId);
                if (userId != 0)
                {
                    _permissionCache.ClearByUser(userId);
                }
                HttpContext.Session.Clear();
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                foreach (var cookie in HttpContext.Request.Cookies.Keys)
                {
                    if (cookie.StartsWith(".AspNetCore.Antiforgery"))
                    {
                        Response.Cookies.Delete(cookie);
                    }
                }
                return RedirectToAction("Login");
            }
            catch
            {
                return RedirectToAction("Login");
            }
        }

        [HttpGet("Module-Selection")]
        [ScreenPermission("-")]
        public async Task<IActionResult> SelectModule()
        {
            //var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            //if (!int.TryParse(userIdString, out int userId))
            //{
            //    return RedirectToAction("Login", "Home");
            //}

            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Home");
            }

            var modulesJson = HttpContext.Session.GetString("ModulesWithScreensJson");
            if (string.IsNullOrEmpty(modulesJson))
            {
                var moduless = await _AuthRepo.GetModulesWithScreensByUserIDAsync(userId);
                modulesJson = JsonConvert.SerializeObject(moduless);
                HttpContext.Session.SetString("ModulesWithScreensJson", modulesJson);
            }

            var modules = JsonConvert.DeserializeObject<List<Module>>(modulesJson);

            return View(modules);
        }

        [HttpGet]
        [ScreenPermission("-")]
        public IActionResult SelectModuleConfirmed(int moduleId)
        {
            var modulesJson = HttpContext.Session.GetString("ModulesWithScreensJson");
            if (string.IsNullOrEmpty(modulesJson))
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            var modules = JsonConvert.DeserializeObject<List<Module>>(modulesJson);
            var selectedModule = modules.Find(m => m.ModuleID == moduleId);
            if (selectedModule == null)
            {
                return RedirectToAction("AccessDenied");
            }

            HttpContext.Session.SetInt32("SelectedModuleID", moduleId);
            HttpContext.Session.SetString("SelectedModule", JsonConvert.SerializeObject(selectedModule));
            HttpContext.Session.SetString("SelectedModuleName", selectedModule.ModuleName);
            HttpContext.Session.SetString("CurrentArea", selectedModule.Area);
            return RedirectToAction(selectedModule.DefaultActionName, selectedModule.DefaultControllerName, new { area = selectedModule.Area });
        }

        [HttpGet("AccessDenied")]
        [ScreenPermission("-")]
        public async Task<IActionResult> AccessDenied()
        {
            return View();
        }
        [ScreenPermission("V")]
        public async Task<IActionResult> Index()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            //if (userId == null)
            //{
            //    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            //    return RedirectToAction("Login", "Home");
            //}

            var deny = await _permission.RequirePermissionAsync(this, userId, "V");
            if (deny != null) return deny;

            var modulesJson = HttpContext.Session.GetString("ModulesWithScreensJson");
            if (string.IsNullOrEmpty(modulesJson))
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            var modulesWithScreens = JsonConvert.DeserializeObject<List<Module>>(modulesJson);
            ViewBag.Modules = modulesWithScreens;

            return View();

            //// Check single permission
            //bool HasAccess = await _AuthRepo.CheckPermissionAsync(userId, ControllerContext.ActionDescriptor.ControllerName, ControllerContext.ActionDescriptor.ActionName, "V");

            //if (HasAccess) return View();
            //else return RedirectToAction("AccessDenied", "Home");

            //// Overall screen permission
            //var permissions = await _AuthRepo.GetUserPermissionsAsync(userId,
            //                      ControllerContext.ActionDescriptor.ControllerName,
            //                      ControllerContext.ActionDescriptor.ActionName);

            //if (permissions == null || !permissions.CanView)
            //    return RedirectToAction("AccessDenied", "Home");

            //ViewBag.Permissions = permissions;
        }

        [ScreenPermission("-")]
        public async Task<IActionResult> Privacy()
        {
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId);
            var deny = await _permission.RequirePermissionAsync(this, userId, "V");

            if (deny != null) return RedirectToAction("AccessDenied", "Home");
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [ScreenPermission("-")]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet("GetLatest")]
        [Authorize(Roles = "SuperAdmin")]
        [ScreenPermission("-")]
        public IActionResult GetLatest()
        {
            //option 1 - working - but as a page
            //var logPath = @"D:\Software Projects\ERP\ErrorLogs.txt";
            //var logs = System.IO.File.Exists(logPath) ? System.IO.File.ReadAllText(logPath) : "No logs found.";
            //ViewBag.Logs = logs;

            //when using option 1, add this 2 lines in the view page.
            //<h2>Error Logs</h2>
            //< pre > @ViewBag.Logs </ pre >

            //option 2 - Pop-up style
            string logFilePath = @"D:\Software Projects\ERP\ErrorLogs.txt";
            if (!System.IO.File.Exists(logFilePath))
                return Json(new string[] { "No logs found." });

            var lines = System.IO.File.ReadAllLines(logFilePath)
                                      .Reverse()
                                      .Take(100)
                                      .ToArray();
            return Json(lines);
        }
    }
}
