/* ReportsController.cs
 * Created on 05-09-2025
 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */
using ERP.Infrastructure.Repositories.Auth;
using ERP.Infrastructure.Services;
using ERP.Web.Attributes;
using ERP.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Areas.HR.Controllers
{
    public class ReportsController : BaseController
    {
        public IAuthRepository _authRepo;
        private readonly IErrorLogger _errorLogger;

        public ReportsController(IAuthRepository authRepo, IErrorLogger errorLogger) : base(authRepo)
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
