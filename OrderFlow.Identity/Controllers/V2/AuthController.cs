using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;

namespace OrderFlow.Identity.Controllers.V2;

[ApiController]
[Route("api/v{version:apiVersion}/auth")]
[ApiVersion("2.0")]
[Tags("Authentication V2")]
public class AuthController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        IConfiguration configuration,
        ILogger<AuthController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Login user
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <param name="validator">Request validator</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>JWT token and user information</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Login(
        [FromBody] LoginRequest request,
        [FromServices] IValidator<LoginRequest> validator,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage);
            _logger.LogWarning("Login validation failed for email: {Email}", request.Email);
            return BadRequest(new ErrorResponse(errors));
        }

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            _logger.LogWarning("Login attempt with non-existent email: {Email}", request.Email);
            return BadRequest(new ErrorResponse(["Invalid email or password"]));
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

        if (result.IsLockedOut)
        {
            _logger.LogWarning("User account locked out: {Email}", request.Email);
            return BadRequest(new ErrorResponse(["Account is locked due to multiple failed login attempts. Please try again later."]));
        }

        if (!result.Succeeded)
        {
            _logger.LogWarning("Failed login attempt for email: {Email}", request.Email);
            return BadRequest(new ErrorResponse(["Invalid email or password"]));
        }

        // Get user roles
        var roles = await _userManager.GetRolesAsync(user);

        // Generate JWT token
        var token = GenerateJwtToken(user, roles);
        var expiryMinutes = int.Parse(_configuration["Jwt:ExpiryInMinutes"] ?? "60");

        _logger.LogInformation("User successfully logged in: {UserId} - {Email}", user.Id, user.Email);

        var response = new LoginResponse
        {
            AccessToken = token,
            TokenType = "Bearer",
            ExpiresIn = expiryMinutes * 60, // Convert to seconds
            UserId = user.Id,
            Email = user.Email!,
            Roles = roles
        };

        return Ok(response);
    }

    /// <summary>
    /// Register new user
    /// </summary>
    /// <param name="request">Registration data</param>
    /// <param name="validator">Request validator</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Registration confirmation</returns>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType<RegisterResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RegisterResponse>> Register(
        [FromBody] RegisterRequest request,
        [FromServices] IValidator<RegisterRequest> validator,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage);
            _logger.LogWarning("Registration validation failed for email: {Email}", request.Email);
            return BadRequest(new ErrorResponse(errors));
        }

        // Check if user already exists
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            _logger.LogWarning("Registration attempt with existing email: {Email}", request.Email);
            return BadRequest(new ErrorResponse(["User with this email already exists"]));
        }

        var user = new IdentityUser
        {
            UserName = request.Email,
            Email = request.Email,
            EmailConfirmed = true // Set to false in production and implement email confirmation
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description);
            _logger.LogWarning("User creation failed for email: {Email}. Errors: {@Errors}",
                request.Email, errors);
            return BadRequest(new ErrorResponse(errors));
        }

        // Add default role
        await _userManager.AddToRoleAsync(user, "Customer");

        _logger.LogInformation("User successfully registered: {UserId} - {Email}", user.Id, user.Email);

        var response = new RegisterResponse
        {
            UserId = user.Id,
            Email = user.Email,
            Message = "User registered successfully"
        };

        return CreatedAtAction(nameof(Login), new { }, response);
    }

    /// <summary>
    /// Get current user information
    /// </summary>
    /// <returns>Current user information</returns>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType<CurrentUserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CurrentUserResponse>> GetCurrentUser()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return Unauthorized();
        }

        var roles = await _userManager.GetRolesAsync(user);

        var response = new CurrentUserResponse
        {
            UserId = user.Id,
            Email = user.Email!,
            Roles = roles
        };

        return Ok(response);
    }

    /// <summary>
    /// Admin only endpoint
    /// </summary>
    /// <returns>Admin confirmation message</returns>
    [HttpGet("admin-only")]
    [ProducesResponseType<AdminOnlyResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Authorize(Roles = "Admin")]
    public ActionResult<AdminOnlyResponse> AdminOnly()
    {
        var response = new AdminOnlyResponse
        {
            Message = "You are an admin!",
            Timestamp = DateTime.UtcNow
        };

        return Ok(response);
    }

    private string GenerateJwtToken(IdentityUser user, IList<string> roles)
    {
        var jwtSecret = _configuration["Jwt:Secret"]!;
        var jwtIssuer = _configuration["Jwt:Issuer"]!;
        var jwtAudience = _configuration["Jwt:Audience"]!;
        var expiryMinutes = int.Parse(_configuration["Jwt:ExpiryInMinutes"] ?? "60");

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

// DTOs for Controllers pattern
public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string TokenType { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public IEnumerable<string> Roles { get; set; } = [];
}

public class RegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class RegisterResponse
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class CurrentUserResponse
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public IEnumerable<string> Roles { get; set; } = [];
}

public class ErrorResponse
{
    public IEnumerable<string> Errors { get; set; }

    public ErrorResponse(IEnumerable<string> errors)
    {
        Errors = errors;
    }
}

public class AdminOnlyResponse
{
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}