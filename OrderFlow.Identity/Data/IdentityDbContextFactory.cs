using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OrderFlow.Identity.Data;

/// <summary>
/// Design-time factory for creating DbContext instances for EF Core migrations
/// This is used when the application isn't running
/// </summary>
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        
        // Use a placeholder connection string for migrations
        // The actual connection string will come from Aspire at runtime
        optionsBuilder.UseNpgsql("Host=localhost;Database=identitydb;Username=postgres;Password=postgres");
        
        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
