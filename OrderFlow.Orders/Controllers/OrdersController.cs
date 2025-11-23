using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Orders.Data;
using OrderFlow.Orders.DTOs;
using OrderFlow.Orders.Entities;

namespace OrderFlow.Orders.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class OrdersController(OrdersDbContext db, IHttpClientFactory httpClientFactory) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderListResponse>>> GetUserOrders()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var orders = await db.Orders
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new OrderListResponse(
                o.Id, o.Status.ToString(), o.TotalAmount, o.Items.Count, o.CreatedAt))
            .ToListAsync();

        return Ok(orders);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrderResponse>> GetById(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");

        var order = await db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order is null)
            return NotFound();

        // Users can only see their own orders (admin check would be added via authorization)
        if (order.UserId != userId)
            return Forbid();

        return Ok(MapToResponse(order));
    }

    [HttpPost]
    public async Task<ActionResult<OrderResponse>> Create(CreateOrderRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        if (!request.Items.Any())
            return BadRequest("Order must have at least one item");

        // Fetch product details from Catalog service
        var catalogClient = httpClientFactory.CreateClient("catalog");
        var orderItems = new List<OrderItem>();

        foreach (var item in request.Items)
        {
            try
            {
                var response = await catalogClient.GetAsync($"/api/v1/products/{item.ProductId}");
                if (!response.IsSuccessStatusCode)
                    return BadRequest($"Product {item.ProductId} not found");

                var product = await response.Content.ReadFromJsonAsync<ProductInfo>();
                if (product is null)
                    return BadRequest($"Could not fetch product {item.ProductId}");

                if (!product.IsActive)
                    return BadRequest($"Product {product.Name} is not available");

                if (product.Stock < item.Quantity)
                    return BadRequest($"Insufficient stock for {product.Name}");

                orderItems.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    ProductName = product.Name,
                    UnitPrice = product.Price,
                    Quantity = item.Quantity
                });
            }
            catch (HttpRequestException)
            {
                return StatusCode(503, "Catalog service unavailable");
            }
        }

        var order = new Order
        {
            UserId = userId,
            ShippingAddress = request.ShippingAddress,
            Notes = request.Notes,
            Items = orderItems,
            TotalAmount = orderItems.Sum(i => i.UnitPrice * i.Quantity)
        };

        db.Orders.Add(order);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = order.Id }, MapToResponse(order));
    }

    [HttpPut("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");

        var order = await db.Orders.FindAsync(id);
        if (order is null)
            return NotFound();

        if (order.UserId != userId)
            return Forbid();

        if (order.Status is not (OrderStatus.Pending or OrderStatus.Confirmed))
            return BadRequest("Order cannot be cancelled at this stage");

        order.Status = OrderStatus.Cancelled;
        order.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        return NoContent();
    }

    private static OrderResponse MapToResponse(Order order) => new(
        order.Id,
        order.UserId,
        order.Status.ToString(),
        order.TotalAmount,
        order.ShippingAddress,
        order.Notes,
        order.CreatedAt,
        order.UpdatedAt,
        order.Items.Select(i => new OrderItemResponse(
            i.Id, i.ProductId, i.ProductName, i.UnitPrice, i.Quantity, i.Subtotal)));

    private record ProductInfo(int Id, string Name, decimal Price, int Stock, bool IsActive);
}
