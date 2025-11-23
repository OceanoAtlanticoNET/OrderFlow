using OrderFlow.Catalog.DTOs;
using OrderFlow.Shared.Common;

namespace OrderFlow.Catalog.Services;

public interface IProductService
{
    Task<ServiceResult<IEnumerable<ProductListResponse>>> GetAllAsync(int? categoryId, bool? isActive, string? search);
    Task<ServiceResult<ProductResponse>> GetByIdAsync(int id);
    Task<ServiceResult<ProductResponse>> CreateAsync(CreateProductRequest request);
    Task<ServiceResult<ProductResponse>> UpdateAsync(int id, UpdateProductRequest request);
    Task<ServiceResult> UpdateStockAsync(int id, int quantity);
    Task<ServiceResult> DeleteAsync(int id);
}
