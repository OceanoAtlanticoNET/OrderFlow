using FluentValidation;
using OrderFlow.Identity.Services.Auth;

namespace OrderFlow.Identity.Features.Auth.V1;

public static class LoginUser
{
    public sealed record LoginUserRequest(
        string Email,
        string Password
    );

    public sealed record LoginUserResponse(
        string AccessToken,
        string TokenType,
        int ExpiresIn,
        string UserId,
        string Email,
        IEnumerable<string> Roles
    );

    public sealed record AuthErrorResponse(
        IEnumerable<string> Errors
    );

    public class Validator : AbstractValidator<LoginUserRequest>
    {
        public Validator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Email format is invalid");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required");
        }
    }

    public static IEndpointRouteBuilder MapLoginUser(this IEndpointRouteBuilder endpoints)
    {
        var authGroup = endpoints.MapAuthGroup();

        authGroup.MapPost("/login", HandleAsync)
            .WithName("LoginUserV1")
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                operation.Summary = "Login user";
                operation.Description = "Authenticates a user and returns a JWT access token";
                return Task.CompletedTask;
            })
            .Produces<LoginUserResponse>(StatusCodes.Status200OK)
            .Produces<AuthErrorResponse>(StatusCodes.Status401Unauthorized)
            .Produces<AuthErrorResponse>(StatusCodes.Status400BadRequest)
            .DisableAntiforgery()
            .AllowAnonymous();

        return endpoints;
    }

    private static async Task<IResult> HandleAsync(
        LoginUserRequest? request,
        IAuthService authService,
        IValidator<LoginUserRequest> validator,
        ILogger<LoginUserRequest> logger,
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
            logger.LogWarning("Login validation failed for email: {Email}", request.Email);
            return Results.BadRequest(new AuthErrorResponse(errors));
        }

        // Call authentication service
        var result = await authService.LoginAsync(request.Email, request.Password);

        if (!result.Succeeded)
        {
            logger.LogWarning("Login failed for email: {Email}", request.Email);
            return Results.BadRequest(new AuthErrorResponse(result.Errors));
        }

        logger.LogInformation("User successfully logged in: {UserId} - {Email}",
            result.Data!.UserId, result.Data.Email);

        return Results.Ok(result.Data);
    }
}