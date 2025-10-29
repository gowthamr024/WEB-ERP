using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERP.Infrastructure.Models.Entities
{
    public class UserModel
    {
        public int UserId { get; set; }
        public int EmplId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = "User";
        public string MobileNumber { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public bool IsBlocked { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class Module
    {
        public int ModuleID { get; set; }
        public string ModuleName { get; set; }
        public string ModuleCode { get; set; }
        public string? Description { get; set; }
        public string Area { get; set; }
        public string DefaultControllerName { get; set; }
        public string DefaultActionName { get; set; }
        public List<Screen> Screens { get; set; } = new();
        public bool IsActive { get; set; }
    }

    public class Screen
    {
        public int ScreenID { get; set; }
        public int? ParentScreenID { get; set; }
        public int ModuleID { get; set; }
        public string ScreenName { get; set; } = string.Empty;
        public string ScreenCode { get; set; } = string.Empty;
        public string ControllerName { get; set; } = string.Empty;
        public string ActionName { get; set; } = string.Empty;
        public int MenuOrder { get; set; }
        public bool IsVisibleInMenu { get; set; }
        public bool IsActive { get; set; }
        public string Area { get; set; }
    }

    public class UserPermission
    {
        public int UserPermissionID { get; set; }
        public int UserID { get; set; }
        public int ScreenID { get; set; }
        public bool CanAdd { get; set; }
        public bool CanEdit { get; set; }
        public bool CanView { get; set; }
        public bool CanDelete { get; set; }
        public bool CanPrint { get; set; }
        public bool CanApprove { get; set; }
        public bool CanCancel { get; set; }
        public bool CanReject { get; set; }
    }

    public class RolePermission
    {
        public int RolePermissionID { get; set; }
        public int RoleID { get; set; }
        public int ScreenID { get; set; }
        public bool CanAdd { get; set; }
        public bool CanEdit { get; set; }
        public bool CanView { get; set; }
        public bool CanDelete { get; set; }
        public bool CanPrint { get; set; }
        public bool CanApprove { get; set; }
        public bool CanCancel { get; set; }
        public bool CanReject { get; set; }
    }

    public class ScreenOrderDto
    {
        public int ScreenID { get; set; }
        public int? TempID { get; set; }
        public int? ParentScreenID { get; set; }
        public int MenuOrder { get; set; }
        public string ScreenName { get; set; }

        public string ControllerName { get; set; }
        public string ActionName { get; set; }
        public int ModuleID { get; set; }
    }
}
