namespace ERP.Infrastructure.Models.DTOs
{
    public class PermissionDto
    {
        public int ScreenID { get; set; }
        public int? ParentScreenID { get; set; }
        public string ScreenName { get; set; }
        public int ModuleID { get; set; }
        public string ModuleName { get; set; }

        public bool CanAdd { get; set; }
        public bool CanEdit { get; set; }
        public bool CanView { get; set; }
        public bool CanDelete { get; set; }
        public bool CanPrint { get; set; }
        public bool CanApprove { get; set; }
        public bool CanCancel { get; set; }
        public bool CanReject { get; set; }

        public List<PermissionDto> Children { get; set; } = new();
    }
    public class ScreenDto
    {
        public int ScreenID { get; set; }
        public string ScreenName { get; set; } = string.Empty;
        public string? ControllerName { get; set; } = string.Empty;
        public string? ActionName { get; set; } = string.Empty;
        public bool IsAssigned { get; set; }
    }
}
