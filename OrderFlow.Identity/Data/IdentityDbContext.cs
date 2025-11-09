using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace OrderFlow.Identity.Data;

/// <summary>
/// Database context for ASP.NET Core Identity
/// Inherits from IdentityDbContext to get all Identity tables (Users, Roles, Claims, etc.)
/// </summary>
public class ApplicationDbContext : IdentityDbContext<IdentityUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // You can customize the ASP.NET Identity model and override the defaults if needed
        // For example, you can rename tables:
        // builder.Entity<IdentityUser>().ToTable("Users");
        // builder.Entity<IdentityRole>().ToTable("Roles");
    }
}
