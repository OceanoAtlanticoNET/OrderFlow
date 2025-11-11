using Microsoft.AspNetCore.Identity;
using FluentValidation;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OrderFlow.Identity.Features.Auth;

public static class LoginUser
{
    public sealed record Request(
        string Email,
        string Password
    );

    public sealed record Response(
        string AccessToken,
        string TokenType,
        int ExpiresIn,
        string UserId,
        string Email,
        IEnumerable<string> Roles
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
                .NotEmpty().WithMessage("Password is required");
        }
    }

    public static IEndpointRouteBuilder MapLoginUser(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/auth/login", HandleAsync)
            .WithName("LoginUser")
            .WithTags("Authentication")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Login user";
                operation.Description = "Authenticates a user and returns a JWT access token";
                return operation;
            })
            .Produces<Response>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .DisableAntiforgery()
            .AllowAnonymous();

        return endpoints;
    }

    private static async Task<IResult> HandleAsync(
        Request? request,
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        IValidator<Request> validator,
        IConfiguration configuration,
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
            logger.LogWarning("Login validation failed for email: {Email}", request.Email);
            return Results.BadRequest(new ErrorResponse(errors));
        }

        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            logger.LogWarning("Login attempt with non-existent email: {Email}", request.Email);
            return Results.Unauthorized();
        }

        var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

        if (result.IsLockedOut)
        {
            logger.LogWarning("User account locked out: {Email}", request.Email);
            return Results.BadRequest(new ErrorResponse(["Account is locked due to multiple failed login attempts. Please try again later."]));
        }

        if (!result.Succeeded)
        {
            logger.LogWarning("Failed login attempt for email: {Email}", request.Email);
            return Results.Unauthorized();
        }

        // Get user roles
        var roles = await userManager.GetRolesAsync(user);

        // Generate JWT token
        var token = GenerateJwtToken(user, roles, configuration);
        var expiryMinutes = int.Parse(configuration["Jwt:ExpiryInMinutes"] ?? "60");

        logger.LogInformation("User successfully logged in: {UserId} - {Email}", user.Id, user.Email);

        var response = new Response(
            AccessToken: token,
            TokenType: "Bearer",
            ExpiresIn: expiryMinutes * 60, // Convert to seconds
            UserId: user.Id,
            Email: user.Email!,
            Roles: roles
        );

        return Results.Ok(response);
    }

    private static string GenerateJwtToken(IdentityUser user, IList<string> roles, IConfiguration configuration)
    {
        var jwtSecret = configuration["Jwt:Secret"]!;
        var jwtIssuer = configuration["Jwt:Issuer"]!;
        var jwtAudience = configuration["Jwt:Audience"]!;
        var expiryMinutes = int.Parse(configuration["Jwt:ExpiryInMinutes"] ?? "60");

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email!),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.NameIdentifier, user.Id)
        };

        // Add role claims
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
