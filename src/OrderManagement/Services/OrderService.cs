using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using OrderManagement.Data;
using OrderManagement.DTOs;
using OrderManagement.Models;

namespace OrderManagement.Services;

public class OrderService : IOrderService
{
    private readonly ApplicationDbContext _context;
    private readonly IMemoryCache _cache;
    private const string UserOrdersCacheKeyPrefix = "UserOrders_";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(2);

    public OrderService(ApplicationDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
    {
        return await _context.Orders
            .AsNoTracking()
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .OrderByDescending(o => o.OrderDate)
            .Select(o => MapToDto(o))
            .ToListAsync();
    }

    public async Task<IEnumerable<OrderDto>> GetUserOrdersAsync(string userId)
    {
        var cacheKey = $"{UserOrdersCacheKeyPrefix}{userId}";

        if (_cache.TryGetValue(cacheKey, out IEnumerable<OrderDto>? cachedOrders) && cachedOrders != null)
        {
            return cachedOrders;
        }

        var orders = await _context.Orders
            .AsNoTracking()
            .Where(o => o.UserId == userId)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .OrderByDescending(o => o.OrderDate)
            .Select(o => MapToDto(o))
            .ToListAsync();

        _cache.Set(cacheKey, orders, CacheDuration);
        return orders;
    }

    public async Task<OrderDto?> GetOrderByIdAsync(int id)
    {
        var order = await _context.Orders
            .AsNoTracking()
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        return order == null ? null : MapToDto(order);
    }

    public async Task<OrderDto?> CreateOrderAsync(string userId, OrderCreateDto orderDto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var productIds = orderDto.Items.Select(i => i.ProductId).ToList();
            var products = await _context.Products
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync();

            if (products.Count != productIds.Count)
            {
                return null;
            }

            foreach (var item in orderDto.Items)
            {
                var product = products.First(p => p.Id == item.ProductId);
                if (product.StockQuantity < item.Quantity)
                {
                    return null;
                }
            }

            var order = new Order
            {
                UserId = userId,
                ShippingAddress = orderDto.ShippingAddress,
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Pending
            };

            decimal totalAmount = 0;

            foreach (var item in orderDto.Items)
            {
                var product = products.First(p => p.Id == item.ProductId);

                var orderItem = new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = product.Price
                };

                order.OrderItems.Add(orderItem);
                totalAmount += orderItem.Quantity * orderItem.UnitPrice;

                product.StockQuantity -= item.Quantity;
            }

            order.TotalAmount = totalAmount;

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            InvalidateUserOrderCache(userId);

            return await GetOrderByIdAsync(order.Id);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> UpdateOrderStatusAsync(int id, OrderStatus status)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null)
        {
            return false;
        }

        order.Status = status;
        await _context.SaveChangesAsync();

        InvalidateUserOrderCache(order.UserId);

        return true;
    }

    public async Task<bool> DeleteOrderAsync(int id)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
        {
            return false;
        }

        if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Cancelled)
        {
            return false;
        }

        foreach (var item in order.OrderItems)
        {
            var product = await _context.Products.FindAsync(item.ProductId);
            if (product != null)
            {
                product.StockQuantity += item.Quantity;
            }
        }

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();

        InvalidateUserOrderCache(order.UserId);

        return true;
    }

    private void InvalidateUserOrderCache(string userId)
    {
        _cache.Remove($"{UserOrdersCacheKeyPrefix}{userId}");
    }

    private static OrderDto MapToDto(Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            UserId = order.UserId,
            OrderDate = order.OrderDate,
            Status = order.Status,
            ShippingAddress = order.ShippingAddress,
            TotalAmount = order.TotalAmount,
            Items = order.OrderItems.Select(oi => new OrderItemDto
            {
                Id = oi.Id,
                ProductId = oi.ProductId,
                ProductName = oi.Product?.Name ?? string.Empty,
                Quantity = oi.Quantity,
                UnitPrice = oi.UnitPrice,
                TotalPrice = oi.Quantity * oi.UnitPrice
            }).ToList()
        };
    }
}
