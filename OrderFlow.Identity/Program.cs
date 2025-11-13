using Scalar.AspNetCore;
using OrderFlow.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Identity.Features.Auth.V1;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Asp.Versioning;

var builder = WebApplication.CreateBuilder(args);

// Add Aspire Service Defaults (OpenTelemetry, Health Checks, Service Discovery, Resilience)
builder.AddServiceDefaults();

// Configure multiple OpenAPI documents for different versions
builder.Services.AddOpenApi("v1");
builder.Services.AddOpenApi("v2");

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
// JWT BEARER AUTHENTICATION
// ============================================
var jwtSecret = builder.Configuration["Jwt:Secret"] 
    ?? throw new InvalidOperationException("JWT Secret is not configured");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] 
    ?? throw new InvalidOperationException("JWT Issuer is not configured");
var jwtAudience = builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException("JWT Audience is not configured");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ClockSkew = TimeSpan.Zero // Remove default 5 minute tolerance
    };
});

var app = builder.Build();

// ============================================
// AUTO-MIGRATE DATABASE ON STARTUP
// ============================================
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await context.Database.MigrateAsync();
    
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    
    foreach (var role in Roles.GetAll())
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
    
    // Map OpenAPI documents - uses document names from AddOpenApi configuration
    app.MapOpenApi();

    // Scalar Documentation with version switching
    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("OrderFlow Identity API")
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
            .AddDocument("v1", "V1 - Minimal API", "/openapi/v1.json", isDefault: true)
            .AddDocument("v2", "V2 - Controllers", "/openapi/v2.json");
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
app.MapRegisterUser();
app.MapLoginUser();
app.MapGetCurrentUser();
app.MapAdminOnly();

await app.RunAsync();
