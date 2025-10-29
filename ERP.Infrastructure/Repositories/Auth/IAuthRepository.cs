using ERP.Infrastructure.Models.DTOs;
using ERP.Infrastructure.Models.Entities;

namespace ERP.Infrastructure.Repositories.Auth
{
    public interface IAuthRepository
    {
        Task<UserModel> ValidateUserAsync(string username, string password); //ok
        Task<int> CreateUserAsync(UserModel userModel); //ok
        Task<List<UserModel>> GetAllUsersAsync();
        Task<bool> CheckPermissionAsync(int? userId, string Controller, string Action, string permissionType); //single action check
        Task<PermissionDto?> GetUserPermissionsAsync(int userId, string controller, string action); //screen overall permissions
        Task<List<Module>> GetModulesWithScreensByUserIDAsync(int? userId); //Module + Screen list
        Task<List<Module>> GetAllModulesWithScreenAsync();
        Task<int> AddModuleAsync(Module module);
        Task<int> UpdateModuleAsync(Module module);
        Task<int> DeleteModuleAsync(int ModuleID);
        Task<int> UpdateScreenHierarchyAsync(List<ScreenOrderDto> screens);
        Task<int> AddScreenAsync(Screen screen);
        Task<int> UpdateScreenAsync(Screen screen);
        Task<List<PermissionDto>> LoadScreensList(int userId);
        Task<List<ScreenDto>> GetScreensByModuleAsync(int userId, int moduleId);
        Task<Dictionary<string, bool>> GetPermissionsByScreenAsync(int userId, int screenId);
        Task<int> SaveUserPermissionAsync(int userId, List<PermissionDto> permissions);
    }

}
