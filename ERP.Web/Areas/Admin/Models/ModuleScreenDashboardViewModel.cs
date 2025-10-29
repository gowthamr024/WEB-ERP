using ERP.Infrastructure.Models.Entities;

namespace ERP.Web.Areas.Admin.Models
{
    public class ModuleScreenDashboardViewModel
    {
        public List<Module> Modules { get; set; } = new();
        public int? SelectedModuleId { get; set; }
        public string? SelectedModuleCode { get; set; }
        public String? SelectedModuleName { get; set; }
        public List<Screen> Screens { get; set; } = new();

        public Module NewModule { get; set; } = new();
        public Screen NewScreen { get; set; } = new();
    }
}
