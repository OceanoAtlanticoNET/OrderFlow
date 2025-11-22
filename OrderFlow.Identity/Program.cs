using Scalar.AspNetCore;
using OrderFlow.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Identity.Features.Auth.V1;
using OrderFlow.Identity.Features.Users.V1;
using OrderFlow.Identity.Features.Roles.V1;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Asp.Versioning;
//
using OrderFlow.Identity.Extensions;
using OrderFlow.Identity.Services.Auth;
using OrderFlow.Identity.Services.Events;
using OrderFlow.Identity.Services.Users;
using OrderFlow.Identity.Services.Roles;

var builder = WebApplication.CreateBuilder(args);

// Add Aspire Service Defaults (OpenTelemetry, Health Checks, Service Discovery, Resilience)
builder.AddServiceDefaults();

// Configure OpenAPI documents for different versions with JWT Bearer authentication
builder.Services.AddOpenApi("v1", options =>
{
    options.ConfigureDocumentInfo(
        "OrderFlow Identity API V1",
        "v1",
        "Authentication API using Minimal APIs with JWT Bearer authentication");
    options.AddJwtBearerSecurity();
    options.FilterByApiVersion("v1");
});

builder.Services.AddOpenApi("v2", options =>
{
    options.ConfigureDocumentInfo(
        "OrderFlow Identity API V2",
        "v2",
        "Authentication API using Controllers with JWT Bearer authentication");
    options.AddJwtBearerSecurity();
    options.FilterByApiVersion("v2");
});

builder.Services.AddAuthorization();
builder.Services.AddControllers();

// Add API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// ============================================
// CORS CONFIGURATION
// ============================================
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Add PostgreSQL DbContext with Aspire
builder.AddNpgsqlDbContext<ApplicationDbContext>("identitydb");

// Add Redis for event publishing
builder.AddRedisClient("cache");

// Add ASP.NET Core Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
    
    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
    
    // User settings
    options.User.RequireUniqueEmail = true;
    
    // Sign in settings
    options.SignIn.RequireConfirmedEmail = false; // Set to true in production
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// ============================================
// SERVICE LAYER
// ============================================
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IEventPublisher, RedisEventPublisher>();

// ============================================
// JWT BEARER AUTHENTICATION
// ============================================
builder.Services.AddJwtAuthentication(builder.Configuration);

var app = builder.Build();

// ============================================
// SEED DEVELOPMENT DATA (Database, Roles, Admin User)
// ============================================
if (app.Environment.IsDevelopment())
{
    await app.Services.SeedDevelopmentDataAsync();

    // Map OpenAPI documents - uses document names from AddOpenApi configuration
    app.MapOpenApi();

    // path: scalar
    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("OrderFlow Identity API")
            .AddDocument("v1", "V1 - Minimal API", "/openapi/v1.json", isDefault: true)
            .AddDocument("v2", "V2 - Controllers", "/openapi/v2.json");
    });

    //path: swagger

    app.UseSwaggerUI(options =>
    {
        
        options.SwaggerEndpoint("/openapi/v1.json", "OrderFlow Identity API V1");
        options.SwaggerEndpoint("/openapi/v2.json", "OrderFlow Identity API V2");
    });

}

app.UseHttpsRedirection();

app.UseCors(); // Must be before Authentication and Authorization

app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultEndpoints(); // Add health check endpoints

// Map Controllers (V2)
app.MapControllers();

// Map Minimal API endpoints (V1)
// Auth endpoints
app.MapRegisterUser();
app.MapLoginUser();
app.MapGetCurrentUser();
app.MapAdminOnly();

// User management endpoints
app.MapUserEndpoints();

// Role management endpoints
app.MapRoleEndpoints();

await app.RunAsync();
