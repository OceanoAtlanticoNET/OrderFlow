using Microsoft.AspNetCore.Mvc;
using OrderFlow.Catalog.DTOs;
using OrderFlow.Catalog.Services;

namespace OrderFlow.Catalog.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class CategoriesController(ICategoryService categoryService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryResponse>>> GetAll()
    {
        var result = await categoryService.GetAllAsync();
        return Ok(result.Data);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoryResponse>> GetById(int id)
    {
        var result = await categoryService.GetByIdAsync(id);

        if (!result.Succeeded)
            return NotFound();

        return Ok(result.Data);
    }

    [HttpPost]
    public async Task<ActionResult<CategoryResponse>> Create(CreateCategoryRequest request)
    {
        var result = await categoryService.CreateAsync(request);

        if (!result.Succeeded)
            return BadRequest(string.Join(", ", result.Errors));

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<CategoryResponse>> Update(int id, UpdateCategoryRequest request)
    {
        var result = await categoryService.UpdateAsync(id, request);

        if (!result.Succeeded)
            return NotFound();

        return Ok(result.Data);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await categoryService.DeleteAsync(id);

        if (!result.Succeeded)
        {
            if (result.Errors.Any(e => e.Contains("not found")))
                return NotFound();

            return BadRequest(string.Join(", ", result.Errors));
        }

        return NoContent();
    }
}
