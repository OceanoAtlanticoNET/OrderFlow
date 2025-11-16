using Microsoft.AspNetCore.Identity;
using OrderFlow.Identity.Features.Auth.V1;
using OrderFlow.Identity.Services.Common;

namespace OrderFlow.Identity.Services.Auth;

/// <summary>
/// Service for authentication operations (login, register, current user)
/// </summary>
public class AuthService : IAuthService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        ITokenService tokenService,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _logger = logger;
    }

    /// <summary>
    /// Authenticate user with email and password
    /// </summary>
    public async Task<AuthResult<LoginUser.LoginUserResponse>> LoginAsync(string email, string password)
    {
        // Find user by email
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            _logger.LogWarning("Login attempt with non-existent email: {Email}", email);
            return AuthResult<LoginUser.LoginUserResponse>.Failure("Invalid email or password");
        }

        // Check password and handle lockout
        var result = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);

        if (result.IsLockedOut)
        {
            _logger.LogWarning("User account locked out: {Email}", email);
            return AuthResult<LoginUser.LoginUserResponse>.Failure(
                "Account is locked due to multiple failed login attempts. Please try again later.");
        }

        if (!result.Succeeded)
        {
            _logger.LogWarning("Failed login attempt for email: {Email}", email);
            return AuthResult<LoginUser.LoginUserResponse>.Failure("Invalid email or password");
        }

        // Get user roles
        var roles = await _userManager.GetRolesAsync(user);

        // Generate JWT token
        var token = await _tokenService.GenerateAccessTokenAsync(user, roles);
        var expiresIn = _tokenService.GetTokenExpiryInSeconds();

        _logger.LogInformation("User successfully logged in: {UserId} - {Email}", user.Id, user.Email);

        var response = new LoginUser.LoginUserResponse(
            AccessToken: token,
            TokenType: "Bearer",
            ExpiresIn: expiresIn,
            UserId: user.Id,
            Email: user.Email!,
            Roles: roles
        );

        return AuthResult<LoginUser.LoginUserResponse>.Success(response);
    }

    /// <summary>
    /// Register a new user account
    /// </summary>
    public async Task<AuthResult<RegisterUser.RegisterUserResponse>> RegisterAsync(
        string email,
        string password)
    {
        // Check if user already exists
        var existingUser = await _userManager.FindByEmailAsync(email);
        if (existingUser is not null)
        {
            _logger.LogWarning("Registration attempt with existing email: {Email}", email);
            return AuthResult<RegisterUser.RegisterUserResponse>.Failure(
                "A user with this email already exists");
        }

        // Create new user with email prefix as username (part before @)
        var userName = email.Split('@')[0];
        var user = new IdentityUser
        {
            UserName = userName,
            Email = email,
            EmailConfirmed = false
        };

        var createResult = await _userManager.CreateAsync(user, password);

        if (!createResult.Succeeded)
        {
            var errors = createResult.Errors.Select(e => e.Description);
            _logger.LogError("Failed to create user {Email}: {Errors}",
                email, string.Join(", ", errors));

            return AuthResult<RegisterUser.RegisterUserResponse>.Failure(errors);
        }

        // Add Customer role
        var roleResult = await _userManager.AddToRoleAsync(user, Data.Roles.Customer);
        if (!roleResult.Succeeded)
        {
            _logger.LogWarning("Failed to assign Customer role to user {UserId}: {Errors}",
                user.Id, string.Join(", ", roleResult.Errors.Select(e => e.Description)));
        }

        _logger.LogInformation("User successfully registered: {UserId} - {Email}",
            user.Id, user.Email);

        var response = new RegisterUser.RegisterUserResponse(
            UserId: user.Id,
            Email: user.Email!,
            Message: "User registered successfully. Please check your email to confirm your account."
        );

        return AuthResult<RegisterUser.RegisterUserResponse>.Success(response);
    }

    /// <summary>
    /// Get current user information from user ID
    /// </summary>
    public async Task<AuthResult<GetCurrentUser.CurrentUserResponse>> GetCurrentUserAsync(string userId)
    {
        // Find user by ID
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            _logger.LogWarning("Get current user failed: User not found {UserId}", userId);
            return AuthResult<GetCurrentUser.CurrentUserResponse>.Failure("User not found");
        }

        // Get user roles
        var roles = await _userManager.GetRolesAsync(user);

        var response = new GetCurrentUser.CurrentUserResponse(
            UserId: user.Id,
            Email: user.Email!,
            Roles: roles
        );

        return AuthResult<GetCurrentUser.CurrentUserResponse>.Success(response);
    }
}
