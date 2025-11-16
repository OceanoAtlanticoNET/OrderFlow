# DTOs Structure & Organization

**Version:** 1.0
**Pattern:** Request/Response segregation with feature-based organization

---

## Overview

DTOs (Data Transfer Objects) define the contract between API and clients. This document outlines the complete DTO structure for the Identity API.

**Key Principles:**
- Separate Request and Response DTOs
- Feature-based folder organization
- Immutable records for DTOs
- Clear, descriptive naming
- Shared common DTOs

---

## Folder Structure

```
Models/
├── Auth/
│   ├── Requests/
│   │   ├── LoginRequest.cs
│   │   └── RegisterRequest.cs
│   └── Responses/
│       ├── LoginResponse.cs
│       ├── RegisterResponse.cs
│       └── CurrentUserResponse.cs
├── Users/
│   ├── Requests/
│   │   ├── CreateUserRequest.cs
│   │   ├── UpdateUserRequest.cs
│   │   ├── UpdateProfileRequest.cs
│   │   └── ChangePasswordRequest.cs
│   ├── Responses/
│   │   ├── UserResponse.cs
│   │   └── UserDetailResponse.cs
│   └── Queries/
│       └── UserQueryParameters.cs
├── Roles/
│   ├── Requests/
│   │   ├── CreateRoleRequest.cs
│   │   └── UpdateRoleRequest.cs
│   └── Responses/
│       ├── RoleResponse.cs
│       └── RoleDetailResponse.cs
└── Common/
    ├── ErrorResponse.cs
    ├── PaginatedResponse.cs
    ├── PaginationMetadata.cs
    └── PaginationQuery.cs
```

---

## Authentication DTOs

### LoginRequest
**File:** `Models/Auth/Requests/LoginRequest.cs`

```csharp
namespace OrderFlow.Identity.Models.Auth.Requests;

/// <summary>
/// Request model for user login
/// </summary>
public record LoginRequest
{
    /// <summary>
    /// User email address
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// User password
    /// </summary>
    public required string Password { get; init; }
}
```

**Validation Rules:**
- Email: Required, valid email format
- Password: Required

**Example:**
```json
{
  "email": "user@example.com",
  "password": "SecurePass123!"
}
```

---

### RegisterRequest
**File:** `Models/Auth/Requests/RegisterRequest.cs`

```csharp
namespace OrderFlow.Identity.Models.Auth.Requests;

/// <summary>
/// Request model for user registration
/// </summary>
public record RegisterRequest
{
    /// <summary>
    /// User email address
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// User password
    /// </summary>
    public required string Password { get; init; }

    /// <summary>
    /// User first name
    /// </summary>
    public required string FirstName { get; init; }

    /// <summary>
    /// User last name
    /// </summary>
    public required string LastName { get; init; }
}
```

**Validation Rules:**
- Email: Required, valid email format, max 256 chars
- Password: Required, min 8 chars, uppercase, lowercase, digit, special char
- FirstName: Required, max 100 chars
- LastName: Required, max 100 chars

**Example:**
```json
{
  "email": "newuser@example.com",
  "password": "SecurePass123!",
  "firstName": "John",
  "lastName": "Doe"
}
```

---

### LoginResponse
**File:** `Models/Auth/Responses/LoginResponse.cs`

```csharp
namespace OrderFlow.Identity.Models.Auth.Responses;

/// <summary>
/// Response model for successful login
/// </summary>
public record LoginResponse
{
    /// <summary>
    /// JWT access token
    /// </summary>
    public required string AccessToken { get; init; }

    /// <summary>
    /// Token type (always "Bearer")
    /// </summary>
    public required string TokenType { get; init; } = "Bearer";

    /// <summary>
    /// Token expiry time in seconds
    /// </summary>
    public required int ExpiresIn { get; init; }

    /// <summary>
    /// User ID
    /// </summary>
    public required string UserId { get; init; }

    /// <summary>
    /// User email
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// User roles
    /// </summary>
    public required IEnumerable<string> Roles { get; init; }
}
```

**Example:**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "tokenType": "Bearer",
  "expiresIn": 3600,
  "userId": "123e4567-e89b-12d3-a456-426614174000",
  "email": "user@example.com",
  "roles": ["Customer"]
}
```

---

### RegisterResponse
**File:** `Models/Auth/Responses/RegisterResponse.cs`

```csharp
namespace OrderFlow.Identity.Models.Auth.Responses;

/// <summary>
/// Response model for successful registration
/// </summary>
public record RegisterResponse
{
    /// <summary>
    /// Newly created user ID
    /// </summary>
    public required string UserId { get; init; }

    /// <summary>
    /// User email
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// Success message
    /// </summary>
    public required string Message { get; init; }
}
```

**Example:**
```json
{
  "userId": "123e4567-e89b-12d3-a456-426614174000",
  "email": "newuser@example.com",
  "message": "User registered successfully"
}
```

---

### CurrentUserResponse
**File:** `Models/Auth/Responses/CurrentUserResponse.cs`

```csharp
namespace OrderFlow.Identity.Models.Auth.Responses;

/// <summary>
/// Response model for current user information
/// </summary>
public record CurrentUserResponse
{
    /// <summary>
    /// User ID
    /// </summary>
    public required string UserId { get; init; }

    /// <summary>
    /// User email
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// User roles
    /// </summary>
    public required IEnumerable<string> Roles { get; init; }
}
```

**Example:**
```json
{
  "userId": "123e4567-e89b-12d3-a456-426614174000",
  "email": "user@example.com",
  "roles": ["Customer", "Admin"]
}
```

---

## User Management DTOs

### CreateUserRequest
**File:** `Models/Users/Requests/CreateUserRequest.cs`

```csharp
namespace OrderFlow.Identity.Models.Users.Requests;

/// <summary>
/// Request model for creating a new user (admin operation)
/// </summary>
public record CreateUserRequest
{
    /// <summary>
    /// User email address
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// User password
    /// </summary>
    public required string Password { get; init; }

    /// <summary>
    /// Username (optional, defaults to email)
    /// </summary>
    public string? UserName { get; init; }

    /// <summary>
    /// Phone number (optional)
    /// </summary>
    public string? PhoneNumber { get; init; }

    /// <summary>
    /// Roles to assign to user (optional, defaults to ["Customer"])
    /// </summary>
    public IEnumerable<string>? Roles { get; init; }
}
```

**Validation Rules:**
- Email: Required, valid email format, max 256 chars
- Password: Required, min 8 chars, uppercase, lowercase, digit, special char
- UserName: Optional, max 256 chars, alphanumeric + underscore/dash
- PhoneNumber: Optional, valid phone format
- Roles: Optional, each role must exist

**Example:**
```json
{
  "email": "admin@example.com",
  "password": "AdminPass123!",
  "userName": "admin",
  "phoneNumber": "+1234567890",
  "roles": ["Admin", "Customer"]
}
```

---

### UpdateUserRequest
**File:** `Models/Users/Requests/UpdateUserRequest.cs`

```csharp
namespace OrderFlow.Identity.Models.Users.Requests;

/// <summary>
/// Request model for updating user information (admin operation)
/// </summary>
public record UpdateUserRequest
{
    /// <summary>
    /// User email address
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// Username
    /// </summary>
    public required string UserName { get; init; }

    /// <summary>
    /// Phone number (nullable)
    /// </summary>
    public string? PhoneNumber { get; init; }

    /// <summary>
    /// Email confirmed status
    /// </summary>
    public bool EmailConfirmed { get; init; }

    /// <summary>
    /// Lockout enabled
    /// </summary>
    public bool LockoutEnabled { get; init; }
}
```

**Example:**
```json
{
  "email": "updated@example.com",
  "userName": "updateduser",
  "phoneNumber": "+9876543210",
  "emailConfirmed": true,
  "lockoutEnabled": true
}
```

---

### UpdateProfileRequest
**File:** `Models/Users/Requests/UpdateProfileRequest.cs`

```csharp
namespace OrderFlow.Identity.Models.Users.Requests;

/// <summary>
/// Request model for updating own profile (self-management)
/// </summary>
public record UpdateProfileRequest
{
    /// <summary>
    /// Username
    /// </summary>
    public required string UserName { get; init; }

    /// <summary>
    /// Phone number (nullable)
    /// </summary>
    public string? PhoneNumber { get; init; }
}
```

**Validation Rules:**
- UserName: Required, max 256 chars, alphanumeric + underscore/dash
- PhoneNumber: Optional, valid phone format

**Example:**
```json
{
  "userName": "johndoe",
  "phoneNumber": "+1234567890"
}
```

---

### ChangePasswordRequest
**File:** `Models/Users/Requests/ChangePasswordRequest.cs`

```csharp
namespace OrderFlow.Identity.Models.Users.Requests;

/// <summary>
/// Request model for changing password
/// </summary>
public record ChangePasswordRequest
{
    /// <summary>
    /// Current password
    /// </summary>
    public required string CurrentPassword { get; init; }

    /// <summary>
    /// New password
    /// </summary>
    public required string NewPassword { get; init; }

    /// <summary>
    /// Confirm new password
    /// </summary>
    public required string ConfirmNewPassword { get; init; }
}
```

**Validation Rules:**
- CurrentPassword: Required
- NewPassword: Required, min 8 chars, uppercase, lowercase, digit, special char
- ConfirmNewPassword: Required, must match NewPassword

**Example:**
```json
{
  "currentPassword": "OldPass123!",
  "newPassword": "NewSecurePass456!",
  "confirmNewPassword": "NewSecurePass456!"
}
```

---

### UserQueryParameters
**File:** `Models/Users/Queries/UserQueryParameters.cs`

```csharp
namespace OrderFlow.Identity.Models.Users.Queries;

/// <summary>
/// Query parameters for filtering and paginating users
/// </summary>
public record UserQueryParameters
{
    /// <summary>
    /// Page number (starts at 1)
    /// </summary>
    public int Page { get; init; } = 1;

    /// <summary>
    /// Page size (max 100)
    /// </summary>
    public int PageSize { get; init; } = 10;

    /// <summary>
    /// Search term (searches email and username)
    /// </summary>
    public string? Search { get; init; }

    /// <summary>
    /// Filter by role name
    /// </summary>
    public string? Role { get; init; }

    /// <summary>
    /// Sort field (email, userName)
    /// </summary>
    public string SortBy { get; init; } = "email";

    /// <summary>
    /// Sort order (asc, desc)
    /// </summary>
    public string SortOrder { get; init; } = "asc";
}
```

**Example:**
```
?page=1&pageSize=20&search=john&role=Admin&sortBy=email&sortOrder=asc
```

---

### UserResponse
**File:** `Models/Users/Responses/UserResponse.cs`

```csharp
namespace OrderFlow.Identity.Models.Users.Responses;

/// <summary>
/// Response model for user information (list view)
/// </summary>
public record UserResponse
{
    /// <summary>
    /// User ID
    /// </summary>
    public required string UserId { get; init; }

    /// <summary>
    /// User email
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// Username
    /// </summary>
    public required string UserName { get; init; }

    /// <summary>
    /// Email confirmed status
    /// </summary>
    public bool EmailConfirmed { get; init; }

    /// <summary>
    /// Lockout end date (null if not locked)
    /// </summary>
    public DateTimeOffset? LockoutEnd { get; init; }

    /// <summary>
    /// Lockout enabled
    /// </summary>
    public bool LockoutEnabled { get; init; }

    /// <summary>
    /// Failed access attempts count
    /// </summary>
    public int AccessFailedCount { get; init; }

    /// <summary>
    /// User roles
    /// </summary>
    public required IEnumerable<string> Roles { get; init; }
}
```

**Example:**
```json
{
  "userId": "123e4567-e89b-12d3-a456-426614174000",
  "email": "user@example.com",
  "userName": "user@example.com",
  "emailConfirmed": true,
  "lockoutEnd": null,
  "lockoutEnabled": true,
  "accessFailedCount": 0,
  "roles": ["Customer"]
}
```

---

### UserDetailResponse
**File:** `Models/Users/Responses/UserDetailResponse.cs`

```csharp
namespace OrderFlow.Identity.Models.Users.Responses;

/// <summary>
/// Response model for detailed user information
/// </summary>
public record UserDetailResponse
{
    /// <summary>
    /// User ID
    /// </summary>
    public required string UserId { get; init; }

    /// <summary>
    /// User email
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// Username
    /// </summary>
    public required string UserName { get; init; }

    /// <summary>
    /// Email confirmed status
    /// </summary>
    public bool EmailConfirmed { get; init; }

    /// <summary>
    /// Phone number
    /// </summary>
    public string? PhoneNumber { get; init; }

    /// <summary>
    /// Phone number confirmed status
    /// </summary>
    public bool PhoneNumberConfirmed { get; init; }

    /// <summary>
    /// Two-factor authentication enabled
    /// </summary>
    public bool TwoFactorEnabled { get; init; }

    /// <summary>
    /// Lockout end date (null if not locked)
    /// </summary>
    public DateTimeOffset? LockoutEnd { get; init; }

    /// <summary>
    /// Lockout enabled
    /// </summary>
    public bool LockoutEnabled { get; init; }

    /// <summary>
    /// Failed access attempts count
    /// </summary>
    public int AccessFailedCount { get; init; }

    /// <summary>
    /// User roles
    /// </summary>
    public required IEnumerable<string> Roles { get; init; }
}
```

**Example:**
```json
{
  "userId": "123e4567-e89b-12d3-a456-426614174000",
  "email": "user@example.com",
  "userName": "user@example.com",
  "emailConfirmed": true,
  "phoneNumber": "+1234567890",
  "phoneNumberConfirmed": false,
  "twoFactorEnabled": false,
  "lockoutEnd": null,
  "lockoutEnabled": true,
  "accessFailedCount": 0,
  "roles": ["Customer", "Admin"]
}
```

---

## Role Management DTOs

### CreateRoleRequest
**File:** `Models/Roles/Requests/CreateRoleRequest.cs`

```csharp
namespace OrderFlow.Identity.Models.Roles.Requests;

/// <summary>
/// Request model for creating a new role
/// </summary>
public record CreateRoleRequest
{
    /// <summary>
    /// Role name
    /// </summary>
    public required string RoleName { get; init; }
}
```

**Validation Rules:**
- RoleName: Required, max 256 chars, alphanumeric + underscore/dash

**Example:**
```json
{
  "roleName": "Manager"
}
```

---

### UpdateRoleRequest
**File:** `Models/Roles/Requests/UpdateRoleRequest.cs`

```csharp
namespace OrderFlow.Identity.Models.Roles.Requests;

/// <summary>
/// Request model for updating a role
/// </summary>
public record UpdateRoleRequest
{
    /// <summary>
    /// New role name
    /// </summary>
    public required string RoleName { get; init; }
}
```

**Example:**
```json
{
  "roleName": "SeniorManager"
}
```

---

### RoleResponse
**File:** `Models/Roles/Responses/RoleResponse.cs`

```csharp
namespace OrderFlow.Identity.Models.Roles.Responses;

/// <summary>
/// Response model for role information
/// </summary>
public record RoleResponse
{
    /// <summary>
    /// Role ID
    /// </summary>
    public required string RoleId { get; init; }

    /// <summary>
    /// Role name
    /// </summary>
    public required string RoleName { get; init; }

    /// <summary>
    /// Normalized role name (uppercase)
    /// </summary>
    public required string NormalizedName { get; init; }

    /// <summary>
    /// Number of users with this role
    /// </summary>
    public int UserCount { get; init; }
}
```

**Example:**
```json
{
  "roleId": "123e4567-e89b-12d3-a456-426614174000",
  "roleName": "Admin",
  "normalizedName": "ADMIN",
  "userCount": 5
}
```

---

### RoleDetailResponse
**File:** `Models/Roles/Responses/RoleDetailResponse.cs`

```csharp
namespace OrderFlow.Identity.Models.Roles.Responses;

/// <summary>
/// Response model for detailed role information
/// </summary>
public record RoleDetailResponse
{
    /// <summary>
    /// Role ID
    /// </summary>
    public required string RoleId { get; init; }

    /// <summary>
    /// Role name
    /// </summary>
    public required string RoleName { get; init; }

    /// <summary>
    /// Normalized role name (uppercase)
    /// </summary>
    public required string NormalizedName { get; init; }

    /// <summary>
    /// Number of users with this role
    /// </summary>
    public int UserCount { get; init; }
}
```

---

## Common DTOs

### ErrorResponse
**File:** `Models/Common/ErrorResponse.cs`

```csharp
namespace OrderFlow.Identity.Models.Common;

/// <summary>
/// Standard error response model
/// </summary>
public record ErrorResponse
{
    /// <summary>
    /// List of error messages
    /// </summary>
    public required IEnumerable<string> Errors { get; init; }

    /// <summary>
    /// General error message (optional)
    /// </summary>
    public string? Message { get; init; }

    /// <summary>
    /// HTTP status code (optional)
    /// </summary>
    public int? StatusCode { get; init; }
}
```

**Example:**
```json
{
  "errors": ["Email already exists", "Password is too weak"],
  "message": "Validation failed",
  "statusCode": 400
}
```

---

### PaginatedResponse<T>
**File:** `Models/Common/PaginatedResponse.cs`

```csharp
namespace OrderFlow.Identity.Models.Common;

/// <summary>
/// Generic paginated response wrapper
/// </summary>
public record PaginatedResponse<T>
{
    /// <summary>
    /// Page data
    /// </summary>
    public required IEnumerable<T> Data { get; init; }

    /// <summary>
    /// Pagination metadata
    /// </summary>
    public required PaginationMetadata Pagination { get; init; }
}
```

**Example:**
```json
{
  "data": [ /* array of items */ ],
  "pagination": {
    "page": 1,
    "pageSize": 10,
    "totalCount": 100,
    "totalPages": 10
  }
}
```

---

### PaginationMetadata
**File:** `Models/Common/PaginationMetadata.cs`

```csharp
namespace OrderFlow.Identity.Models.Common;

/// <summary>
/// Pagination metadata
/// </summary>
public record PaginationMetadata
{
    /// <summary>
    /// Current page number
    /// </summary>
    public required int Page { get; init; }

    /// <summary>
    /// Page size
    /// </summary>
    public required int PageSize { get; init; }

    /// <summary>
    /// Total number of items
    /// </summary>
    public required int TotalCount { get; init; }

    /// <summary>
    /// Total number of pages
    /// </summary>
    public required int TotalPages { get; init; }
}
```

---

### PaginationQuery
**File:** `Models/Common/PaginationQuery.cs`

```csharp
namespace OrderFlow.Identity.Models.Common;

/// <summary>
/// Base pagination query parameters
/// </summary>
public record PaginationQuery
{
    /// <summary>
    /// Page number (starts at 1)
    /// </summary>
    public int Page { get; init; } = 1;

    /// <summary>
    /// Page size (max 100)
    /// </summary>
    public int PageSize { get; init; } = 10;
}
```

---

## Naming Conventions

### Requests
- Suffix: `Request`
- Examples: `LoginRequest`, `CreateUserRequest`, `UpdateRoleRequest`

### Responses
- Suffix: `Response`
- Examples: `UserResponse`, `LoginResponse`, `RoleResponse`
- Detail views: `UserDetailResponse`, `RoleDetailResponse`

### Queries
- Suffix: `Parameters` or `Query`
- Examples: `UserQueryParameters`, `PaginationQuery`

### Common Types
- Descriptive names: `ErrorResponse`, `PaginatedResponse<T>`

---

## Property Naming

- **C# (DTOs)**: PascalCase (e.g., `UserId`, `Email`)
- **JSON (Serialized)**: camelCase (e.g., `userId`, `email`)
- **Configured in**: `Program.cs` with `JsonSerializerOptions`

```csharp
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});
```

---

## Validation

### FluentValidation Validators

Each request DTO has a corresponding validator:

```
Validators/
├── Auth/
│   ├── LoginRequestValidator.cs
│   └── RegisterRequestValidator.cs
├── Users/
│   ├── CreateUserRequestValidator.cs
│   ├── UpdateUserRequestValidator.cs
│   ├── UpdateProfileRequestValidator.cs
│   └── ChangePasswordRequestValidator.cs
└── Roles/
    ├── CreateRoleRequestValidator.cs
    └── UpdateRoleRequestValidator.cs
```

### Validator Registration

```csharp
// Program.cs
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
```

---

## Migration from Existing DTOs

### Current V1 DTOs (embedded in endpoint files)
- Keep as-is for backward compatibility
- V1 endpoints continue using embedded DTOs

### New Shared DTOs
- Used by new endpoints
- Can be gradually adopted by V1 endpoints during refactoring

### V2 DTOs (in AuthController.cs)
- Keep as-is for backward compatibility
- V2 controller continues using inline DTOs

---

## Notes

1. All DTOs use `record` types for immutability
2. Required properties use `required` modifier (C# 11)
3. Nullable properties use `?` operator
4. Collections use `IEnumerable<T>` for flexibility
5. Date/time uses `DateTimeOffset` for timezone support
6. All DTOs have XML documentation comments
7. Example JSON included for each DTO
