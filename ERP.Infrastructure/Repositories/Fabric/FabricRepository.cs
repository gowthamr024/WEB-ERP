using ERP.Infrastructure.Database;
using ERP.Infrastructure.Helpers;
using ERP.Infrastructure.Models.DTOs;
using ERP.Infrastructure.Models.Entities;
using ERP.Infrastructure.Services;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace ERP.Infrastructure.Repositories.Fabric
{
    public class FabricRepository : BaseRepository, IFabricRepository
    {
        private readonly IErrorLogger _errorLogger;
        private readonly PermissionCache _cache;

        public FabricRepository(DbConnection dbConnection, IErrorLogger errorLogger, PermissionCache cache)
    : base(dbConnection)
        {
            _errorLogger = errorLogger ?? throw new ArgumentNullException(nameof(errorLogger));
            _cache = cache;
        }

        public async Task<int?> CreateOrder(Order order)
        {
            string query = @"INSERT INTO Orders (CustomerName, OrderDate, TotalAmount, Status)
                                 OUTPUT INSERTED.OrderID
                                 VALUES (@CustomerName, @OrderDate, @TotalAmount, @Status)";
            var cmd = CreateCommand(query, CommandType.Text);

            AddParameter(cmd, "@OrderDate", order.OrderDate);
            AddParameter(cmd, "@TotalAmount", order.TotalAmount);
            AddParameter(cmd, "@Status", order.Status);
            try
            {
                return await ExecuteSingleQueryAsync(cmd);
            }
            catch (Exception ex)
            {
                _errorLogger.Log(ex, Title: "DB operation failed");
                return null;
            }

        }

        public async Task<Order?> GetOrderDetailsById(int orderId)
        {

            string query = @"SELECT OrderID, CustomerName, OrderDate, TotalAmount, Status
                                 FROM Orders WHERE OrderID = @OrderID";

            var cmd = CreateCommand(query, CommandType.Text);
            AddParameter(cmd, "@OrderID", orderId);

            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    return new Order
                    {
                        OrderID = reader.GetInt32(0),
                        //CustomerName = reader.GetString(1),
                        OrderDate = reader.GetDateTime(2),
                        TotalAmount = reader.GetDecimal(3),
                        Status = reader.GetString(4)
                    };
                }
            }
            return null;
        }



        public async Task<List<Order>> GetAllOrders()
        {
            var orders = new List<Order>();

            string query = @"SELECT OrderID, CustomerName, OrderDate, TotalAmount, Status FROM Orders";
            var cmd = CreateCommand(query, CommandType.Text);

            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    orders.Add(new Order
                    {
                        OrderID = reader.GetInt32(0),
                        //CustomerName = reader.GetString(1),
                        OrderDate = reader.GetDateTime(2),
                        TotalAmount = reader.GetDecimal(3),
                        Status = reader.GetString(4)
                    });
                }
            }
            return orders;
        }

        public async Task<bool> UpdateOrder(Order order)
        {
            string query = @"UPDATE Orders 
                                 SET CustomerName = @CustomerName, 
                                     OrderDate = @OrderDate, 
                                     TotalAmount = @TotalAmount, 
                                     Status = @Status
                                 WHERE OrderID = @OrderID";

            var cmd = CreateCommand(query, CommandType.Text);
            //AddParameter(cmd, "@CustomerName", order.CustomerName);
            AddParameter(cmd, "@OrderDate", order.OrderDate);
            AddParameter(cmd, "@TotalAmount", order.TotalAmount);
            AddParameter(cmd, "@Status", order.Status);
            AddParameter(cmd, "@OrderID", order.OrderID);

            return await ExecuteSingleQueryAsync(cmd) > 0;
        }

        public async Task<bool> DeleteOrder(int orderId)
        {
            string query = @"DELETE FROM Orders WHERE OrderID = @OrderID";

            var cmd = CreateCommand(query, CommandType.Text);
            AddParameter(cmd, "@OrderID", orderId);

            return await ExecuteSingleQueryAsync(cmd) > 0;
        }
    }
}
