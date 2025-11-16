# Service Layer Architecture

**Version:** 1.0
**Pattern:** Service Repository Pattern with Dependency Injection

---

## Overview

The service layer abstracts business logic from endpoint handlers, providing:
- **Reusability**: Services can be used by both V1 and V2 APIs
- **Testability**: Business logic can be unit tested independently
- **Maintainability**: Single source of truth for operations
- **Separation of Concerns**: Endpoints handle HTTP, services handle business logic

---

## Service Architecture

```
┌─────────────────────────────────────────────────────┐
│                  API Layer (V1/V2)                  │
│  (Minimal APIs / Controllers - HTTP concerns)       │
└─────────────────────┬───────────────────────────────┘
                      │
                      ▼
┌─────────────────────────────────────────────────────┐
│                  Service Layer                      │
│  (Business Logic, Validation, Orchestration)        │
│                                                     │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────┐ │
│  │ AuthService  │  │ UserService  │  │RoleService│ │
│  │              │  │              │  │           │ │
│  │ TokenService │  │              │  │           │ │
│  └──────────────┘  └──────────────┘  └──────────┘ │
└─────────────────────┬───────────────────────────────┘
                      │
                      ▼
┌─────────────────────────────────────────────────────┐
│              ASP.NET Core Identity                  │
│  (UserManager, SignInManager, RoleManager)          │
└─────────────────────┬───────────────────────────────┘
                      │
                      ▼
┌─────────────────────────────────────────────────────┐
│              Database (PostgreSQL)                  │
│  (AspNetUsers, AspNetRoles, AspNetUserRoles)        │
└─────────────────────────────────────────────────────┘
```

---

## Service Interfaces & Implementations

### 1. ITokenService / TokenService

**Purpose:** Generate and validate JWT tokens

**Location:**
- Interface: `Services/Auth/ITokenService.cs`
- Implementation: `Services/Auth/TokenService.cs`

**Interface:**
```csharp
public interface ITokenService
{
    /// <summary>
    /// Generates JWT access token for authenticated user
    /// </summary>
    /// <param name="user">The authenticated user</param>
    /// <param name="roles">User's roles</param>
    /// <returns>JWT token string</returns>
    Task<string> GenerateAccessTokenAsync(IdentityUser user, IEnumerable<string> roles);

    /// <summary>
    /// Gets token expiry time in seconds
    /// </summary>
    /// <returns>Expiry time in seconds</returns>
    int GetTokenExpiryInSeconds();
}
```

**Implementation Details:**
```csharp
public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Task<string> GenerateAccessTokenAsync(IdentityUser user, IEnumerable<string> roles)
    {
        // 1. Read JWT configuration (Secret, Issuer, Audience, ExpiryInMinutes)
        // 2. Create claims (sub, email, role claims)
        // 3. Create signing credentials with HMAC-SHA256
        // 4. Generate JWT token
        // 5. Return token string
    }

    public int GetTokenExpiryInSeconds()
    {
        // Return configured expiry time in seconds
    }
}
```

**Dependencies:**
- `IConfiguration` - For JWT settings

---

### 2. IAuthService / AuthService

**Purpose:** Handle authentication operations (login, register)

**Location:**
- Interface: `Services/Auth/IAuthService.cs`
- Implementation: `Services/Auth/AuthService.cs`

**Interface:**
```csharp
public interface IAuthService
{
    /// <summary>
    /// Authenticate user with email and password
    /// </summary>
    /// <param name="email">User email</param>
    /// <param name="password">User password</param>
    /// <returns>Login result with token and user info</returns>
    Task<AuthResult<LoginResponse>> LoginAsync(string email, string password);

    /// <summary>
    /// Register new user account
    /// </summary>
    /// <param name="email">User email</param>
    /// <param name="password">User password</param>
    /// <param name="firstName">User first name</param>
    /// <param name="lastName">User last name</param>
    /// <returns>Registration result with user info</returns>
    Task<AuthResult<RegisterResponse>> RegisterAsync(string email, string password, string firstName, string lastName);

    /// <summary>
    /// Get current user information from user ID
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Current user information</returns>
    Task<AuthResult<CurrentUserResponse>> GetCurrentUserAsync(string userId);
}
```

**Result Wrapper:**
```csharp
public class AuthResult<T>
{
    public bool Succeeded { get; set; }
    public T? Data { get; set; }
    public IEnumerable<string> Errors { get; set; } = new List<string>();
}
```

**Implementation Details:**
```csharp
public class AuthService : IAuthService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly ITokenService _tokenService;

    public AuthService(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        ITokenService tokenService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
    }

    public async Task<AuthResult<LoginResponse>> LoginAsync(string email, string password)
    {
        // 1. Find user by email
        // 2. Check if user exists
        // 3. Attempt sign-in with password (handles lockout)
        // 4. Get user roles
        // 5. Generate JWT token
        // 6. Return LoginResponse with token and user info
    }

    public async Task<AuthResult<RegisterResponse>> RegisterAsync(...)
    {
        // 1. Check if email already exists
        // 2. Create new IdentityUser
        // 3. Create user with password
        // 4. Add default "Customer" role
        // 5. Return RegisterResponse with user info
    }

    public async Task<AuthResult<CurrentUserResponse>> GetCurrentUserAsync(string userId)
    {
        // 1. Find user by ID
        // 2. Get user roles
        // 3. Return CurrentUserResponse
    }
}
```

**Dependencies:**
- `UserManager<IdentityUser>` - User operations
- `SignInManager<IdentityUser>` - Authentication
- `ITokenService` - Token generation

---

### 3. IUserService / UserService

**Purpose:** Handle user management operations

**Location:**
- Interface: `Services/Users/IUserService.cs`
- Implementation: `Services/Users/UserService.cs`

**Interface:**
```csharp
public interface IUserService
{
    // List & Search
    Task<PaginatedResult<UserResponse>> GetUsersAsync(UserQueryParameters parameters);

    // Individual User Operations
    Task<ServiceResult<UserDetailResponse>> GetUserByIdAsync(string userId);
    Task<ServiceResult<UserResponse>> CreateUserAsync(CreateUserRequest request);
    Task<ServiceResult<UserResponse>> UpdateUserAsync(string userId, UpdateUserRequest request);
    Task<ServiceResult> DeleteUserAsync(string userId);

    // Account Management
    Task<ServiceResult> LockUserAsync(string userId, DateTimeOffset? lockoutEnd);
    Task<ServiceResult> UnlockUserAsync(string userId);

    // Profile Management (Self)
    Task<ServiceResult<UserDetailResponse>> GetCurrentUserProfileAsync(string userId);
    Task<ServiceResult<UserResponse>> UpdateCurrentUserProfileAsync(string userId, UpdateProfileRequest request);
    Task<ServiceResult> ChangePasswordAsync(string userId, string currentPassword, string newPassword);

    // Role Assignment
    Task<ServiceResult<IEnumerable<string>>> GetUserRolesAsync(string userId);
    Task<ServiceResult> AddUserToRoleAsync(string userId, string roleName);
    Task<ServiceResult> RemoveUserFromRoleAsync(string userId, string roleName);
}
```

**Result Wrappers:**
```csharp
public class ServiceResult
{
    public bool Succeeded { get; set; }
    public string Message { get; set; } = string.Empty;
    public IEnumerable<string> Errors { get; set; } = new List<string>();
}

public class ServiceResult<T> : ServiceResult
{
    public T? Data { get; set; }
}

public class PaginatedResult<T>
{
    public IEnumerable<T> Data { get; set; } = new List<T>();
    public PaginationMetadata Pagination { get; set; } = new();
}

public class PaginationMetadata
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
}
```

**Implementation Details:**
```csharp
public class UserService : IUserService
{
    private readonly UserManager<IdentityUser> _userManager;

    public UserService(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<PaginatedResult<UserResponse>> GetUsersAsync(UserQueryParameters parameters)
    {
        // 1. Get queryable users from UserManager
        // 2. Apply search filter (email, username)
        // 3. Apply role filter if specified
        // 4. Apply sorting
        // 5. Calculate pagination
        // 6. Execute query with Skip/Take
        // 7. Map to UserResponse DTOs
        // 8. Return paginated result
    }

    public async Task<ServiceResult<UserDetailResponse>> GetUserByIdAsync(string userId)
    {
        // 1. Find user by ID
        // 2. Return 404 if not found
        // 3. Get user roles
        // 4. Map to UserDetailResponse
        // 5. Return result
    }

    public async Task<ServiceResult<UserResponse>> CreateUserAsync(CreateUserRequest request)
    {
        // 1. Create IdentityUser from request
        // 2. Create user with password
        // 3. Check result (email exists, weak password, etc.)
        // 4. Add roles if specified
        // 5. Return UserResponse
    }

    public async Task<ServiceResult<UserResponse>> UpdateUserAsync(string userId, UpdateUserRequest request)
    {
        // 1. Find user by ID
        // 2. Update properties (email, username, phone)
        // 3. Validate (email/username not taken)
        // 4. Update user
        // 5. Return updated UserResponse
    }

    public async Task<ServiceResult> DeleteUserAsync(string userId)
    {
        // 1. Find user by ID
        // 2. Delete user
        // 3. Return result
    }

    public async Task<ServiceResult> LockUserAsync(string userId, DateTimeOffset? lockoutEnd)
    {
        // 1. Find user by ID
        // 2. Set lockout end date
        // 3. Update user
        // 4. Return result
    }

    public async Task<ServiceResult> UnlockUserAsync(string userId)
    {
        // 1. Find user by ID
        // 2. Clear lockout (set LockoutEnd = null)
        // 3. Reset access failed count
        // 4. Update user
        // 5. Return result
    }

    public async Task<ServiceResult> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
    {
        // 1. Find user by ID
        // 2. Change password with UserManager
        // 3. Check if current password is correct
        // 4. Validate new password strength
        // 5. Return result
    }

    public async Task<ServiceResult<IEnumerable<string>>> GetUserRolesAsync(string userId)
    {
        // 1. Find user by ID
        // 2. Get user roles
        // 3. Return role names
    }

    public async Task<ServiceResult> AddUserToRoleAsync(string userId, string roleName)
    {
        // 1. Find user by ID
        // 2. Check if role exists
        // 3. Check if user already has role
        // 4. Add user to role
        // 5. Return result
    }

    public async Task<ServiceResult> RemoveUserFromRoleAsync(string userId, string roleName)
    {
        // 1. Find user by ID
        // 2. Check if user has role
        // 3. Get all user roles
        // 4. Prevent removing last role
        // 5. Remove user from role
        // 6. Return result
    }
}
```

**Dependencies:**
- `UserManager<IdentityUser>` - User CRUD operations

---

### 4. IRoleService / RoleService

**Purpose:** Handle role management operations

**Location:**
- Interface: `Services/Roles/IRoleService.cs`
- Implementation: `Services/Roles/RoleService.cs`

**Interface:**
```csharp
public interface IRoleService
{
    // Role CRUD
    Task<ServiceResult<IEnumerable<RoleResponse>>> GetAllRolesAsync();
    Task<ServiceResult<RoleDetailResponse>> GetRoleByIdAsync(string roleId);
    Task<ServiceResult<RoleResponse>> CreateRoleAsync(string roleName);
    Task<ServiceResult<RoleResponse>> UpdateRoleAsync(string roleId, string newRoleName);
    Task<ServiceResult> DeleteRoleAsync(string roleId);

    // Role Members
    Task<PaginatedResult<UserResponse>> GetUsersInRoleAsync(string roleId, PaginationQuery pagination);
}
```

**Implementation Details:**
```csharp
public class RoleService : IRoleService
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<IdentityUser> _userManager;

    public RoleService(
        RoleManager<IdentityRole> roleManager,
        UserManager<IdentityUser> userManager)
    {
        _roleManager = roleManager;
        _userManager = userManager;
    }

    public async Task<ServiceResult<IEnumerable<RoleResponse>>> GetAllRolesAsync()
    {
        // 1. Get all roles from RoleManager
        // 2. For each role, count users with that role
        // 3. Map to RoleResponse DTOs
        // 4. Return result
    }

    public async Task<ServiceResult<RoleDetailResponse>> GetRoleByIdAsync(string roleId)
    {
        // 1. Find role by ID
        // 2. Return 404 if not found
        // 3. Count users in role
        // 4. Map to RoleDetailResponse
        // 5. Return result
    }

    public async Task<ServiceResult<RoleResponse>> CreateRoleAsync(string roleName)
    {
        // 1. Check if role already exists
        // 2. Create new IdentityRole
        // 3. Create role with RoleManager
        // 4. Return RoleResponse
    }

    public async Task<ServiceResult<RoleResponse>> UpdateRoleAsync(string roleId, string newRoleName)
    {
        // 1. Find role by ID
        // 2. Check if new name already exists
        // 3. Update role name
        // 4. Update with RoleManager
        // 5. Return updated RoleResponse
    }

    public async Task<ServiceResult> DeleteRoleAsync(string roleId)
    {
        // 1. Find role by ID
        // 2. Check if role has users
        // 3. Prevent deletion if users exist
        // 4. Delete role
        // 5. Return result
    }

    public async Task<PaginatedResult<UserResponse>> GetUsersInRoleAsync(string roleId, PaginationQuery pagination)
    {
        // 1. Find role by ID
        // 2. Get role name
        // 3. Get all users in role
        // 4. Apply pagination
        // 5. Map to UserResponse DTOs
        // 6. Return paginated result
    }
}
```

**Dependencies:**
- `RoleManager<IdentityRole>` - Role CRUD operations
- `UserManager<IdentityUser>` - User enumeration for role members

---

## Service Registration

**Location:** `Program.cs`

```csharp
// Register services with Dependency Injection
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRoleService, RoleService>();
```

**Lifetime:** `Scoped` (per HTTP request)
- Services should be scoped to match ASP.NET Core Identity managers
- Each request gets fresh service instances
- Prevents state leakage between requests

---

## Error Handling Strategy

### Service Layer Responsibilities
- Validate business rules
- Return structured results (ServiceResult, AuthResult)
- Never throw exceptions for expected failures (user not found, duplicate email, etc.)
- Let unexpected exceptions bubble up to global handler

### Example Service Error Handling
```csharp
public async Task<ServiceResult<UserResponse>> CreateUserAsync(CreateUserRequest request)
{
    // Check if email exists (expected failure - return error)
    var existingUser = await _userManager.FindByEmailAsync(request.Email);
    if (existingUser != null)
    {
        return new ServiceResult<UserResponse>
        {
            Succeeded = false,
            Errors = new[] { "Email already exists" }
        };
    }

    // Create user
    var user = new IdentityUser { Email = request.Email, UserName = request.Email };
    var result = await _userManager.CreateAsync(user, request.Password);

    if (!result.Succeeded)
    {
        return new ServiceResult<UserResponse>
        {
            Succeeded = false,
            Errors = result.Errors.Select(e => e.Description)
        };
    }

    // Success
    return new ServiceResult<UserResponse>
    {
        Succeeded = true,
        Data = new UserResponse { /* map user */ },
        Message = "User created successfully"
    };
}
```

### Endpoint Layer Responsibilities
- Map ServiceResult to HTTP status codes
- Handle unexpected exceptions (500 Internal Server Error)
- Log errors
- Return consistent error response format

```csharp
// Example endpoint using service
app.MapPost("/api/v1/admin/users", async (CreateUserRequest request, IUserService userService) =>
{
    var result = await userService.CreateUserAsync(request);

    if (!result.Succeeded)
    {
        return Results.BadRequest(new ErrorResponse { Errors = result.Errors });
    }

    return Results.Created($"/api/v1/admin/users/{result.Data.UserId}", result.Data);
})
.RequireAuthorization("admin");
```

---

## Validation Strategy

### Input Validation
- FluentValidation at endpoint level (existing pattern)
- Validators automatically run before endpoint handler
- Return 400 Bad Request with validation errors

### Business Rule Validation
- Service layer validates business rules:
  - User cannot delete themselves
  - User cannot lock themselves
  - User must have at least one role
  - Role cannot be deleted if it has users
  - etc.

---

## Testing Strategy

### Unit Tests for Services
```csharp
public class UserServiceTests
{
    [Fact]
    public async Task CreateUser_DuplicateEmail_ReturnsFailed()
    {
        // Arrange
        var mockUserManager = CreateMockUserManager();
        mockUserManager.Setup(x => x.FindByEmailAsync("test@example.com"))
            .ReturnsAsync(new IdentityUser());

        var service = new UserService(mockUserManager.Object);

        // Act
        var result = await service.CreateUserAsync(new CreateUserRequest
        {
            Email = "test@example.com"
        });

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Email already exists", result.Errors);
    }
}
```

### Integration Tests for Endpoints
```csharp
public class UserEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task GetUsers_AdminRole_ReturnsUsers()
    {
        // Arrange
        var client = _factory.CreateClient();
        var token = await GetAdminTokenAsync();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.GetAsync("/api/v1/admin/users");

        // Assert
        response.EnsureSuccessStatusCode();
        var users = await response.Content.ReadFromJsonAsync<PaginatedResponse<UserResponse>>();
        Assert.NotEmpty(users.Data);
    }
}
```

---

## Migration Path

### Phase 1: Create Services
1. Create service interfaces
2. Implement service classes
3. Register services in DI container
4. Write unit tests

### Phase 2: Refactor Existing Endpoints
1. Update LoginUser.cs to use IAuthService
2. Update RegisterUser.cs to use IAuthService
3. Update GetCurrentUser.cs to use IAuthService
4. Keep endpoint structure same, just delegate to services

### Phase 3: Add New Endpoints
1. Create new endpoint files (e.g., GetUsers.cs)
2. Use services for all business logic
3. Keep endpoints thin (validation + service call + HTTP response)

---

## Benefits

1. **Separation of Concerns**: Endpoints handle HTTP, services handle business logic
2. **Reusability**: Services can be used by V1, V2, or future API versions
3. **Testability**: Easy to unit test services in isolation
4. **Maintainability**: Business logic in one place
5. **Consistency**: Same logic used across all endpoints
6. **Single Responsibility**: Each service has one clear purpose

---

## Notes

- All services use ASP.NET Core Identity managers (UserManager, RoleManager, SignInManager)
- No direct database access - Identity handles all data operations
- Services return structured results, not exceptions
- Endpoints map service results to HTTP responses
- All services are scoped to HTTP request lifetime
