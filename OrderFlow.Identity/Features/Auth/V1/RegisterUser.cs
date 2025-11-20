using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using OrderFlow.Identity.Services.Auth;

namespace OrderFlow.Identity.Features.Auth.V1;

public static class RegisterUser
{
    public sealed record RegisterUserRequest(
        string Email,
        string Password
    );

    public sealed record RegisterUserResponse(
        string UserId,
        string Email,
        string Message
    );

    public class Validator : AbstractValidator<RegisterUserRequest>
    {
        public Validator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Email format is invalid");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters long")
                .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter")
                .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter")
                .Matches(@"\d").WithMessage("Password must contain at least one digit")
                .Matches(@"[^\da-zA-Z]").WithMessage("Password must contain at least one special character");
        }
    }

    public static IEndpointRouteBuilder MapRegisterUser(this IEndpointRouteBuilder endpoints)
    {
        var authGroup = endpoints.MapAuthGroup();

        authGroup.MapPost("/register", HandleAsync)
            .WithName("RegisterUserV1")
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                operation.Summary = "Register a new user";
                operation.Description = "Creates a new user account with the provided credentials";
                return Task.CompletedTask;
            })
            .Produces<RegisterUserResponse>(StatusCodes.Status201Created)
            .Produces<ValidationProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .DisableAntiforgery()
            .AllowAnonymous();

        return endpoints;
    }

    private static async Task<IResult> HandleAsync(
        RegisterUserRequest request,
        IAuthService authService,
        IValidator<RegisterUserRequest> validator,
        ILogger<RegisterUserRequest> logger,
        CancellationToken cancellationToken = default)
    {

        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.ToDictionary();
            logger.LogWarning("Registration validation failed for email: {Email}", request.Email);
            return Results.ValidationProblem(errors, title: "Validation failed");
        }

        // Call authentication service
        var result = await authService.RegisterAsync(
            request.Email,
            request.Password);

        if (!result.Succeeded)
        {
            logger.LogWarning("Registration failed for email: {Email}", request.Email);
            return Results.Problem(
                title: "Registration failed",
                detail: string.Join(", ", result.Errors),
                statusCode: StatusCodes.Status400BadRequest);
        }

        logger.LogInformation("User successfully registered: {UserId} - {Email}",
            result.Data!.UserId, result.Data.Email);

        return Results.Created($"/api/auth/users/{result.Data.UserId}", result.Data);
    }
}