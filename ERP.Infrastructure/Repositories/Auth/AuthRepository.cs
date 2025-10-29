using ERP.Infrastructure.Database;
using ERP.Infrastructure.Helpers;
using ERP.Infrastructure.Models.DTOs;
using ERP.Infrastructure.Models.Entities;
using ERP.Infrastructure.Services;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ERP.Infrastructure.Repositories.Auth
{
    public class AuthRepository : BaseRepository, IAuthRepository
    {
        private readonly IErrorLogger _errorLogger;
        private readonly PermissionCache _cache;
        private readonly DbConnection _dbConnection;

        public AuthRepository(DbConnection dbConnection, IErrorLogger errorLogger, PermissionCache cache)
    : base(dbConnection)
        {
            _errorLogger = errorLogger ?? throw new ArgumentNullException(nameof(errorLogger));
            _cache = cache;
            _dbConnection = dbConnection ?? throw new ArgumentNullException(nameof(dbConnection));
        }

        public async Task<UserModel> ValidateUserAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username cannot be empty.", nameof(username));
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be empty.", nameof(password));

            const string Qry = @"SELECT UserID, Username, FullName, Email, MobileNumber, R.RoleName Role, u.IsActive, PasswordHash
                                FROM Users U
                                LEFT JOIN [Roles] R on u.RoleID = R.RoleID
                                WHERE u.Username = @Username";
            try
            {
                using var cmd = CreateCommand(Qry, CommandType.Text);
                AddParameter(cmd, "@Username", username);
                //AddParameter(cmd, "@Password", password);

                //var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var storedHash = reader.GetString(reader.GetOrdinal("PasswordHash"));
                    if (!BCrypt.Net.BCrypt.Verify(password, storedHash))
                        return null!;

                    return new UserModel
                    {
                        UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                        Username = reader.GetString(reader.GetOrdinal("Username")),
                        FullName = reader.GetString(reader.GetOrdinal("FullName")),
                        Email = reader.GetString(reader.GetOrdinal("Email")),
                        MobileNumber = reader.GetString(reader.GetOrdinal("MobileNumber")),
                        Role = reader.GetString(reader.GetOrdinal("Role")),
                        IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))
                    };
                }
                return null!;
            }
            catch (Exception ex)
            {
                _errorLogger.Log(ex, Title: "DB operation failed");
                return null!;
            }
        }

        public async Task<int> CreateUserAsync(UserModel userModel)
        {
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(userModel.PasswordHash);

            const string Qry = @"
                INSERT INTO Users (EmplID, Username, PasswordHash, Email, Role, FullName)
                VALUES (@EmplID, @Username, @PasswordHash, @Email, @Role, @FullName)";

            using var cmd = CreateCommand(Qry, CommandType.Text);
            AddParameter(cmd, "@EmplID", userModel.EmplId);
            AddParameter(cmd, "@Username", userModel.Username);
            AddParameter(cmd, "@FullName", userModel.FullName);
            AddParameter(cmd, "@PasswordHash", passwordHash);
            AddParameter(cmd, "@Email", userModel.Email);
            AddParameter(cmd, "@Role", userModel.Role);
            try
            {
                return await ExecuteSingleQueryAsync(cmd);
            }
            catch
            {
                return 0;
            }
        }

        public async Task<List<UserModel>> GetAllUsersAsync()
        {
            var Users = new List<UserModel>();
            var cmd = CreateCommand("Select UserID, UserName, FullName from Users where IsActive = 1 and IsBlocked = 0", CommandType.Text);
            try
            {
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    Users.Add(new UserModel
                    {
                        UserId = reader.GetInt32(reader.GetOrdinal("UserID")),
                        Username = reader.GetString(reader.GetOrdinal("UserName")),
                        FullName = reader.GetString(reader.GetOrdinal("FullName"))
                    });
                }

                return Users;
            }
            catch (Exception ex)
            {
                _errorLogger.Log(ex, Title: "DB operation failed");
                return new List<UserModel>();
            }
        }

        public async Task<bool> CheckPermissionAsync(int? userId, string Controller, string Action, string permissionType)
        {
            // Version 1.0 -- individual permission check db level
            //string cacheKey = $"{userId}:{Controller}:{Action}";
            //if (_cache.TryGet(cacheKey, out PermissionDto cached))
            //    return permissionType switch
            //    {
            //        "N" => cached.CanAdd,
            //        "E" => cached.CanEdit,
            //        "V" => cached.CanView,
            //        "D" => cached.CanDelete,
            //        "P" => cached.CanPrint,
            //        "A" => cached.CanApprove,
            //        "C" => cached.CanCancel,
            //        "R" => cached.CanReject,
            //        _ => false
            //    }
            //    ;
            //var cmd = CreateCommand("usp_CheckPermission", CommandType.StoredProcedure);
            //AddParameter(cmd, "@UserId", userId);
            //AddParameter(cmd, "@ControllerName", Controller);
            //AddParameter(cmd, "@ActionName", Action);
            //AddParameter(cmd, "@PermissionType", permissionType);
            //try
            //{
            //    var result = await cmd.ExecuteScalarAsync();
            //    return result != null && Convert.ToInt32(result) == 1;
            //}
            //catch
            //{
            //    return false;
            //}

            // Version 2 -- One Query per screen and cache it.

            if (permissionType == "-") { return true; }

            var dto = await GetUserScreenPermissionsAsync(userId, Controller, Action);
            if (dto == null) return false;
            return permissionType switch
            {
                "N" => dto.CanAdd,
                "E" => dto.CanEdit,
                "V" => dto.CanView,
                "D" => dto.CanDelete,
                "P" => dto.CanPrint,
                "A" => dto.CanApprove,
                "C" => dto.CanCancel,
                "R" => dto.CanReject,
                _ => false
            };
        }

        public async Task<PermissionDto?> GetUserScreenPermissionsAsync(int? userId, string controller, string action)
        {
            string cacheKey = $"{userId}:{controller.ToLower()}:{action.ToLower()}";

            if (_cache.TryGet(cacheKey, out PermissionDto? cached))
                return cached;

            var cmd = CreateCommand("sp_GetUserScreenPermissions", CommandType.StoredProcedure);
            AddParameter(cmd, "@UserId", (object?)userId ?? DBNull.Value);
            AddParameter(cmd, "@Controller", controller);
            AddParameter(cmd, "@Action", action);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var dto = new PermissionDto
                {
                    ScreenID = reader.GetInt32(reader.GetOrdinal("ScreenID")),
                    ScreenName = reader["ScreenName"].ToString() ?? string.Empty,
                    CanAdd = reader.GetBoolean(reader.GetOrdinal("CanAdd")),
                    CanEdit = reader.GetBoolean(reader.GetOrdinal("CanEdit")),
                    CanView = reader.GetBoolean(reader.GetOrdinal("CanView")),
                    CanDelete = reader.GetBoolean(reader.GetOrdinal("CanDelete")),
                    CanPrint = reader.GetBoolean(reader.GetOrdinal("CanPrint")),
                    CanApprove = reader.GetBoolean(reader.GetOrdinal("CanApprove")),
                    CanCancel = reader.GetBoolean(reader.GetOrdinal("CanCancel")),
                    CanReject = reader.GetBoolean(reader.GetOrdinal("CanReject"))
                };

                _cache.Set(cacheKey, dto, TimeSpan.FromMinutes(60));

                return dto;
            }

            return null;
        }

        public async Task<PermissionDto?> GetUserPermissionsAsync(int userId, string controller, string action)
        {
            string key = $"{userId}:{controller.ToLower()}:{action.ToLower()}";

            // Check cache first
            if (_cache.TryGet(key, out PermissionDto? cached))
                return cached;

            PermissionDto? dto = null;

            using var cmd = CreateCommand("sp_GetUserScreenPermissions", CommandType.StoredProcedure);
            AddParameter(cmd, "@UserId", userId);
            AddParameter(cmd, "@Controller", controller);
            AddParameter(cmd, "@Action", action);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                dto = new PermissionDto
                {
                    ScreenID = reader.GetInt32(reader.GetOrdinal("ScreenID")),
                    ScreenName = reader.GetString(reader.GetOrdinal("ScreenName")),
                    CanAdd = reader.GetBoolean(reader.GetOrdinal("CanAdd")),
                    CanEdit = reader.GetBoolean(reader.GetOrdinal("CanEdit")),
                    CanView = reader.GetBoolean(reader.GetOrdinal("CanView")),
                    CanDelete = reader.GetBoolean(reader.GetOrdinal("CanDelete")),
                    CanPrint = reader.GetBoolean(reader.GetOrdinal("CanPrint")),
                    CanApprove = reader.GetBoolean(reader.GetOrdinal("CanApprove")),
                    CanCancel = reader.GetBoolean(reader.GetOrdinal("CanCancel")),
                    CanReject = reader.GetBoolean(reader.GetOrdinal("CanReject"))
                };
            }

            // ✅ Save to cache
            if (dto != null)
                _cache.Set(key, dto, TimeSpan.FromMinutes(60));

            return dto;
        }

        public async Task<List<Module>> GetModulesWithScreensByUserIDAsync(int? userId)
        {
            var modules = new List<Module>();

            var cmd = CreateCommand("usp_modulesaccesslistwithscreens", CommandType.StoredProcedure);
            AddParameter(cmd, "@UserID", (object?)userId ?? DBNull.Value);
            try
            {
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        int moduleId = reader.GetInt32(reader.GetOrdinal("ModuleID"));
                        var module = modules.FirstOrDefault(m => m.ModuleID == moduleId);

                        if (module == null)
                        {
                            module = new Module
                            {
                                ModuleID = moduleId,
                                ModuleName = reader.GetString(reader.GetOrdinal("ModuleName")),
                                DefaultControllerName = reader.GetString(reader.GetOrdinal("DefaultController")),
                                DefaultActionName = reader.GetString(reader.GetOrdinal("DefaultAction")),
                                Area = reader.GetString(reader.GetOrdinal("Area"))
                            };
                            modules.Add(module);
                        }

                        module.Screens.Add(new Screen
                        {
                            ScreenID = reader.GetInt32(reader.GetOrdinal("ScreenID")),
                            ParentScreenID = reader.IsDBNull(reader.GetOrdinal("ParentScreenID")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("ParentScreenID")),
                            ScreenName = reader.GetString(reader.GetOrdinal("ScreenName")),
                            ControllerName = reader.GetString(reader.GetOrdinal("Controller")),
                            ActionName = reader.GetString(reader.GetOrdinal("Action")),
                            IsVisibleInMenu = reader.GetBoolean(reader.GetOrdinal("IsVisibleInMenu"))
                        });

                        //caching Permissions 
                        var dto = new PermissionDto
                        {
                            ScreenID = reader.GetInt32(reader.GetOrdinal("ScreenID")),
                            ScreenName = reader["ScreenName"].ToString() ?? string.Empty,
                            CanAdd = reader.GetBoolean(reader.GetOrdinal("CanAdd")),
                            CanEdit = reader.GetBoolean(reader.GetOrdinal("CanEdit")),
                            CanView = reader.GetBoolean(reader.GetOrdinal("CanView")),
                            CanDelete = reader.GetBoolean(reader.GetOrdinal("CanDelete")),
                            CanPrint = reader.GetBoolean(reader.GetOrdinal("CanPrint")),
                            CanApprove = reader.GetBoolean(reader.GetOrdinal("CanApprove")),
                            CanCancel = reader.GetBoolean(reader.GetOrdinal("CanCancel")),
                            CanReject = reader.GetBoolean(reader.GetOrdinal("CanReject"))
                        };

                        string cacheKey = $"{userId}:{reader.GetString(reader.GetOrdinal("Controller")).ToLower()}:{reader.GetString(reader.GetOrdinal("Action")).ToLower()}";
                        _cache.Set(cacheKey, dto, TimeSpan.FromMinutes(60));
                    }
                }

                return modules;
            }
            catch (Exception ex)
            {
                _errorLogger.Log(ex, Title: "DB operation failed");
                return new List<Module>();
            }
        }

        //=========== Menu control ==========
        public async Task<List<Module>> GetAllModulesWithScreenAsync()
        {
            var modules = new List<Module>();
            var cmd = CreateCommand("usp_getallmoduleswithscreens", CommandType.StoredProcedure);

            try
            {
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    int moduleId = reader.GetInt32(reader.GetOrdinal("ModuleID"));
                    var module = modules.FirstOrDefault(m => m.ModuleID == moduleId);

                    if (module == null)
                    {
                        module = new Module
                        {
                            ModuleID = moduleId,
                            ModuleName = reader.GetString(reader.GetOrdinal("ModuleName")),
                            ModuleCode = reader.GetString(reader.GetOrdinal("ModuleCode")),
                            DefaultControllerName = reader.GetString(reader.GetOrdinal("DefaultController")),
                            DefaultActionName = reader.GetString(reader.GetOrdinal("DefaultAction"))
                        };
                        modules.Add(module);
                    }
                    if (reader.GetInt32(reader.GetOrdinal("ScreenID")) != 0)
                    {
                        module.Screens.Add(new Screen
                        {
                            ScreenID = reader.GetInt32(reader.GetOrdinal("ScreenID")),
                            ParentScreenID = reader.GetInt32(reader.GetOrdinal("ParentScreenID")),
                            ScreenName = reader.GetString(reader.GetOrdinal("ScreenName")),
                            ControllerName = reader.GetString(reader.GetOrdinal("Controller")),
                            ActionName = reader.GetString(reader.GetOrdinal("Action")),
                            MenuOrder = reader.GetInt32(reader.GetOrdinal("MenuOrder"))
                        });
                    }
                }
                return modules;
            }
            catch (Exception ex)
            {
                _errorLogger.Log(ex, Title: "DB operation failed");
                return null!;
            }
        }

        public async Task<int> AddModuleAsync(Module module)
        {
            const string qry = @"
        INSERT INTO Modules (ModuleName, ModuleCode, DefaultController, DefaultAction)
        VALUES (@ModuleName, @ModuleCode, @DefaultController, @DefaultAction)";

            using var cmd = CreateCommand(qry, CommandType.Text);
            AddParameter(cmd, "@ModuleName", module.ModuleName);
            AddParameter(cmd, "@ModuleCode", module.ModuleCode);
            AddParameter(cmd, "@DefaultController", module.DefaultControllerName);
            AddParameter(cmd, "@DefaultAction", module.DefaultActionName);

            try
            {
                return await ExecuteSingleQueryAsync(cmd);
            }
            catch (Exception ex)
            {
                _errorLogger.Log(ex, Title: "DB operation failed");
                return 0;
            }
        }

        public async Task<int> UpdateModuleAsync(Module module)
        {
            const string qry = @" UPDATE Modules SET ModuleName = @ModuleName, ModuleCode = @ModuleCode, DefaultController = @DefaultController, DefaultAction = @DefaultAction WHERE ModuleID = @ModuleID";

            using var cmd = CreateCommand(qry, CommandType.Text);
            AddParameter(cmd, "@ModuleID", module.ModuleID);
            AddParameter(cmd, "@ModuleName", module.ModuleName);
            //AddParameter(cmd, "@ModuleCode", module.ModuleCode);
            AddParameter(cmd, "@DefaultController", module.DefaultControllerName);
            AddParameter(cmd, "@DefaultAction", module.DefaultActionName);

            try
            {
                return await ExecuteSingleQueryAsync(cmd);
            }
            catch (Exception ex)
            {
                _errorLogger.Log(ex, Title: "DB operation failed");
                return 0;
            }
        }

        public async Task<int> DeleteModuleAsync(int ModuleID)
        {
            return 0;
        }

        public async Task<int> UpdateScreenHierarchyAsync(List<ScreenOrderDto> screens)
        {
            try
            {
                using var cmd = CreateCommand("dbo.sp_SaveMenuTree", CommandType.StoredProcedure);

                var dt = new DataTable();
                dt.Columns.Add("ScreenID", typeof(int));
                dt.Columns.Add("ScreenName", typeof(string));
                dt.Columns.Add("ParentScreenID", typeof(int));
                dt.Columns.Add("MenuOrder", typeof(int));
                dt.Columns.Add("ControllerName", typeof(string));
                dt.Columns.Add("ActionName", typeof(string));
                dt.Columns.Add("ModuleID", typeof(int));

                foreach (var s in screens)
                {
                    dt.Rows.Add(
                        s.ScreenID,
                        s.ScreenName,
                        s.ParentScreenID.HasValue && s.ParentScreenID.Value > 0 ? s.ParentScreenID.Value : DBNull.Value,
                        s.MenuOrder,
                        s.ControllerName ?? (object)DBNull.Value,
                        s.ActionName ?? (object)DBNull.Value,
                        s.ModuleID
                    );
                }

                // Add TVP parameter
                var param = cmd.Parameters.AddWithValue("@Screens", dt);
                param.SqlDbType = SqlDbType.Structured;
                param.TypeName = "dbo.ScreenHierarchyType";

                return await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                _errorLogger.Log(ex, Title: "DB operation failed");
                return 0;
            }
        }

        public async Task<int> AddScreenAsync(Screen screen)
        {
            using var cmd = CreateCommand("usp_InsertScreen", CommandType.StoredProcedure);
            AddParameter(cmd, "@ScreenName", screen.ScreenName);
            AddParameter(cmd, "@ModuleID", screen.ModuleID);
            AddParameter(cmd, "@ScreenCode", screen.ScreenCode);
            AddParameter(cmd, "@ControllerName", screen.ControllerName);
            AddParameter(cmd, "@ActionName", screen.ActionName);
            AddParameter(cmd, "@MenuOrder", screen.MenuOrder);
            AddParameter(cmd, "@IsActive", screen.IsActive);
            AddParameter(cmd, "@ParentScreenID", screen.ParentScreenID!);
            AddParameter(cmd, "@IsVisibleInMenu", screen.IsVisibleInMenu);
            AddParameter(cmd, "@Area", screen.Area);
            var outpara = new SqlParameter("@NewScreenID", SqlDbType.Int) { Direction = ParameterDirection.Output };
            cmd.Parameters.Add(outpara);

            try
            {
                return await ExecuteSingleQueryAsync(cmd);
            }
            catch (Exception ex)
            {
                _errorLogger.Log(ex, Title: "DB operation failed");
                return 0;
            }
        }

        public async Task<int> UpdateScreenAsync(Screen screen)
        {

            using var cmd = CreateCommand("usp_UpdateScreen", CommandType.StoredProcedure);
            AddParameter(cmd, "@ScreenID", screen.ScreenID);
            AddParameter(cmd, "@ScreenName", screen.ScreenName);
            AddParameter(cmd, "@ModuleID", screen.ModuleID);
            AddParameter(cmd, "@ScreenCode", screen.ScreenCode);
            AddParameter(cmd, "@ControllerName", screen.ControllerName);
            AddParameter(cmd, "@ActionName", screen.ActionName);
            AddParameter(cmd, "@IsActive", screen.IsActive);
            AddParameter(cmd, "@MenuOrder", screen.MenuOrder);
            AddParameter(cmd, "@ParentScreenID", (object)screen.ParentScreenID! ?? DBNull.Value);
            AddParameter(cmd, "@IsVisibleInMenu", screen.IsVisibleInMenu);
            AddParameter(cmd, "@Area", screen.Area);

            try
            {
                return await ExecuteSingleQueryAsync(cmd);
            }
            catch (Exception ex)
            {
                _errorLogger.Log(ex, Title: "DB operation failed");
                return 0;
            }
        }

        public async Task<int> DeleteScreen(int screenId)
        {
            using var cmd = CreateCommand("usp_DeleteScreen", CommandType.StoredProcedure);
            AddParameter(cmd, "@ScreenID", screenId);
            try
            {
                return await ExecuteSingleQueryAsync(cmd);
            }
            catch (Exception ex)
            {
                _errorLogger.Log(ex, Title: "DB operation failed");
                return 0;
            }
        }

        public async Task<int> BulkUpdateParentOrder(IEnumerable<(int ScreenID, int? ParentScreenID, int MenuOrder)> updates)
        {
            var dt = new DataTable();
            dt.Columns.Add("ScreenID", typeof(int));
            dt.Columns.Add("ParentScreenID", typeof(int));
            dt.Columns.Add("MenuOrder", typeof(int));

            foreach (var u in updates)
            {
                var parentVal = u.ParentScreenID.HasValue ? (object)u.ParentScreenID.Value : DBNull.Value;
                dt.Rows.Add(u.ScreenID, parentVal, u.MenuOrder);
            }

            using var cmd = CreateCommand("usp_BulkUpdateScreenParentOrder", CommandType.StoredProcedure);
            var p = cmd.Parameters.AddWithValue("@Updates", dt);
            p.SqlDbType = SqlDbType.Structured;
            p.TypeName = "dbo.ScreenUpdateType";

            try
            {
                return await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                _errorLogger.Log(ex, Title: "DB operation failed");
                return 0;
            }
        }

        public async Task<List<PermissionDto>> LoadScreensList(int userId)
        {
            var permissions = new List<PermissionDto>();

            const string qry = @"
                            SELECT 
                                s.ScreenID,
                                s.ParentScreenID,
                                s.ScreenName,
                                s.ModuleID,
                                m.ModuleName,
                                ISNULL(up.CanAdd, 0) AS CanAdd,
                                ISNULL(up.CanEdit, 0) AS CanEdit,
                                ISNULL(up.CanView, 0) AS CanView,
                                ISNULL(up.CanDelete, 0) AS CanDelete,
                                ISNULL(up.CanPrint, 0) AS CanPrint,
                                ISNULL(up.CanApprove, 0) AS CanApprove,
                                ISNULL(up.CanCancel, 0) AS CanCancel,
                                ISNULL(up.CanReject, 0) AS CanReject
                            FROM Screens s
                            INNER JOIN Modules m ON s.ModuleID = m.ModuleID
                            LEFT JOIN UserPermissions up ON up.ScreenID = s.ScreenID AND up.UserID = @UserID
                            WHERE s.IsActive = 1
                            ORDER BY m.ModuleName, s.MenuOrder;";

            using var cmd = CreateCommand(qry, CommandType.Text);
            AddParameter(cmd, "@UserID", userId);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                permissions.Add(new PermissionDto
                {

                    ScreenID = reader.GetInt32(reader.GetOrdinal("ScreenID")),
                    ParentScreenID = reader.IsDBNull(reader.GetOrdinal("ParentScreenID")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("ParentScreenID")),
                    ScreenName = reader.GetString(reader.GetOrdinal("ScreenName")),
                    ModuleID = reader.GetInt32(reader.GetOrdinal("ModuleID")),
                    ModuleName = reader.GetString(reader.GetOrdinal("ModuleName")),
                    CanAdd = reader.GetBoolean(reader.GetOrdinal("CanAdd")),
                    CanEdit = reader.GetBoolean(reader.GetOrdinal("CanEdit")),
                    CanView = reader.GetBoolean(reader.GetOrdinal("CanView")),
                    CanDelete = reader.GetBoolean(reader.GetOrdinal("CanDelete")),
                    CanPrint = reader.GetBoolean(reader.GetOrdinal("CanPrint")),
                    CanApprove = reader.GetBoolean(reader.GetOrdinal("CanApprove")),
                    CanCancel = reader.GetBoolean(reader.GetOrdinal("CanCancel")),
                    CanReject = reader.GetBoolean(reader.GetOrdinal("CanReject"))
                });
            }

            return permissions;
        }

        public async Task<List<ScreenDto>> GetScreensByModuleAsync(int userId, int moduleId)
        {
            var screens = new List<ScreenDto>();

            using var cmd = CreateCommand("GetScreensByModule", CommandType.StoredProcedure);
            AddParameter(cmd, "@UserID", userId);
            AddParameter(cmd, "@ModuleID", moduleId);

            try
            {
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    screens.Add(new ScreenDto
                    {
                        ScreenID = reader.GetInt32(reader.GetOrdinal("ScreenID")),
                        ScreenName = reader.GetString(reader.GetOrdinal("ScreenName")),
                        ControllerName = reader.IsDBNull(reader.GetOrdinal("ControllerName")) ? null : reader.GetString(reader.GetOrdinal("ControllerName")),
                        ActionName = reader.IsDBNull(reader.GetOrdinal("ActionName")) ? null : reader.GetString(reader.GetOrdinal("ActionName")),
                        IsAssigned = reader.GetBoolean(reader.GetOrdinal("IsAssigned"))
                    });
                }
                return screens;
            }
            catch (Exception ex)
            {
                _errorLogger.Log(ex, Title: "DB operation failed in GetScreensByModuleAsync");
                return new List<ScreenDto>();
            }
        }

        public async Task<Dictionary<string, bool>> GetPermissionsByScreenAsync(int userId, int screenId)
        {
            var result = new Dictionary<string, bool>();

            const string qry = @"
        SELECT 
            ISNULL(up.CanView, 0) AS CanView,
            ISNULL(up.CanAdd, 0) AS CanAdd,
            ISNULL(up.CanEdit, 0) AS CanEdit,
            ISNULL(up.CanDelete, 0) AS CanDelete,
            ISNULL(up.CanPrint, 0) AS CanPrint,
            ISNULL(up.CanApprove, 0) AS CanApprove,
            ISNULL(up.CanCancel, 0) AS CanCancel,
            ISNULL(up.CanReject, 0) AS CanReject
        FROM Screens s
        LEFT JOIN UserPermissions up 
            ON up.ScreenID = s.ScreenID AND up.UserID = @UserID
        WHERE s.ScreenID = @ScreenID;";

            using var cmd = CreateCommand(qry, CommandType.Text);
            AddParameter(cmd, "@UserID", userId);
            AddParameter(cmd, "@ScreenID", screenId);

            try
            {
                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    result["V"] = reader.GetBoolean(reader.GetOrdinal("CanView"));
                    result["N"] = reader.GetBoolean(reader.GetOrdinal("CanAdd"));
                    result["E"] = reader.GetBoolean(reader.GetOrdinal("CanEdit"));
                    result["D"] = reader.GetBoolean(reader.GetOrdinal("CanDelete"));
                    result["P"] = reader.GetBoolean(reader.GetOrdinal("CanPrint"));
                    result["A"] = reader.GetBoolean(reader.GetOrdinal("CanApprove"));
                    result["C"] = reader.GetBoolean(reader.GetOrdinal("CanCancel"));
                    result["R"] = reader.GetBoolean(reader.GetOrdinal("CanReject"));
                }
                return result;
            }
            catch (Exception ex)
            {
                _errorLogger.Log(ex, Title: "DB operation failed in GetPermissionsByScreenAsync");
                return new Dictionary<string, bool>();
            }
        }


        public async Task<int> SaveUserPermissionAsync(int userId, List<PermissionDto> permissions)
        {
            if (permissions == null)
                throw new ArgumentNullException(nameof(permissions));

            _dbConnection.BeginTransaction();

            try
            {
                using var cmd = CreateCommand("SaveUserPermissions", CommandType.StoredProcedure);

                var tvp = new DataTable();
                tvp.Columns.Add("ScreenID", typeof(int));
                tvp.Columns.Add("CanAdd", typeof(bool));
                tvp.Columns.Add("CanEdit", typeof(bool));
                tvp.Columns.Add("CanView", typeof(bool));
                tvp.Columns.Add("CanDelete", typeof(bool));
                tvp.Columns.Add("CanPrint", typeof(bool));
                tvp.Columns.Add("CanApprove", typeof(bool));
                tvp.Columns.Add("CanCancel", typeof(bool));
                tvp.Columns.Add("CanReject", typeof(bool));

                foreach (var dto in permissions)
                {
                    tvp.Rows.Add(
                        dto.ScreenID,
                        dto.CanAdd,
                        dto.CanEdit,
                        dto.CanView,
                        dto.CanDelete,
                        dto.CanPrint,
                        dto.CanApprove,
                        dto.CanCancel,
                        dto.CanReject
                    );
                }

                AddParameter(cmd, "@UserID", userId);
                var tvpParam = cmd.Parameters.AddWithValue("@Permissions", tvp);
                tvpParam.SqlDbType = SqlDbType.Structured;
                tvpParam.TypeName = "TVP_Permissions";

                int result = await ExecuteSingleQueryAsync(cmd);

                _dbConnection.CommitTransaction();

                _cache.ClearByUser(userId);
                return 1;
            }
            catch (Exception ex)
            {
                _dbConnection.RollbackTransaction();
                _errorLogger.Log(ex, Title: "Failed to save user permissions");
                return -1;
            }
        }


    }
}
