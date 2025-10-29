using ERP.Infrastructure.Models.DTOs;
using ERP.Infrastructure.Models.Entities;

namespace ERP.Web.Areas.Admin.Models
{
    public class AssignPermissionViewModel
    {
        public int UserId { get; set; }
        public List<PermissionDto> Permissions { get; set; } = new();
        public List<UserModel> Users { get; set; }
        public List<Module> Modules { get; set; }
    }
}
