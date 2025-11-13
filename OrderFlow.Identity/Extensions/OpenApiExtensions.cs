using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace OrderFlow.Identity.Extensions;

public static class OpenApiExtensions
{
    /// <summary>
    /// Adds JWT Bearer authentication configuration to OpenAPI documents
    /// </summary>
    public static void AddJwtBearerSecurity(this OpenApiOptions options)
    {
        // Add JWT Bearer security scheme
        options.AddDocumentTransformer((document, context, cancellationToken) =>
        {
            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Enter your JWT token in the format: your-token-here"
            };
            return Task.CompletedTask;
        });

        // Automatically add security requirements to protected endpoints
        options.AddOperationTransformer((operation, context, cancellationToken) =>
        {
            var metadata = context.Description.ActionDescriptor.EndpointMetadata;
            var hasAuthorize = metadata.OfType<Microsoft.AspNetCore.Authorization.IAuthorizeData>().Any();
            var hasAllowAnonymous = metadata.OfType<Microsoft.AspNetCore.Authorization.IAllowAnonymous>().Any();

            if (hasAuthorize && !hasAllowAnonymous)
            {
                operation.Security ??= new List<OpenApiSecurityRequirement>();
                operation.Security.Add(new OpenApiSecurityRequirement
                {
                    [new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    }] = Array.Empty<string>()
                });
            }

            return Task.CompletedTask;
        });
    }

    /// <summary>
    /// Configures OpenAPI document information
    /// </summary>
    public static void ConfigureDocumentInfo(
        this OpenApiOptions options,
        string title,
        string version,
        string description)
    {
        options.AddDocumentTransformer((document, context, cancellationToken) =>
        {
            document.Info.Title = title;
            document.Info.Version = version;
            document.Info.Description = description;
            return Task.CompletedTask;
        });
    }
}
