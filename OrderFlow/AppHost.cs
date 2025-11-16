var builder = DistributedApplication.CreateBuilder(args);

// ============================================
// INFRASTRUCTURE
// ============================================

// PostgreSQL - Database for microservices
var postgres = builder.AddPostgres("postgres")
    .WithDataVolume("orderflow-postgres-data")
    .WithHostPort(5432)
    .WithLifetime(ContainerLifetime.Persistent);

// Identity database
var identityDb = postgres.AddDatabase("identitydb");

// Redis - Distributed cache for rate limiting, sessions, and caching
var redis = builder.AddRedis("cache")
    .WithDataVolume("orderflow-redis-data")
    .WithHostPort(6379)
    .WithLifetime(ContainerLifetime.Persistent);

// ============================================
// MICROSERVICES
// ============================================
var identityService = builder.AddProject<Projects.OrderFlow_Identity>("orderflow-identity")
    .WithReference(identityDb)
    .WaitFor(identityDb); // Wait for database to be ready before starting service

// ============================================
// API GATEWAY
// ============================================
// API Gateway acts as the single entry point for all client requests
// It handles authentication, authorization, rate limiting, and routes to microservices
var apiGateway = builder.AddProject<Projects.OrderFlow_ApiGateway>("orderflow-apigateway")
    .WithReference(redis) // Redis for rate limiting and caching
    .WithReference(identityService) // Gateway needs to route to Identity Service
    .WaitFor(identityService); // Wait for Identity Service to be ready

// ============================================
// FRONTEND - React App
// ============================================
// Frontend communicates ONLY with API Gateway (not directly with microservices)
var frontendApp = builder.AddNpmApp("orderflow-web", "../orderflow.web", "dev")
    .WithReference(apiGateway) // Frontend talks to Gateway, not to services directly
    .WithEnvironment("VITE_API_GATEWAY_URL", apiGateway.GetEndpoint("https")) // Gateway URL for frontend
    .WithHttpEndpoint(env: "VITE_PORT") // Vite uses VITE_PORT environment variable
    .WithExternalHttpEndpoints() // Make endpoint accessible via Aspire dashboard
    .PublishAsDockerFile();

builder.Build().Run();
