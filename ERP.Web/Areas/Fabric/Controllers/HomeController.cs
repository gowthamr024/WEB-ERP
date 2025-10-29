using ERP.Infrastructure.Repositories.Auth;
using ERP.Infrastructure.Repositories.Fabric;
using ERP.Web.Attributes;
using ERP.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Areas.Fabric.Controllers
{
    [Area("Fabric")]
    public class HomeController : BaseController
    {
        private readonly IFabricRepository _fabRepo;
        public IAuthRepository _AuthRepo;

        public HomeController(IAuthRepository authRepo, IFabricRepository repo) : base(authRepo)
        {
            _fabRepo = repo;
            _AuthRepo = authRepo;
        }

        [ScreenPermission("V")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
