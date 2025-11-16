using OrderFlow.Identity.Features.Auth.V1;
using OrderFlow.Identity.Services.Common;

namespace OrderFlow.Identity.Services.Auth;

/// <summary>
/// Service for authentication operations (login, register, current user)
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Authenticate user with email and password
    /// </summary>
    /// <param name="email">User email address</param>
    /// <param name="password">User password</param>
    /// <returns>Authentication result with token and user information</returns>
    Task<AuthResult<LoginUser.LoginUserResponse>> LoginAsync(string email, string password);

    /// <summary>
    /// Register a new user account
    /// </summary>
    /// <param name="email">User email address</param>
    /// <param name="password">User password</param>
    /// <param name="firstName">User first name</param>
    /// <param name="lastName">User last name</param>
    /// <returns>Registration result with user information</returns>
    Task<AuthResult<RegisterUser.RegisterUserResponse>> RegisterAsync(
        string email,
        string password,
        string firstName,
        string lastName);

    /// <summary>
    /// Get current user information from user ID
    /// </summary>
    /// <param name="userId">User ID from JWT claims</param>
    /// <returns>Current user information</returns>
    Task<AuthResult<GetCurrentUser.CurrentUserResponse>> GetCurrentUserAsync(string userId);
}
