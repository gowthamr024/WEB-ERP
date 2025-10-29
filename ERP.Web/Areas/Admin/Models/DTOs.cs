namespace ERP.Web.Areas.Admin.Models
{
    public class ScreenPermissionDto
    {
        public int ScreenID { get; set; }
        public string ScreenName { get; set; }
        public int? ParentScreenID { get; set; }
        public bool CanView { get; set; }
        public bool CanAdd { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public bool CanPrint { get; set; }
        public bool CanApprove { get; set; }
        public bool CanCancel { get; set; }
        public bool CanReject { get; set; }
        public List<ScreenPermissionDto> Children { get; set; } = new();
    }

    public class TreeNode
    {
        public int id { get; set; }
        public string text { get; set; } = string.Empty;
        public List<TreeNode> children { get; set; } = new();
    }



}
