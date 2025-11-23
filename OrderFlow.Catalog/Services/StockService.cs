using Microsoft.EntityFrameworkCore;
using OrderFlow.Catalog.Data;
using OrderFlow.Shared.Common;

namespace OrderFlow.Catalog.Services;

public class StockService(CatalogDbContext db, ILogger<StockService> logger) : IStockService
{
    public async Task<ServiceResult> ReserveStockAsync(int productId, int quantity)
    {
        // Atomic operation: check and update in single query
        // Works safely with multiple instances
        var rowsAffected = await db.Products
            .Where(p => p.Id == productId && p.Stock >= quantity)
            .ExecuteUpdateAsync(p => p
                .SetProperty(x => x.Stock, x => x.Stock - quantity)
                .SetProperty(x => x.UpdatedAt, DateTime.UtcNow));

        if (rowsAffected == 0)
        {
            // Check if product exists to provide better error message
            var exists = await db.Products.AnyAsync(p => p.Id == productId);
            if (!exists)
            {
                logger.LogWarning("Product not found for stock reservation: {ProductId}", productId);
                return ServiceResult.Failure($"Product {productId} not found");
            }

            logger.LogWarning(
                "Insufficient stock for product {ProductId}: requested {Quantity}",
                productId, quantity);
            return ServiceResult.Failure($"Insufficient stock for product {productId}");
        }

        logger.LogInformation(
            "Stock reserved for product {ProductId}: -{Quantity}",
            productId, quantity);

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> ReleaseStockAsync(int productId, int quantity)
    {
        // Atomic operation: update in single query
        var rowsAffected = await db.Products
            .Where(p => p.Id == productId)
            .ExecuteUpdateAsync(p => p
                .SetProperty(x => x.Stock, x => x.Stock + quantity)
                .SetProperty(x => x.UpdatedAt, DateTime.UtcNow));

        if (rowsAffected == 0)
        {
            logger.LogWarning("Product not found for stock release: {ProductId}", productId);
            return ServiceResult.Failure($"Product {productId} not found");
        }

        logger.LogInformation(
            "Stock released for product {ProductId}: +{Quantity}",
            productId, quantity);

        return ServiceResult.Success();
    }
}
