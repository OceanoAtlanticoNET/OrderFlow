# Implementation Plan

**Version:** 1.0
**Target:** OrderFlow.Identity API - RESTful Improvements

---

## Overview

This document outlines the step-by-step implementation plan for improving the Identity API with proper service layer, user management, and role management following REST principles.

**Scope:**
- ✅ Create service layer abstraction
- ✅ Add user management endpoints (admin + self-management)
- ✅ Add role management endpoints
- ✅ Implement pagination and filtering
- ✅ Standardize response formats
- ❌ NO email features (no provider configured)
- ❌ NO database changes (use existing Identity tables)
- ❌ NO V2 deletion (keep for backward compatibility)

---

## Phase 1: Foundation (Service Layer)

**Goal:** Create service layer to abstract business logic from endpoints

### Step 1.1: Create Common DTOs & Result Types

**Priority:** HIGH
**Files to create:**
- `Models/Common/ErrorResponse.cs`
- `Models/Common/PaginatedResponse.cs`
- `Models/Common/PaginationMetadata.cs`
- `Models/Common/PaginationQuery.cs`
- `Services/Common/ServiceResult.cs` (helper class for service results)
- `Services/Common/AuthResult.cs` (helper class for auth results)
- `Services/Common/PaginatedResult.cs` (helper class for paginated results)

**Tasks:**
1. Create `Models/Common/` folder
2. Create `Services/Common/` folder
3. Implement ErrorResponse record
4. Implement PaginatedResponse<T> record
5. Implement PaginationMetadata record
6. Implement PaginationQuery record
7. Implement ServiceResult and ServiceResult<T> classes
8. Implement AuthResult<T> class
9. Implement PaginatedResult<T> class

**Validation:** Compile successfully, no errors

---

### Step 1.2: Create ITokenService & TokenService

**Priority:** HIGH
**Files to create:**
- `Services/Auth/ITokenService.cs`
- `Services/Auth/TokenService.cs`

**Tasks:**
1. Create `Services/Auth/` folder
2. Define ITokenService interface:
   - `Task<string> GenerateAccessTokenAsync(IdentityUser user, IEnumerable<string> roles)`
   - `int GetTokenExpiryInSeconds()`
3. Implement TokenService:
   - Read JWT config (Secret, Issuer, Audience, ExpiryInMinutes)
   - Create claims (sub, email, role claims)
   - Create signing credentials (HMAC-SHA256)
   - Generate JWT token
   - Return token string
4. Register service in Program.cs: `builder.Services.AddScoped<ITokenService, TokenService>();`

**Validation:**
- Compile successfully
- Service registered in DI container

---

### Step 1.3: Create IAuthService & AuthService

**Priority:** HIGH
**Files to create:**
- `Services/Auth/IAuthService.cs`
- `Services/Auth/AuthService.cs`

**Tasks:**
1. Define IAuthService interface:
   - `Task<AuthResult<LoginResponse>> LoginAsync(string email, string password)`
   - `Task<AuthResult<RegisterResponse>> RegisterAsync(string email, string password, string firstName, string lastName)`
   - `Task<AuthResult<CurrentUserResponse>> GetCurrentUserAsync(string userId)`
2. Implement AuthService:
   - Inject UserManager, SignInManager, ITokenService
   - Implement LoginAsync (find user, sign in, generate token)
   - Implement RegisterAsync (create user, add Customer role)
   - Implement GetCurrentUserAsync (find user, get roles)
3. Register service in Program.cs: `builder.Services.AddScoped<IAuthService, AuthService>();`

**Dependencies:**
- Requires existing auth DTOs from V1 (LoginResponse, RegisterResponse, CurrentUserResponse)
- Can reuse existing DTOs or create new ones in Models/Auth/Responses/

**Validation:**
- Compile successfully
- Service registered in DI container
- Unit test LoginAsync with valid/invalid credentials
- Unit test RegisterAsync with duplicate email

---

### Step 1.4: Create IUserService & UserService

**Priority:** HIGH
**Files to create:**
- `Services/Users/IUserService.cs`
- `Services/Users/UserService.cs`

**Tasks:**
1. Create `Services/Users/` folder
2. Define IUserService interface with methods:
   - List & Search: `GetUsersAsync(UserQueryParameters)`
   - CRUD: `GetUserByIdAsync`, `CreateUserAsync`, `UpdateUserAsync`, `DeleteUserAsync`
   - Account: `LockUserAsync`, `UnlockUserAsync`
   - Profile: `GetCurrentUserProfileAsync`, `UpdateCurrentUserProfileAsync`, `ChangePasswordAsync`
   - Roles: `GetUserRolesAsync`, `AddUserToRoleAsync`, `RemoveUserFromRoleAsync`
3. Implement UserService:
   - Inject UserManager
   - Implement all methods with proper error handling
   - Return ServiceResult/PaginatedResult
4. Register service in Program.cs: `builder.Services.AddScoped<IUserService, UserService>();`

**Validation:**
- Compile successfully
- Service registered in DI container
- Unit test GetUsersAsync with pagination
- Unit test CreateUserAsync with duplicate email
- Unit test DeleteUserAsync

---

### Step 1.5: Create IRoleService & RoleService

**Priority:** HIGH
**Files to create:**
- `Services/Roles/IRoleService.cs`
- `Services/Roles/RoleService.cs`

**Tasks:**
1. Create `Services/Roles/` folder
2. Define IRoleService interface with methods:
   - CRUD: `GetAllRolesAsync`, `GetRoleByIdAsync`, `CreateRoleAsync`, `UpdateRoleAsync`, `DeleteRoleAsync`
   - Members: `GetUsersInRoleAsync`
3. Implement RoleService:
   - Inject RoleManager, UserManager
   - Implement all methods with proper error handling
   - Return ServiceResult/PaginatedResult
   - Prevent deletion of roles with users
4. Register service in Program.cs: `builder.Services.AddScoped<IRoleService, RoleService>();`

**Validation:**
- Compile successfully
- Service registered in DI container
- Unit test CreateRoleAsync with duplicate name
- Unit test DeleteRoleAsync with users in role

---

### Step 1.6: Update Program.cs Service Registration

**Priority:** HIGH
**File to modify:**
- `Program.cs`

**Tasks:**
1. Add service registrations after Identity configuration:
```csharp
// Service layer
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRoleService, RoleService>();
```

**Validation:**
- Application runs without errors
- Services can be injected into endpoints

---

## Phase 2: Refactor Existing V1 Endpoints

**Goal:** Update existing V1 auth endpoints to use new service layer

### Step 2.1: Refactor LoginUser.cs

**Priority:** HIGH
**File to modify:**
- `Features/Auth/V1/LoginUser.cs`

**Tasks:**
1. Inject IAuthService instead of UserManager/SignInManager
2. Remove GenerateJwtToken private method (now in TokenService)
3. Update endpoint handler:
   - Call `await authService.LoginAsync(request.Email, request.Password)`
   - Map AuthResult to HTTP response (200 OK or 401 Unauthorized)
   - Return existing LoginUserResponse format for backward compatibility
4. Keep existing validator unchanged

**Validation:**
- Login with valid credentials returns token
- Login with invalid credentials returns 401
- Account lockout works after 5 failed attempts
- Response format unchanged (backward compatible)

---

### Step 2.2: Refactor RegisterUser.cs

**Priority:** HIGH
**File to modify:**
- `Features/Auth/V1/RegisterUser.cs`

**Tasks:**
1. Inject IAuthService instead of UserManager
2. Update endpoint handler:
   - Call `await authService.RegisterAsync(request.Email, request.Password, request.FirstName, request.LastName)`
   - Map AuthResult to HTTP response (201 Created or 400 Bad Request)
   - Return existing RegisterUserResponse format
3. Keep existing validator unchanged

**Validation:**
- Registration creates user with Customer role
- Duplicate email returns 409 Conflict
- Weak password returns 400 Bad Request
- Response format unchanged

---

### Step 2.3: Refactor GetCurrentUser.cs

**Priority:** HIGH
**File to modify:**
- `Features/Auth/V1/GetCurrentUser.cs`

**Tasks:**
1. Inject IAuthService instead of UserManager
2. Update endpoint handler:
   - Extract userId from claims (existing code)
   - Call `await authService.GetCurrentUserAsync(userId)`
   - Map AuthResult to HTTP response
   - Return existing CurrentUserResponse format
3. No validator needed (existing behavior)

**Validation:**
- Authenticated user can get their info
- Unauthenticated request returns 401
- Response includes user ID, email, roles

---

## Phase 3: User Management DTOs

**Goal:** Create all DTOs needed for user management endpoints

### Step 3.1: Create User Request DTOs

**Priority:** HIGH
**Files to create:**
- `Models/Users/Requests/CreateUserRequest.cs`
- `Models/Users/Requests/UpdateUserRequest.cs`
- `Models/Users/Requests/UpdateProfileRequest.cs`
- `Models/Users/Requests/ChangePasswordRequest.cs`

**Tasks:**
1. Create `Models/Users/Requests/` folder
2. Implement CreateUserRequest record (email, password, userName, phoneNumber, roles)
3. Implement UpdateUserRequest record (email, userName, phoneNumber, emailConfirmed, lockoutEnabled)
4. Implement UpdateProfileRequest record (userName, phoneNumber)
5. Implement ChangePasswordRequest record (currentPassword, newPassword, confirmNewPassword)

**Validation:** Compile successfully

---

### Step 3.2: Create User Query DTOs

**Priority:** HIGH
**Files to create:**
- `Models/Users/Queries/UserQueryParameters.cs`

**Tasks:**
1. Create `Models/Users/Queries/` folder
2. Implement UserQueryParameters record (page, pageSize, search, role, sortBy, sortOrder)

**Validation:** Compile successfully

---

### Step 3.3: Create User Response DTOs

**Priority:** HIGH
**Files to create:**
- `Models/Users/Responses/UserResponse.cs`
- `Models/Users/Responses/UserDetailResponse.cs`

**Tasks:**
1. Create `Models/Users/Responses/` folder
2. Implement UserResponse record (userId, email, userName, emailConfirmed, lockoutEnd, lockoutEnabled, accessFailedCount, roles)
3. Implement UserDetailResponse record (all UserResponse fields + phoneNumber, phoneNumberConfirmed, twoFactorEnabled)

**Validation:** Compile successfully

---

### Step 3.4: Create User Validators

**Priority:** HIGH
**Files to create:**
- `Validators/Users/CreateUserRequestValidator.cs`
- `Validators/Users/UpdateUserRequestValidator.cs`
- `Validators/Users/UpdateProfileRequestValidator.cs`
- `Validators/Users/ChangePasswordRequestValidator.cs`

**Tasks:**
1. Create `Validators/Users/` folder
2. Implement CreateUserRequestValidator:
   - Email: Required, valid email, max 256 chars
   - Password: Required, min 8 chars, complexity rules
   - UserName: Optional, max 256 chars, alphanumeric
   - PhoneNumber: Optional, valid format
3. Implement UpdateUserRequestValidator:
   - Email: Required, valid email
   - UserName: Required, max 256 chars
4. Implement UpdateProfileRequestValidator:
   - UserName: Required, max 256 chars
5. Implement ChangePasswordRequestValidator:
   - CurrentPassword: Required
   - NewPassword: Required, min 8 chars, complexity
   - ConfirmNewPassword: Must match NewPassword

**Validation:**
- Validators registered in DI (automatic with AddValidatorsFromAssembly)
- Validators trigger before endpoint execution

---

## Phase 4: User Management Endpoints

**Goal:** Implement all user management endpoints

### Step 4.1: Create User List Endpoint (Admin)

**Priority:** HIGH
**File to create:**
- `Features/Users/V1/GetUsers.cs`

**Tasks:**
1. Create `Features/Users/V1/` folder
2. Create GetUsers.cs with:
   - Endpoint: `GET /api/v1/admin/users`
   - Authorization: Admin role required
   - Query binding: UserQueryParameters from query string
   - Handler: Call userService.GetUsersAsync(parameters)
   - Response: PaginatedResponse<UserResponse>
3. Add endpoint mapping to a new UserGroup.cs (similar to AuthGroup.cs)

**Validation:**
- Admin can list users with pagination
- Non-admin gets 403 Forbidden
- Search filters work
- Role filter works
- Sorting works

---

### Step 4.2: Create User Detail Endpoint (Admin)

**Priority:** HIGH
**File to create:**
- `Features/Users/V1/GetUserById.cs`

**Tasks:**
1. Create GetUserById.cs with:
   - Endpoint: `GET /api/v1/admin/users/{userId}`
   - Authorization: Admin role required
   - Handler: Call userService.GetUserByIdAsync(userId)
   - Response: UserDetailResponse or 404 Not Found

**Validation:**
- Admin can get user details
- Returns 404 for non-existent user
- Response includes all user properties

---

### Step 4.3: Create User Creation Endpoint (Admin)

**Priority:** HIGH
**File to create:**
- `Features/Users/V1/CreateUser.cs`

**Tasks:**
1. Create CreateUser.cs with:
   - Endpoint: `POST /api/v1/admin/users`
   - Authorization: Admin role required
   - Request: CreateUserRequest with validation
   - Handler: Call userService.CreateUserAsync(request)
   - Response: 201 Created with UserResponse or 400/409 errors

**Validation:**
- Admin can create users
- Duplicate email returns 409
- Weak password returns 400
- Roles assigned correctly

---

### Step 4.4: Create User Update Endpoint (Admin)

**Priority:** MEDIUM
**File to create:**
- `Features/Users/V1/UpdateUser.cs`

**Tasks:**
1. Create UpdateUser.cs with:
   - Endpoint: `PUT /api/v1/admin/users/{userId}`
   - Authorization: Admin role required
   - Request: UpdateUserRequest with validation
   - Handler: Call userService.UpdateUserAsync(userId, request)
   - Response: 200 OK with UserResponse or 404/409 errors

**Validation:**
- Admin can update users
- Email/username uniqueness validated
- Returns 404 for non-existent user

---

### Step 4.5: Create User Deletion Endpoint (Admin)

**Priority:** MEDIUM
**File to create:**
- `Features/Users/V1/DeleteUser.cs`

**Tasks:**
1. Create DeleteUser.cs with:
   - Endpoint: `DELETE /api/v1/admin/users/{userId}`
   - Authorization: Admin role required
   - Handler: Call userService.DeleteUserAsync(userId)
   - Response: 204 No Content or 404 Not Found

**Validation:**
- Admin can delete users
- Returns 404 for non-existent user
- User data removed from database

---

### Step 4.6: Create User Lock/Unlock Endpoints (Admin)

**Priority:** MEDIUM
**Files to create:**
- `Features/Users/V1/LockUser.cs`
- `Features/Users/V1/UnlockUser.cs`

**Tasks:**
1. Create LockUser.cs:
   - Endpoint: `POST /api/v1/admin/users/{userId}/lock`
   - Authorization: Admin role required
   - Handler: Call userService.LockUserAsync(userId, lockoutEnd)
   - Response: 200 OK or 404 Not Found
2. Create UnlockUser.cs:
   - Endpoint: `POST /api/v1/admin/users/{userId}/unlock`
   - Authorization: Admin role required
   - Handler: Call userService.UnlockUserAsync(userId)
   - Response: 200 OK or 404 Not Found

**Validation:**
- Admin can lock/unlock users
- Locked user cannot login
- Unlocked user can login

---

### Step 4.7: Create User Self-Management Endpoints

**Priority:** MEDIUM
**Files to create:**
- `Features/Users/V1/GetMyProfile.cs`
- `Features/Users/V1/UpdateMyProfile.cs`
- `Features/Users/V1/ChangeMyPassword.cs`

**Tasks:**
1. Create GetMyProfile.cs:
   - Endpoint: `GET /api/v1/users/me`
   - Authorization: Authenticated user
   - Handler: Extract userId from claims, call userService.GetCurrentUserProfileAsync(userId)
   - Response: UserDetailResponse
2. Create UpdateMyProfile.cs:
   - Endpoint: `PUT /api/v1/users/me`
   - Authorization: Authenticated user
   - Request: UpdateProfileRequest with validation
   - Handler: Extract userId, call userService.UpdateCurrentUserProfileAsync(userId, request)
   - Response: 200 OK with UserResponse
3. Create ChangeMyPassword.cs:
   - Endpoint: `PATCH /api/v1/users/me/password`
   - Authorization: Authenticated user
   - Request: ChangePasswordRequest with validation
   - Handler: Extract userId, call userService.ChangePasswordAsync(userId, currentPassword, newPassword)
   - Response: 200 OK or 401 Unauthorized (wrong current password)

**Validation:**
- Authenticated user can view/update own profile
- User can change own password
- Current password validation works

---

### Step 4.8: Create User Role Assignment Endpoints (Admin)

**Priority:** MEDIUM
**Files to create:**
- `Features/Users/V1/GetUserRoles.cs`
- `Features/Users/V1/AssignUserRole.cs`
- `Features/Users/V1/RemoveUserRole.cs`

**Tasks:**
1. Create GetUserRoles.cs:
   - Endpoint: `GET /api/v1/admin/users/{userId}/roles`
   - Authorization: Admin role required
   - Handler: Call userService.GetUserRolesAsync(userId)
   - Response: List of role names
2. Create AssignUserRole.cs:
   - Endpoint: `POST /api/v1/admin/users/{userId}/roles/{roleName}`
   - Authorization: Admin role required
   - Handler: Call userService.AddUserToRoleAsync(userId, roleName)
   - Response: 200 OK or 400/404 errors
3. Create RemoveUserRole.cs:
   - Endpoint: `DELETE /api/v1/admin/users/{userId}/roles/{roleName}`
   - Authorization: Admin role required
   - Handler: Call userService.RemoveUserFromRoleAsync(userId, roleName)
   - Response: 200 OK or 400/404 errors

**Validation:**
- Admin can assign/remove roles
- Cannot remove last role from user
- Role must exist

---

### Step 4.9: Create UserGroup.cs

**Priority:** HIGH
**File to create:**
- `Features/Users/V1/UserGroup.cs`

**Tasks:**
1. Create UserGroup.cs similar to AuthGroup.cs
2. Add MapGroup for `/api/v1/admin/users`
3. Map all admin user endpoints
4. Add MapGroup for `/api/v1/users/me`
5. Map all self-management endpoints
6. Register group in Program.cs: `app.MapUserEndpoints();`

**Validation:**
- All user endpoints accessible
- Routes match API design document

---

## Phase 5: Role Management DTOs & Endpoints

**Goal:** Implement complete role management

### Step 5.1: Create Role DTOs

**Priority:** MEDIUM
**Files to create:**
- `Models/Roles/Requests/CreateRoleRequest.cs`
- `Models/Roles/Requests/UpdateRoleRequest.cs`
- `Models/Roles/Responses/RoleResponse.cs`
- `Models/Roles/Responses/RoleDetailResponse.cs`
- `Validators/Roles/CreateRoleRequestValidator.cs`
- `Validators/Roles/UpdateRoleRequestValidator.cs`

**Tasks:**
1. Create `Models/Roles/` folders (Requests, Responses)
2. Create `Validators/Roles/` folder
3. Implement all DTOs and validators

**Validation:** Compile successfully

---

### Step 5.2: Create Role Management Endpoints

**Priority:** MEDIUM
**Files to create:**
- `Features/Roles/V1/GetRoles.cs`
- `Features/Roles/V1/GetRoleById.cs`
- `Features/Roles/V1/CreateRole.cs`
- `Features/Roles/V1/UpdateRole.cs`
- `Features/Roles/V1/DeleteRole.cs`
- `Features/Roles/V1/GetRoleUsers.cs`
- `Features/Roles/V1/RoleGroup.cs`

**Tasks:**
1. Create `Features/Roles/V1/` folder
2. Implement GetRoles.cs (`GET /api/v1/admin/roles`)
3. Implement GetRoleById.cs (`GET /api/v1/admin/roles/{roleId}`)
4. Implement CreateRole.cs (`POST /api/v1/admin/roles`)
5. Implement UpdateRole.cs (`PUT /api/v1/admin/roles/{roleId}`)
6. Implement DeleteRole.cs (`DELETE /api/v1/admin/roles/{roleId}`)
7. Implement GetRoleUsers.cs (`GET /api/v1/admin/roles/{roleId}/users`)
8. Create RoleGroup.cs to map all endpoints
9. Register in Program.cs: `app.MapRoleEndpoints();`

**Validation:**
- Admin can CRUD roles
- Cannot delete role with users
- Can list users in role with pagination

---

## Phase 6: Documentation & Testing

**Goal:** Update documentation and add comprehensive tests

### Step 6.1: Update OpenAPI Documentation

**Priority:** MEDIUM
**File to modify:**
- `Extensions/OpenApiExtensions.cs`
- Individual endpoint files

**Tasks:**
1. Add XML documentation comments to all endpoints
2. Add request/response examples to OpenAPI
3. Update document info with new endpoint descriptions
4. Test Swagger UI and Scalar documentation

**Validation:**
- Swagger UI shows all endpoints
- Request/response schemas correct
- Examples display properly

---

### Step 6.2: Create Integration Tests

**Priority:** LOW
**Files to create:**
- `OrderFlow.Identity.Tests/` project (if doesn't exist)
- Integration tests for each endpoint category

**Tasks:**
1. Create test project (xUnit)
2. Add WebApplicationFactory for testing
3. Write integration tests for auth endpoints
4. Write integration tests for user management
5. Write integration tests for role management
6. Test authorization (admin vs regular user)

**Validation:**
- All tests pass
- Coverage > 80%

---

## Phase 7: Final Cleanup

**Goal:** Polish and finalize implementation

### Step 7.1: Code Review Checklist

**Priority:** HIGH

**Tasks:**
1. Verify all endpoints match API_ENDPOINTS.md
2. Verify all services match SERVICE_LAYER.md
3. Verify all DTOs match DTOS_STRUCTURE.md
4. Check consistent error handling
5. Check consistent response formats
6. Verify authorization on all protected endpoints
7. Check input validation on all endpoints
8. Review logging statements
9. Check performance (pagination, N+1 queries)

**Validation:** All checklist items completed

---

### Step 7.2: Update README

**Priority:** MEDIUM
**File to modify:**
- `README.md` (root project)

**Tasks:**
1. Update API endpoints section
2. Add authentication instructions
3. Add role management documentation
4. Update examples

**Validation:** README accurate and helpful

---

## Implementation Order Summary

**Recommended sequence:**

1. **Week 1: Foundation**
   - Phase 1: Create service layer (Steps 1.1 - 1.6)
   - Phase 2: Refactor existing endpoints (Steps 2.1 - 2.3)
   - Test: Ensure existing auth endpoints still work

2. **Week 2: User Management**
   - Phase 3: Create user DTOs (Steps 3.1 - 3.4)
   - Phase 4: Implement user endpoints (Steps 4.1 - 4.9)
   - Test: User CRUD, lock/unlock, self-management

3. **Week 3: Role Management**
   - Phase 5: Create role DTOs and endpoints (Steps 5.1 - 5.2)
   - Test: Role CRUD, user assignment

4. **Week 4: Polish**
   - Phase 6: Documentation and testing (Steps 6.1 - 6.2)
   - Phase 7: Final cleanup (Steps 7.1 - 7.2)
   - Test: Full integration testing

---

## Testing Strategy

### Unit Tests
- Test each service method in isolation
- Mock UserManager, RoleManager, SignInManager
- Test success and failure cases
- Test validation logic

### Integration Tests
- Test complete request/response flow
- Use in-memory database
- Test authentication and authorization
- Test pagination and filtering
- Test error scenarios

### Manual Testing
- Use Swagger UI for interactive testing
- Test all endpoints with Postman/HTTP file
- Verify error messages are helpful
- Check response formats match documentation

---

## Rollback Plan

If issues arise during implementation:

1. **Service Layer Issues**: Revert to inline logic in endpoints
2. **Endpoint Issues**: Disable specific endpoints, keep existing ones working
3. **Database Issues**: Shouldn't happen (no schema changes), but can revert migrations
4. **Breaking Changes**: Ensure backward compatibility for V1 existing endpoints

**Key principle:** Never break existing functionality

---

## Success Criteria

**Phase 1 Complete:**
- ✅ Service layer created and registered
- ✅ Existing auth endpoints refactored to use services
- ✅ No breaking changes to V1 API responses
- ✅ All existing tests pass

**Phase 2 Complete:**
- ✅ User management endpoints implemented
- ✅ Admin can CRUD users
- ✅ Users can manage own profile
- ✅ Pagination and filtering work

**Phase 3 Complete:**
- ✅ Role management endpoints implemented
- ✅ Admin can CRUD roles
- ✅ Admin can assign/remove roles from users
- ✅ Role constraints enforced

**Final Complete:**
- ✅ All endpoints documented in Swagger
- ✅ Integration tests pass
- ✅ Code reviewed and cleaned
- ✅ README updated
- ✅ No breaking changes to existing API

---

## Notes

1. **Backward Compatibility**: V1 existing endpoints must remain unchanged in response format
2. **V2 Controllers**: Keep untouched - no changes to V2 during this implementation
3. **Email Features**: Explicitly excluded - no email confirmation, password reset, etc.
4. **Database Schema**: No changes - use existing AspNet Identity tables
5. **Authorization**: All new admin endpoints require Admin role
6. **Validation**: FluentValidation for all request DTOs
7. **Error Handling**: Consistent ErrorResponse format across all endpoints
8. **Pagination**: Default page size 10, max 100
9. **Sorting**: Default sort by email ascending
10. **Admin Protection**: Prevent admin from locking/deleting self
