using ERP.Infrastructure.Repositories.Auth;
using ERP.Infrastructure.Services;
using ERP.Web.Attributes;
using ERP.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Areas.HR.Controllers
{
    public class AttendanceController : BaseController
    {
        public IAuthRepository _authRepo;
        private readonly IErrorLogger _errorLogger;

        public AttendanceController(IAuthRepository authRepo, IErrorLogger errorLogger) : base(authRepo)
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
