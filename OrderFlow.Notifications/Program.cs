using OrderFlow.Notifications;
using OrderFlow.Notifications.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

// Add Redis for subscribing to events
builder.AddRedisClient("cache");

// Register services
builder.Services.AddScoped<IEmailService, EmailService>();

// Add the worker
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
