using Microsoft.AspNetCore.Identity;
using OrderFlow.Identity.Data;
using FluentValidation;

namespace OrderFlow.Identity.Features.Auth;

public static class RegisterUser
{
    public sealed record Request(
        string Email,
        string Password,
        string FirstName,
        string LastName
    );

    public sealed record Response(
        string UserId,
        string Email,
        string Message
    );

    public sealed record ErrorResponse(
        IEnumerable<string> Errors
    );

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Email format is invalid");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters long");

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required");
        }
    }

    public static IEndpointRouteBuilder MapRegisterUser(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/auth/register", HandleAsync)
            .WithName("RegisterUser")
            .WithTags("Authentication")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Register a new user";
                operation.Description = "Creates a new user account with the provided credentials";
                return operation;
            })
            .Produces<Response>(StatusCodes.Status201Created)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .DisableAntiforgery()
            .AllowAnonymous();

        return endpoints;
    }

    private static async Task<IResult> HandleAsync(
        Request? request,
        UserManager<IdentityUser> userManager,
        IValidator<Request> validator,
        ILogger<Request> logger,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            return Results.BadRequest(new ErrorResponse(["Request body is required"]));
        }
        
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage);
            logger.LogWarning("Registration validation failed for email: {Email}", request.Email);
            return Results.BadRequest(new ErrorResponse(errors));
        }

        var existingUser = await userManager.FindByEmailAsync(request.Email);
        if (existingUser is not null)
        {
            logger.LogWarning("Registration attempt with existing email: {Email}", request.Email);
            return Results.BadRequest(new ErrorResponse(["A user with this email already exists"]));
        }

        var user = new IdentityUser
        {
            UserName = request.Email,
            Email = request.Email,
            EmailConfirmed = false
        };

        var createResult = await userManager.CreateAsync(user, request.Password);

        if (!createResult.Succeeded)
        {
            var errors = createResult.Errors.Select(e => e.Description);
            logger.LogError("Failed to create user {Email}: {Errors}", 
                request.Email, string.Join(", ", errors));
            
            return Results.BadRequest(new ErrorResponse(errors));
        }

        var roleResult = await userManager.AddToRoleAsync(user, Roles.Customer);
        if (!roleResult.Succeeded)
        {
            logger.LogWarning("Failed to assign Customer role to user {UserId}: {Errors}", 
                user.Id, string.Join(", ", roleResult.Errors.Select(e => e.Description)));
        }

        logger.LogInformation("User successfully registered: {UserId} - {Email}", 
            user.Id, user.Email);

        var response = new Response(
            UserId: user.Id,
            Email: user.Email,
            Message: "User registered successfully. Please check your email to confirm your account."
        );

        return Results.Created($"/api/auth/users/{user.Id}", response);
    }
}
