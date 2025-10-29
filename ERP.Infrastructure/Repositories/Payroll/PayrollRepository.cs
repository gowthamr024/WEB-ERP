using ERP.Infrastructure.Database;
using ERP.Infrastructure.Helpers;
using ERP.Infrastructure.Models.Entities;
using ERP.Infrastructure.Services;

namespace ERP.Infrastructure.Repositories.Payroll
{
    public class PayrollRepository : BaseRepository, IPayrollRepository
    {
        private readonly IErrorLogger _errorLogger;
        private readonly PermissionCache _cache;
        private readonly DbConnection _dbConnection;
        public PayrollRepository(DbConnection dbConnection, IErrorLogger errorLogger, PermissionCache cache)
    : base(dbConnection)
        {
            _errorLogger = errorLogger ?? throw new ArgumentNullException(nameof(errorLogger));
            _cache = cache;
            _dbConnection = dbConnection ?? throw new ArgumentNullException(nameof(dbConnection));
        }

        public async Task<List<EmployeeMaster>> getAllEmployees()
        {
            return new List<EmployeeMaster>();
        }
    }
}
