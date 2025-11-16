using FluentValidation;
using OrderFlow.Identity.Services.Auth;

namespace OrderFlow.Identity.Features.Auth.V1;

public static class RegisterUser
{
    public sealed record RegisterUserRequest(
        string Email,
        string Password,
        string FirstName,
        string LastName
    );

    public sealed record RegisterUserResponse(
        string UserId,
        string Email,
        string Message
    );

    public sealed record AuthErrorResponse(
        IEnumerable<string> Errors
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

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required");
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
            .Produces<AuthErrorResponse>(StatusCodes.Status400BadRequest)
            .DisableAntiforgery()
            .AllowAnonymous();

        return endpoints;
    }

    private static async Task<IResult> HandleAsync(
        RegisterUserRequest? request,
        IAuthService authService,
        IValidator<RegisterUserRequest> validator,
        ILogger<RegisterUserRequest> logger,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            return Results.BadRequest(new AuthErrorResponse(["Request body is required"]));
        }

        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage);
            logger.LogWarning("Registration validation failed for email: {Email}", request.Email);
            return Results.BadRequest(new AuthErrorResponse(errors));
        }

        // Call authentication service
        var result = await authService.RegisterAsync(
            request.Email,
            request.Password,
            request.FirstName,
            request.LastName);

        if (!result.Succeeded)
        {
            logger.LogWarning("Registration failed for email: {Email}", request.Email);
            return Results.BadRequest(new AuthErrorResponse(result.Errors));
        }

        logger.LogInformation("User successfully registered: {UserId} - {Email}",
            result.Data!.UserId, result.Data.Email);

        return Results.Created($"/api/auth/users/{result.Data.UserId}", result.Data);
    }
}