using ERP.Infrastructure.Helpers;
using ERP.Infrastructure.Repositories.Auth;
using ERP.Infrastructure.Services;
using ERP.Web.Areas.Admin.Controllers;
using ERP.Web.Attributes;
using ERP.Web.Controllers;
using ERP.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Areas.Common.Controllers
{
    [Area("Common")]
    public class ProfileController : BaseController
    {
        private readonly IErrorLogger _errorLogger;
        public IAuthRepository _authRepo;
        public ProfileController(IAuthRepository authRepo, IErrorLogger errorLogger) : base(authRepo)
        {
            _authRepo = authRepo ?? throw new ArgumentNullException(nameof(authRepo));
            _errorLogger = errorLogger ?? throw new ArgumentNullException(nameof(errorLogger));
        }

        [HttpGet("Dashboard")]
        [ScreenPermission("-")]
        public IActionResult Dashboard()
        {
            return View();
        }
    }
}
