using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OrderFlow.Identity.Extensions;

public static class OpenApiExtensions
{
    /// <summary>
    /// Adds JWT Bearer authentication configuration to OpenAPI documents
    /// </summary>
    public static void AddJwtBearerSecurity(this OpenApiOptions options)
    {
        // Add JWT Bearer security scheme
        options.AddDocumentTransformer(new JwtBearerSecuritySchemeDocumentTransformer());

        // Automatically add security requirements to protected endpoints
        options.AddOperationTransformer(new JwtBearerSecurityRequirementOperationTransformer());
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
        options.AddDocumentTransformer(new DocumentInfoTransformer(title, version, description));
    }
}

internal sealed class JwtBearerSecuritySchemeDocumentTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        // Ensure components and its dictionaries are initialized in v2
        document.RegisterComponents();

        var scheme = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Enter your JWT token in the format: your-token-here"
        };

        // Add or replace the Bearer security scheme via helper that initializes dictionaries
        document.AddComponent("Bearer", scheme);

        return Task.CompletedTask;
    }
}

internal sealed class JwtBearerSecurityRequirementOperationTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        var metadata = context.Description.ActionDescriptor.EndpointMetadata;
        var hasAuthorize = metadata.OfType<IAuthorizeData>().Any();
        var hasAllowAnonymous = metadata.OfType<IAllowAnonymous>().Any();

        if (hasAuthorize && !hasAllowAnonymous)
        {
            operation.Security ??= new List<OpenApiSecurityRequirement>();

            // Reference the previously added Bearer scheme by name in this document
            var bearerRef = new OpenApiSecuritySchemeReference("Bearer", context.Document, null);

            operation.Security.Add(new OpenApiSecurityRequirement
            {
                [bearerRef] = new List<string>()
            });
        }

        return Task.CompletedTask;
    }
}

internal sealed class DocumentInfoTransformer : IOpenApiDocumentTransformer
{
    private readonly string _title;
    private readonly string _version;
    private readonly string _description;

    public DocumentInfoTransformer(string title, string version, string description)
    {
        _title = title;
        _version = version;
        _description = description;
    }

    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        // Ensure Info exists in v2
        document.Info ??= new OpenApiInfo();

        document.Info.Title = _title;
        document.Info.Version = _version;
        document.Info.Description = _description;
        return Task.CompletedTask;
    }
}
