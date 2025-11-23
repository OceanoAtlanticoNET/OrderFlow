using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using OrderFlow.Orders.DTOs;
using OrderFlow.Orders.Services;

namespace OrderFlow.Orders.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class OrdersController(IOrderService orderService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderListResponse>>> GetUserOrders()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await orderService.GetUserOrdersAsync(userId);
        return Ok(result.Data);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrderResponse>> GetById(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await orderService.GetByIdAsync(id, userId);

        if (!result.Succeeded)
        {
            if (result.Errors.Any(e => e.Contains("Access denied")))
                return Forbid();

            return NotFound();
        }

        return Ok(result.Data);
    }

    [HttpPost]
    public async Task<ActionResult<OrderResponse>> Create(CreateOrderRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await orderService.CreateAsync(userId, request);

        if (!result.Succeeded)
        {
            if (result.Errors.Any(e => e.Contains("service unavailable")))
                return StatusCode(503, string.Join(", ", result.Errors));

            return BadRequest(string.Join(", ", result.Errors));
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
    }

    [HttpPut("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await orderService.CancelAsync(id, userId);

        if (!result.Succeeded)
        {
            if (result.Errors.Any(e => e.Contains("Access denied")))
                return Forbid();

            if (result.Errors.Any(e => e.Contains("not found")))
                return NotFound();

            return BadRequest(string.Join(", ", result.Errors));
        }

        return NoContent();
    }
}
