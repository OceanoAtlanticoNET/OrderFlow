var builder = DistributedApplication.CreateBuilder(args);

// ============================================
// INFRASTRUCTURE - PostgreSQL
// ============================================
var postgres = builder.AddPostgres("postgres")
    .WithLifetime(ContainerLifetime.Persistent);

// Identity database
var identityDb = postgres.AddDatabase("identitydb");

// ============================================
// MICROSERVICES
// ============================================
var identityService = builder.AddProject<Projects.OrderFlow_Identity>("orderflow-identity")
    .WithReference(identityDb);

// ============================================
// FRONTEND - React App
// ============================================
var frontendApp = builder.AddNpmApp("orderflow-web", "../orderflow.web", "dev")
    .WithReference(identityService)
    .WithHttpEndpoint(port: 5173, env: "PORT")
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();

builder.Build().Run();
