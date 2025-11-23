using OrderFlow.Orders.DTOs;
using OrderFlow.Orders.Entities;
using OrderFlow.Shared.Common;

namespace OrderFlow.Orders.Services;

public interface IOrderService
{
    // User operations
    Task<ServiceResult<IEnumerable<OrderListResponse>>> GetUserOrdersAsync(string userId);
    Task<ServiceResult<OrderResponse>> GetByIdAsync(int id, string userId);
    Task<ServiceResult<OrderResponse>> CreateAsync(string userId, CreateOrderRequest request);
    Task<ServiceResult> CancelAsync(int id, string userId);

    // Admin operations
    Task<ServiceResult<IEnumerable<OrderListResponse>>> GetAllAsync(OrderStatus? status, string? userId);
    Task<ServiceResult<OrderResponse>> GetByIdForAdminAsync(int id);
    Task<ServiceResult> UpdateStatusAsync(int id, OrderStatus newStatus);
}
