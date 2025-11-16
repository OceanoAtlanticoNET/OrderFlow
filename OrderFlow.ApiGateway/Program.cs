using OrderFlow.ApiGateway.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Aspire service defaults (telemetry, health checks, service discovery)
builder.AddServiceDefaults();

// Redis for distributed rate limiting
builder.AddRedisClient("cache");

// CORS for frontend communication
builder.Services.AddGatewayCors();

// JWT authentication (validates tokens from Identity service)
builder.Services.AddJwtAuthentication(builder.Configuration);

// Authorization policies (authenticated, admin, customer)
builder.Services.AddGatewayAuthorizationPolicies();

// Rate limiting with Redis (100 req/min auth, 20 req/min anon)
builder.Services.AddRedisRateLimiting();

// YARP reverse proxy (routes to microservices)
builder.Services.AddYarpReverseProxy(builder.Configuration);

var app = builder.Build();

// Health check endpoints
app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapReverseProxy();

app.Run();
