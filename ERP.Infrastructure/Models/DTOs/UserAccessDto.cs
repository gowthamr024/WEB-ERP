using ERP.Infrastructure.Models.Entities;

namespace ERP.Infrastructure.Models.DTOs
{
    public class UserAccessDto
    {
        public int UserID { get; set; }
        public string UserName { get; set; } = string.Empty;
        public List<Module> Modules { get; set; } = new();
        public List<PermissionDto> Permissions { get; set; } = new();
    }
}
