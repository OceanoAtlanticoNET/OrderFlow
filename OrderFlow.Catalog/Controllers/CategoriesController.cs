using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Catalog.Data;
using OrderFlow.Catalog.DTOs;
using OrderFlow.Catalog.Entities;

namespace OrderFlow.Catalog.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class CategoriesController(CatalogDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryResponse>>> GetAll()
    {
        var categories = await db.Categories
            .Select(c => new CategoryResponse(c.Id, c.Name, c.Description, c.Products.Count))
            .ToListAsync();

        return Ok(categories);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoryResponse>> GetById(int id)
    {
        var category = await db.Categories
            .Where(c => c.Id == id)
            .Select(c => new CategoryResponse(c.Id, c.Name, c.Description, c.Products.Count))
            .FirstOrDefaultAsync();

        if (category is null)
            return NotFound();

        return Ok(category);
    }

    [HttpPost]
    public async Task<ActionResult<CategoryResponse>> Create(CreateCategoryRequest request)
    {
        var category = new Category
        {
            Name = request.Name,
            Description = request.Description
        };

        db.Categories.Add(category);
        await db.SaveChangesAsync();

        var response = new CategoryResponse(category.Id, category.Name, category.Description, 0);
        return CreatedAtAction(nameof(GetById), new { id = category.Id }, response);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<CategoryResponse>> Update(int id, UpdateCategoryRequest request)
    {
        var category = await db.Categories.FindAsync(id);
        if (category is null)
            return NotFound();

        category.Name = request.Name;
        category.Description = request.Description;

        await db.SaveChangesAsync();

        var productCount = await db.Products.CountAsync(p => p.CategoryId == id);
        return Ok(new CategoryResponse(category.Id, category.Name, category.Description, productCount));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await db.Categories
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category is null)
            return NotFound();

        if (category.Products.Count > 0)
            return BadRequest("Cannot delete category with products");

        db.Categories.Remove(category);
        await db.SaveChangesAsync();

        return NoContent();
    }
}
