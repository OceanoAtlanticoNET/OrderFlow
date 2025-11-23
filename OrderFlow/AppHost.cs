var builder = DistributedApplication.CreateBuilder(args);

// ============================================
// INFRASTRUCTURE
// ============================================

// PostgreSQL - Database for microservices
var postgres = builder.AddPostgres("postgres")
    .WithDataVolume("orderflow-postgres-data")
    .WithPgAdmin()
    .WithHostPort(5432)
    .WithLifetime(ContainerLifetime.Persistent);

// Databases for microservices
var identityDb = postgres.AddDatabase("identitydb");
var catalogDb = postgres.AddDatabase("catalogdb");
var ordersDb = postgres.AddDatabase("ordersdb");

// Redis - Distributed cache for rate limiting, sessions, pub/sub events
var redis = builder.AddRedis("cache")
    .WithDataVolume("orderflow-redis-data")
    .WithHostPort(6379)
    .WithLifetime(ContainerLifetime.Persistent);

// MailDev - Local SMTP server for development (Web UI on 1080, SMTP on 1025)
var maildev = builder.AddContainer("maildev", "maildev/maildev")
    .WithHttpEndpoint(port: 1080, targetPort: 1080, name: "web")
    .WithEndpoint(port: 1025, targetPort: 1025, name: "smtp")
    .WithLifetime(ContainerLifetime.Persistent);

// ============================================
// MICROSERVICES
// ============================================
var identityService = builder.AddProject<Projects.OrderFlow_Identity>("orderflow-identity")
    .WithReference(identityDb)
    .WithReference(redis) // Redis for publishing events
    .WaitFor(identityDb);

// Notifications Worker - Listens to Redis events and sends emails
var notificationsService = builder.AddProject<Projects.OrderFlow_Notifications>("orderflow-notifications")
    .WithReference(redis) // Redis for subscribing to events
    .WithEnvironment("Email__SmtpHost", maildev.GetEndpoint("smtp").Property(EndpointProperty.Host))
    .WithEnvironment("Email__SmtpPort", maildev.GetEndpoint("smtp").Property(EndpointProperty.Port))
    .WaitFor(redis);

// Catalog Service - Products and Categories
var catalogService = builder.AddProject<Projects.OrderFlow_Catalog>("orderflow-catalog")
    .WithReference(catalogDb)
    .WaitFor(catalogDb);

// Orders Service - Order management
var ordersService = builder.AddProject<Projects.OrderFlow_Orders>("orderflow-orders")
    .WithReference(ordersDb)
    .WithReference(catalogService) // For product validation
    .WaitFor(ordersDb);

// ============================================
// API GATEWAY
// ============================================
// API Gateway acts as the single entry point for all client requests
// It handles authentication, authorization, rate limiting, and routes to microservices
var apiGateway = builder.AddProject<Projects.OrderFlow_ApiGateway>("orderflow-apigateway")
    .WithReference(redis) // Redis for rate limiting and caching
    .WithReference(identityService)
    .WithReference(catalogService)
    .WithReference(ordersService)
    .WaitFor(identityService)
    .WaitFor(catalogService)
    .WaitFor(ordersService);

// ============================================
// FRONTEND - React App
// ============================================
// Frontend communicates ONLY with API Gateway (not directly with microservices)
var frontendApp = builder.AddNpmApp("orderflow-web", "../orderflow.web", "dev")
    .WithReference(apiGateway) // Frontend talks to Gateway, not to services directly
    .WithEnvironment("VITE_API_GATEWAY_URL", apiGateway.GetEndpoint("https")) // Gateway URL for frontend
    .WithHttpEndpoint(env: "VITE_PORT") // Vite uses VITE_PORT environment variable
    .WaitFor(apiGateway)
    .WithExternalHttpEndpoints() // Make endpoint accessible via Aspire dashboard
    .PublishAsDockerFile();

builder.Build().Run();
