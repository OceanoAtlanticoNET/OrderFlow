using OrderFlow.Shared.Common;

namespace OrderFlow.Catalog.Services;

public interface IStockService
{
    Task<ServiceResult> ReserveStockAsync(int productId, int quantity);
    Task<ServiceResult> ReleaseStockAsync(int productId, int quantity);
}
