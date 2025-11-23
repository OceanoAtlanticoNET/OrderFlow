using Microsoft.AspNetCore.Mvc;
using OrderFlow.Orders.DTOs;
using OrderFlow.Orders.Entities;
using OrderFlow.Orders.Services;

namespace OrderFlow.Orders.Controllers;

[ApiController]
[Route("api/v1/admin/orders")]
public class AdminOrdersController(IOrderService orderService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderListResponse>>> GetAll(
        [FromQuery] OrderStatus? status = null,
        [FromQuery] string? userId = null)
    {
        var result = await orderService.GetAllAsync(status, userId);
        return Ok(result.Data);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrderResponse>> GetById(int id)
    {
        var result = await orderService.GetByIdForAdminAsync(id);

        if (!result.Succeeded)
            return NotFound();

        return Ok(result.Data);
    }

    [HttpPut("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, UpdateOrderStatusRequest request)
    {
        var result = await orderService.UpdateStatusAsync(id, request.Status);

        if (!result.Succeeded)
        {
            if (result.Errors.Any(e => e.Contains("not found")))
                return NotFound();

            return BadRequest(string.Join(", ", result.Errors));
        }

        return NoContent();
    }
}
