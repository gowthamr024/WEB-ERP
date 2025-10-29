using ERP.Infrastructure.Database;
using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Data.SqlTypes;
using System.Threading.Tasks;


namespace ERP.Infrastructure.Repositories
{
    public abstract class BaseRepository
    {
        private readonly DbConnection _dbConnection;

        /// <summary>
        /// Use this template for any DB execution
        /// 
        ///     BeginTransaction();
        ///     try
        ///     {
        ///         using var cmd = CreateCommand('parameters');
        ///         AddParameter(cmd, @parameter1, value);
        ///         ExecuteNonQuery(cmd);
        /// 
        ///         CommitTransaction();
        ///     }
        ///     catch
        ///     {
        ///         RollbackTransaction();
        ///         throw;
        ///     }
        /// 
        /// </summary>

        protected BaseRepository(DbConnection dbConnection)
        {
            _dbConnection = dbConnection ?? throw new ArgumentNullException(nameof(dbConnection));
        }

        protected SqlCommand CreateCommand(string Query, CommandType commandType = CommandType.Text)
        {
            if (string.IsNullOrWhiteSpace(Query))
                throw new ArgumentException("Stored procedure name / Query cannot be null or empty.", nameof(Query));

            var cmd = new SqlCommand(Query, (SqlConnection)_dbConnection.Open())
            {
                CommandType = commandType
            };

            if (_dbConnection.CurrentTransaction != null)
                cmd.Transaction = (SqlTransaction)_dbConnection.CurrentTransaction;

            return cmd;
        }

        protected void AddParameter(SqlCommand cmd, string Parameter_name, object value)
        {
            cmd.Parameters.AddWithValue(Parameter_name, value ?? DBNull.Value);
        }

        protected async Task<int> ExecuteSingleQueryAsync(SqlCommand command)
        {
            return await command.ExecuteNonQueryAsync(); //Executes a command and returns number of affected rows.     
        }

        protected async Task<T> ExecuteScalarAsync<T>(SqlCommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));
            
            object? result = await command.ExecuteScalarAsync();
            if (result == null || result == DBNull.Value)
                return default!;

            if (result is T variable)
                return variable;

            return (T)Convert.ChangeType(result, typeof(T));
            // Executes a scalar query and returns the result with the type mentioned.
        }

        protected async Task<DataTable> ExecuteDataTableAsync(SqlCommand command)
        {
            using var adapter = new SqlDataAdapter(command);
            var dataTable = new DataTable();
            await Task.Run(() => adapter.Fill(dataTable));
            return dataTable;
        }

        protected async Task<SqlBoolean> Check_ObjectAsync(string ObjectName)
        {
            const string Qry = " Select COUNT(1) from sys.objects where name = @Objectname ";
            var cmd = CreateCommand(Qry);
            AddParameter(cmd, "@ObjectName", ObjectName);

            return await ExecuteScalarAsync<SqlBoolean>(cmd);
        }

        protected async Task<int> ExecuteMultiQueryAsync(string[] Queries)
        {
            if (Queries == null || Queries.Length == 0)
                throw new ArgumentException("Queries array cannot be null or empty.", nameof(Queries));

            int totalAffectedRows = 0;

            _dbConnection.BeginTransaction();

            try
            {
                foreach (var Query in Queries)
                {
                    var cmd = CreateCommand(Query);
                    int affected = await cmd.ExecuteNonQueryAsync();
                    totalAffectedRows += affected;
                }

                _dbConnection.CommitTransaction();
            }
            catch
            {
                _dbConnection.RollbackTransaction();
                throw;
            }

            return totalAffectedRows;
        }
    }
}
