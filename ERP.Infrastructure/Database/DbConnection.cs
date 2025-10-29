using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace ERP.Infrastructure.Database
{
    public class DbConnection : IDisposable
    {
        private bool _disposed = false;
        private readonly IConfiguration _config;
        private readonly string _connectionString;
        private IDbConnection _connection;
        private IDbTransaction _transaction; 
        public IDbTransaction CurrentTransaction => _transaction;
        
        public DbConnection(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _connectionString = _config.GetConnectionString("ERPDatabase")
                ?? throw new InvalidOperationException("Connection string 'ERPDatabase' not found.");
        }

        public IDbConnection Open()
        {
            if (_connection == null || _connection.State != ConnectionState.Open)
            {
                _connection = new SqlConnection(_connectionString);
                _connection.Open();
            }
            return _connection;
        }

        public IDbTransaction BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            if (_transaction != null)
                throw new InvalidOperationException("A transaction is already in progress.");

            _transaction = Open().BeginTransaction(isolationLevel);
            return _transaction;
        }

        public void CommitTransaction()
        {
            if (_transaction == null)
                throw new InvalidOperationException("No transaction is in progress.");

            _transaction.Commit();
            _transaction.Dispose();
            _transaction = null!;
        }

        public void RollbackTransaction()
        {
            if (_transaction == null)
                throw new InvalidOperationException("No transaction is in progress.");

            _transaction.Rollback();
            _transaction?.Dispose();
            _transaction = null!;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    try
                    {
                        if (_transaction != null)
                        {
                            _transaction?.Dispose();
                            _transaction = null;
                        }

                        if (_connection != null)
                        {
                            if (_connection.State != ConnectionState.Closed)
                                _connection.Close();

                            _connection.Dispose();
                            _connection = null!;
                        }
                    }
                    catch
                    {
                        // Avoid throwing exceptions in Dispose
                    }
                }

                _disposed = true;
            }
        }

        ~DbConnection()
        {
            Dispose(false);
        }
    }
}
