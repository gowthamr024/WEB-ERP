using ERP.Infrastructure.Models.DTOs;
using ERP.Infrastructure.Models.Entities;
using ERP.Infrastructure.Repositories.Auth;
using ERP.Infrastructure.Services;
using ERP.Web.Areas.Admin.Models;
using ERP.Web.Attributes;
using ERP.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "SuperAdmin")]

    public class MenuController : BaseController
    {
        private readonly ILogger<MenuController> _logger;
        private readonly IErrorLogger _errorLogger;
        public IAuthRepository _authRepo;

        public MenuController(ILogger<MenuController> logger, IAuthRepository authRepo, IErrorLogger errorLogger) : base(authRepo)
        {
            _logger = logger;
            _authRepo = authRepo ?? throw new ArgumentNullException(nameof(authRepo));
            _errorLogger = errorLogger ?? throw new ArgumentNullException(nameof(errorLogger));
        }

        /* ===== Module Creation ===== */
        [HttpGet]
        [ScreenPermission("-")]
        public async Task<IActionResult> Index(int? moduleId)
        {
            var _modules = await _authRepo.GetAllModulesWithScreenAsync();
            var _selectedModule = _modules.FirstOrDefault(m => m.ModuleID == moduleId);

            var model = new ModuleScreenDashboardViewModel
            {
                Modules = _modules,
                SelectedModuleId = moduleId,
                SelectedModuleCode = _selectedModule?.ModuleCode,
                SelectedModuleName = _selectedModule?.ModuleName,
                Screens = _selectedModule?.Screens ?? new List<Screen>()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ScreenPermission("-")]
        public async Task<IActionResult> AddModule(Module module)
        {
            try
            {
                if (string.IsNullOrEmpty(module.ModuleName) || string.IsNullOrEmpty(module.DefaultControllerName) || string.IsNullOrEmpty(module.DefaultActionName)) return Json(new { success = false, message = "Please fill all the details" });
                if (module.ModuleID > 0)
                    await _authRepo.UpdateModuleAsync(module);
                else
                    await _authRepo.AddModuleAsync(module);

                return Json(new { success = true, message = "Module saved successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ScreenPermission("-")]
        public async Task<IActionResult> DeleteModule(int moduleId)
        {
            try
            {
                var result = await _authRepo.DeleteModuleAsync(moduleId);
                return Json(new { success = result > 0, message = result > 0 ? "Module deleted" : "Delete failed" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting module {ModuleId}", moduleId);
                return Json(new { success = false, message = "Error deleting module" });
            }
        }

        /* ===== Menu Management / Creation ===== */
        [HttpGet]
        [ScreenPermission("-")]
        public async Task<IActionResult> MenuManagement(int? moduleId)
        {
            var _modules = await _authRepo.GetAllModulesWithScreenAsync();
            var _selectedModule = _modules.FirstOrDefault(m => m.ModuleID == moduleId);

            var model = new ModuleScreenDashboardViewModel
            {
                Modules = _modules,
                SelectedModuleId = moduleId,
                SelectedModuleCode = _selectedModule?.ModuleCode,
                SelectedModuleName = _selectedModule?.ModuleName,
                Screens = _selectedModule?.Screens ?? new List<Screen>()
            };

            return View(model);
        }

        [HttpGet]
        [ScreenPermission("-")]
        public async Task<IActionResult> GetScreenTree(int moduleId)
        {
            try
            {
                var modules = await _authRepo.GetAllModulesWithScreenAsync();
                var selectedModule = modules.FirstOrDefault(m => m.ModuleID == moduleId);

                if (selectedModule == null)
                    return Json(new List<object>());

                var flatScreens = selectedModule.Screens
                .OrderBy(s => s.MenuOrder)
                .ToList();

                var treeData = flatScreens.Select(s => new
                {
                    id = s.ScreenID,
                    parent = (s.ParentScreenID == 0) ? "#" : s.ParentScreenID.ToString(), // '#' = root
                    text = s.ScreenName,
                    type = string.IsNullOrEmpty(s.ControllerName) ? "default" : "screen",
                    data = new
                    {
                        controller = s.ControllerName,
                        action = s.ActionName
                    }
                }).ToList();

                return Json(treeData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error building screen tree for module {ModuleId}", moduleId);
                return StatusCode(500, "Error loading screen tree");
            }
        }

        [HttpPost]
        [ScreenPermission("-")]
        public async Task<IActionResult> SaveMenuTree([FromBody] List<ScreenOrderDto> menuTree)
        {
            if (menuTree == null || !menuTree.Any())
                return Json(new { success = false, message = "No menu data received" });

            try
            {
                await _authRepo.UpdateScreenHierarchyAsync(menuTree);
                return Json(new { success = true, message = "Menu structure updated successfully!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error saving menu tree");
                return Json(new { success = false, message = "Error saving menu tree" });
            }
        }

        //// No longer needed
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //[ScreenPermission("-")]
        //public async Task<IActionResult> AddScreen(Screen screen)
        //{
        //    //if (screen.ScreenID == 0 || string.IsNullOrEmpty(screen.ScreenName)) return Json(new { success = false, message = "Please fill all the details" });
        //    if (screen.ScreenID > 0)
        //        await _authRepo.UpdateScreenAsync(screen);
        //    else
        //        await _authRepo.AddScreenAsync(screen);

        //    var moduleWithScreens = await _authRepo.GetAllModulesWithScreenAsync();
        //    var selectedModule = moduleWithScreens.FirstOrDefault(m => m.ModuleID == screen.ModuleID);

        //    var model = new ModuleScreenDashboardViewModel
        //    {
        //        Modules = moduleWithScreens,
        //        SelectedModuleId = screen.ModuleID,
        //        SelectedModuleName = selectedModule?.ModuleName,
        //        Screens = selectedModule?.Screens ?? new List<Screen>()
        //    };
        //    return View("MenuManagement", model);
        //    //return PartialView("_ModuleScreensPartial", model);
        //}

        // Delete_Screen
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //[ScreenPermission("-")]
        //public async Task<IActionResult> DeleteScreen(int screenId)
        //{
        //    try
        //    {
        //        var result = await _authRepo.DeleteScreenAsync(screenId);
        //        return Json(new { success = result > 0, message = result > 0 ? "Screen deleted" : "Delete failed" });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error deleting screen {ScreenId}", screenId);
        //        return Json(new { success = false, message = "Error deleting screen" });
        //    }
        //}*/

        //[HttpGet]
        //[ScreenPermission("-")]
        //public async Task<IActionResult> LoadModuleScreens(int moduleId)
        //{
        //    var modules = await _authRepo.GetAllModulesWithScreenAsync(); //All screen
        //    var selectedModule = modules.FirstOrDefault(m => m.ModuleID == moduleId);
        //    var screens = modules.FirstOrDefault(m => m.ModuleID == moduleId)?.Screens ?? new List<Screen>();

        //    var model = new ModuleScreenDashboardViewModel
        //    {
        //        Modules = modules,
        //        SelectedModuleId = moduleId,
        //        SelectedModuleCode = selectedModule?.ModuleCode,
        //        SelectedModuleName = selectedModule?.ModuleName,
        //        Screens = screens
        //    };

        //    return PartialView("_ModuleScreensPartial", model);
        //}

        // update_Screen_Order
        //[HttpPost]
        //[ScreenPermission("-")]
        //public async Task<IActionResult> UpdateScreenOrder([FromBody] List<ScreenOrderDto> screens)
        //{
        //    if (screens == null || !screens.Any())
        //        return Json(new { success = false, message = "No data received" });

        //    try
        //    {
        //        await _authRepo.UpdateScreenOrderAsync(screens);
        //        return Json(new { success = true, message = "Screen order updated successfully!" });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error updating screen order");
        //        return Json(new { success = false, message = "Error updating order" });
        //    }
        //}*/

        /* ===== Permission Management ===== */
        [HttpGet]
        [ScreenPermission("-")]
        public async Task<IActionResult> PermissionManagement()
        {
            try
            {
                var users = await _authRepo.GetAllUsersAsync();
                var _modules = await _authRepo.GetAllModulesWithScreenAsync();
                var model = new AssignPermissionViewModel
                {
                    UserId = 0,
                    Permissions = new List<PermissionDto>(),
                    Users = users,
                    Modules = _modules
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading permissions screen");
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        [ScreenPermission("-")]
        public async Task<IActionResult> SearchUsers(string term)
        {
            var users = await _authRepo.GetAllUsersAsync();
            var filtered = users
                .Where(u => u.FullName.Contains(term, StringComparison.OrdinalIgnoreCase) || u.Username.Contains(term, StringComparison.OrdinalIgnoreCase))
                .Select(u => new { id = u.UserId, text = u.Username + " (" + u.FullName + ")" })
                .ToList();
            return Json(filtered);
        }

        [HttpGet]
        [ScreenPermission("-")]
        public async Task<IActionResult> LoadUserPermissionsTree(int userId)
        {
            if (userId <= 0)
                return Json(new { success = false, message = "Invalid user ID" });

            var allModules = await _authRepo.GetAllModulesWithScreenAsync();
            var userPermissions = await _authRepo.LoadScreensList(userId);

            var modules = allModules.Select(m => new PermissionDto
            {
                ModuleID = m.ModuleID,
                ModuleName = m.ModuleName,
                Children = m.Screens.Select(s =>
                {
                    var perm = userPermissions.FirstOrDefault(up => up.ScreenID == s.ScreenID);
                    return new PermissionDto
                    {
                        ScreenID = s.ScreenID,
                        ParentScreenID = s.ParentScreenID,
                        ScreenName = s.ScreenName,
                        ModuleID = m.ModuleID,
                        CanView = perm?.CanView ?? false,
                        CanAdd = perm?.CanAdd ?? false,
                        CanEdit = perm?.CanEdit ?? false,
                        CanDelete = perm?.CanDelete ?? false,
                        CanPrint = perm?.CanPrint ?? false,
                        CanApprove = perm?.CanApprove ?? false,
                        CanCancel = perm?.CanCancel ?? false,
                        CanReject = perm?.CanReject ?? false,
                    };
                }).ToList()
            }).ToList();

            return Json(new { success = true, Modules = modules });
        }

        [HttpGet]
        [ScreenPermission("-")]
        public async Task<IActionResult> GetScreensByModule(int userId, int moduleId)
        {
            var screens = await _authRepo.GetScreensByModuleAsync(userId, moduleId);
            if (screens == null || screens.Count == 0) return Json(new { success = false, data = screens });
            else return Json(new { success = true, data = screens });
        }

        [HttpGet]
        [ScreenPermission("-")]
        public async Task<IActionResult> GetPermissionsByScreen(int userId, int screenId)
        {
            var perms = await _authRepo.GetPermissionsByScreenAsync(userId, screenId);
            // perms = Dictionary<string, bool> with keys: V,N,E,D,P,A,C,R
            return Json(perms);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ScreenPermission("-")]
        public async Task<IActionResult> SaveUserPermissions([FromBody] AssignPermissionViewModel model)
        {
            if (model == null || model.Permissions == null || !model.Permissions.Any())
                return Json(new { success = false, message = "No permissions provided" });

            try
            {
                var result = await _authRepo.SaveUserPermissionAsync(model.UserId, model.Permissions);

                if (result > 0)
                {
                    return Json(new { success = true, message = "Permissions updated successfully." });
                }

                return Json(new { success = false, message = "No changes were saved." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving permissions for user {UserId}", model.UserId);
                return Json(new { success = false, message = "An error occurred while saving permissions." });
            }
        }

        [HttpGet]
        [ScreenPermission("-")]
        public async Task<IActionResult> LoadUserPermissions(int userId)
        {
            try
            {
                var screensWithPermissions = await _authRepo.LoadScreensList(userId);

                var model = new AssignPermissionViewModel
                {
                    UserId = userId,
                    Permissions = screensWithPermissions
                };
                return PartialView("_PermissionGridPartial", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading permissions for user {UserId}", userId);
                return StatusCode(500, $"Error loading permissions: {ex.Message}");
            }
        }



    }
}
