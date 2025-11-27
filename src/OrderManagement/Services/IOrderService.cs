using OrderManagement.DTOs;
using OrderManagement.Models;

namespace OrderManagement.Services;

public interface IOrderService
{
    Task<IEnumerable<OrderDto>> GetAllOrdersAsync();
    Task<IEnumerable<OrderDto>> GetUserOrdersAsync(string userId);
    Task<OrderDto?> GetOrderByIdAsync(int id);
    Task<OrderDto?> CreateOrderAsync(string userId, OrderCreateDto orderDto);
    Task<bool> UpdateOrderStatusAsync(int id, OrderStatus status);
    Task<bool> DeleteOrderAsync(int id);
}
