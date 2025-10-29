using ERP.Infrastructure.Models.DTOs;

namespace ERP.Infrastructure.Repositories.Fabric
{
    public interface IFabricRepository
    {
        Task<int?> CreateOrder(Order order);
        Task<Order?> GetOrderDetailsById(int orderId);
        Task<List<Order>> GetAllOrders();
        Task<bool> UpdateOrder(Order order);
        Task<bool> DeleteOrder(int orderId);
    }
}
