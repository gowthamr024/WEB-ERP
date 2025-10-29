using ERP.Infrastructure.Repositories.Auth;
using ERP.Infrastructure.Services;
using ERP.Web.Attributes;
using ERP.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Areas.Admin.Controllers
{
    public class SettingsController : BaseController
    {
        public IAuthRepository _authRepo;
        private readonly IErrorLogger _errorLogger;

        public SettingsController(IAuthRepository authRepo, IErrorLogger errorLogger) : base(authRepo)
        {
            _authRepo = authRepo ?? throw new ArgumentNullException(nameof(authRepo));
            _errorLogger = errorLogger ?? throw new ArgumentNullException(nameof(errorLogger));
        }

        [ScreenPermission("V")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
