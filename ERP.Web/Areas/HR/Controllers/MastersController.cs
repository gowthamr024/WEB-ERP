using ERP.Web.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Areas.HR.Controllers
{
    public class MastersController : Controller
    {
        [ScreenPermission("V")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
