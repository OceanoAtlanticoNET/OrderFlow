# Identity API - Endpoint Design

**Version:** 1.0
**API Version:** V1 (Minimal APIs)
**Base Path:** `/api/v1`

---

## Overview

This document defines all RESTful endpoints for the OrderFlow Identity API. The API handles authentication, user management, and role management following REST principles.

**Key Principles:**
- Resource-based URLs (nouns, not verbs)
- Proper HTTP methods (GET, POST, PUT, PATCH, DELETE)
- Consistent response formats
- Appropriate status codes
- Pagination for collections
- Role-based authorization

---

## Authentication Endpoints

**Base Path:** `/api/v1/auth`

### POST /auth/login
Authenticate user and return JWT token.

**Request:**
```json
{
  "email": "user@example.com",
  "password": "SecurePass123!"
}
```

**Response:** `200 OK`
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "tokenType": "Bearer",
  "expiresIn": 3600,
  "userId": "user-id-guid",
  "email": "user@example.com",
  "roles": ["Customer"]
}
```

**Errors:**
- `400 Bad Request` - Validation failed
- `401 Unauthorized` - Invalid credentials
- `423 Locked` - Account locked

**Status:** ✅ Existing (will refactor to use service)

---

### POST /auth/register
Register new user account.

**Request:**
```json
{
  "email": "newuser@example.com",
  "password": "SecurePass123!",
  "firstName": "John",
  "lastName": "Doe"
}
```

**Response:** `201 Created`
```json
{
  "userId": "user-id-guid",
  "email": "newuser@example.com",
  "message": "User registered successfully"
}
```

**Errors:**
- `400 Bad Request` - Validation failed
- `409 Conflict` - Email already exists

**Status:** ✅ Existing (will refactor to use service)

---

### GET /auth/me
Get current authenticated user information.

**Headers:**
```
Authorization: Bearer {token}
```

**Response:** `200 OK`
```json
{
  "userId": "user-id-guid",
  "email": "user@example.com",
  "roles": ["Customer", "Admin"]
}
```

**Errors:**
- `401 Unauthorized` - Invalid or missing token

**Status:** ✅ Existing (will refactor to use service)

---

## User Self-Management Endpoints

**Base Path:** `/api/v1/users/me`
**Authorization:** Required (authenticated user)

### GET /users/me
Get current user's full profile.

**Headers:**
```
Authorization: Bearer {token}
```

**Response:** `200 OK`
```json
{
  "userId": "user-id-guid",
  "email": "user@example.com",
  "userName": "user@example.com",
  "emailConfirmed": true,
  "phoneNumber": null,
  "twoFactorEnabled": false,
  "lockoutEnabled": true,
  "accessFailedCount": 0,
  "roles": ["Customer"]
}
```

**Errors:**
- `401 Unauthorized` - Invalid or missing token

**Status:** 🆕 New endpoint

---

### PUT /users/me
Update current user's profile.

**Headers:**
```
Authorization: Bearer {token}
```

**Request:**
```json
{
  "userName": "johndoe",
  "phoneNumber": "+1234567890"
}
```

**Response:** `200 OK`
```json
{
  "userId": "user-id-guid",
  "email": "user@example.com",
  "userName": "johndoe",
  "phoneNumber": "+1234567890",
  "message": "Profile updated successfully"
}
```

**Errors:**
- `400 Bad Request` - Validation failed
- `401 Unauthorized` - Invalid or missing token
- `409 Conflict` - Username already taken

**Status:** 🆕 New endpoint

---

### PATCH /users/me/password
Change current user's password.

**Headers:**
```
Authorization: Bearer {token}
```

**Request:**
```json
{
  "currentPassword": "OldPass123!",
  "newPassword": "NewSecurePass456!",
  "confirmNewPassword": "NewSecurePass456!"
}
```

**Response:** `200 OK`
```json
{
  "message": "Password changed successfully"
}
```

**Errors:**
- `400 Bad Request` - Validation failed (weak password, passwords don't match)
- `401 Unauthorized` - Invalid or missing token / Current password incorrect

**Status:** 🆕 New endpoint

---

## User Administration Endpoints

**Base Path:** `/api/v1/admin/users`
**Authorization:** Required (Admin role)

### GET /admin/users
List all users with pagination and filtering.

**Headers:**
```
Authorization: Bearer {token}
```

**Query Parameters:**
- `page` (int, default: 1) - Page number
- `pageSize` (int, default: 10, max: 100) - Items per page
- `search` (string, optional) - Search in email/username
- `role` (string, optional) - Filter by role name
- `sortBy` (string, default: "email") - Sort field (email, userName, createdAt)
- `sortOrder` (string, default: "asc") - Sort direction (asc, desc)

**Example:**
```
GET /api/v1/admin/users?page=1&pageSize=20&search=john&role=Admin&sortBy=email&sortOrder=asc
```

**Response:** `200 OK`
```json
{
  "data": [
    {
      "userId": "user-id-1",
      "email": "john@example.com",
      "userName": "john@example.com",
      "emailConfirmed": true,
      "lockoutEnd": null,
      "lockoutEnabled": true,
      "accessFailedCount": 0,
      "roles": ["Admin", "Customer"]
    },
    {
      "userId": "user-id-2",
      "email": "jane@example.com",
      "userName": "jane@example.com",
      "emailConfirmed": false,
      "lockoutEnd": null,
      "lockoutEnabled": true,
      "accessFailedCount": 2,
      "roles": ["Customer"]
    }
  ],
  "pagination": {
    "page": 1,
    "pageSize": 20,
    "totalCount": 150,
    "totalPages": 8
  }
}
```

**Errors:**
- `401 Unauthorized` - Invalid or missing token
- `403 Forbidden` - User lacks Admin role

**Status:** 🆕 New endpoint

---

### POST /admin/users
Create new user (admin operation).

**Headers:**
```
Authorization: Bearer {token}
```

**Request:**
```json
{
  "email": "newuser@example.com",
  "password": "TempPass123!",
  "userName": "newuser",
  "roles": ["Customer"]
}
```

**Response:** `201 Created`
```json
{
  "userId": "user-id-guid",
  "email": "newuser@example.com",
  "userName": "newuser",
  "roles": ["Customer"],
  "message": "User created successfully"
}
```

**Errors:**
- `400 Bad Request` - Validation failed
- `401 Unauthorized` - Invalid or missing token
- `403 Forbidden` - User lacks Admin role
- `409 Conflict` - Email or username already exists

**Status:** 🆕 New endpoint

---

### GET /admin/users/{userId}
Get detailed information about a specific user.

**Headers:**
```
Authorization: Bearer {token}
```

**Response:** `200 OK`
```json
{
  "userId": "user-id-guid",
  "email": "user@example.com",
  "userName": "user@example.com",
  "emailConfirmed": true,
  "phoneNumber": "+1234567890",
  "twoFactorEnabled": false,
  "lockoutEnd": null,
  "lockoutEnabled": true,
  "accessFailedCount": 0,
  "roles": ["Customer"]
}
```

**Errors:**
- `401 Unauthorized` - Invalid or missing token
- `403 Forbidden` - User lacks Admin role
- `404 Not Found` - User not found

**Status:** 🆕 New endpoint

---

### PUT /admin/users/{userId}
Update user information (full replacement).

**Headers:**
```
Authorization: Bearer {token}
```

**Request:**
```json
{
  "email": "updated@example.com",
  "userName": "updateduser",
  "phoneNumber": "+1234567890",
  "emailConfirmed": true,
  "lockoutEnabled": true
}
```

**Response:** `200 OK`
```json
{
  "userId": "user-id-guid",
  "email": "updated@example.com",
  "userName": "updateduser",
  "phoneNumber": "+1234567890",
  "message": "User updated successfully"
}
```

**Errors:**
- `400 Bad Request` - Validation failed
- `401 Unauthorized` - Invalid or missing token
- `403 Forbidden` - User lacks Admin role
- `404 Not Found` - User not found
- `409 Conflict` - Email or username already exists

**Status:** 🆕 New endpoint

---

### DELETE /admin/users/{userId}
Delete user account.

**Headers:**
```
Authorization: Bearer {token}
```

**Response:** `204 No Content`

**Errors:**
- `401 Unauthorized` - Invalid or missing token
- `403 Forbidden` - User lacks Admin role
- `404 Not Found` - User not found
- `409 Conflict` - Cannot delete yourself

**Status:** 🆕 New endpoint

---

### POST /admin/users/{userId}/lock
Lock user account (prevent login).

**Headers:**
```
Authorization: Bearer {token}
```

**Request:**
```json
{
  "lockoutEnd": "2025-12-31T23:59:59Z",
  "reason": "Suspicious activity"
}
```

**Response:** `200 OK`
```json
{
  "userId": "user-id-guid",
  "lockoutEnd": "2025-12-31T23:59:59Z",
  "message": "User account locked successfully"
}
```

**Errors:**
- `401 Unauthorized` - Invalid or missing token
- `403 Forbidden` - User lacks Admin role
- `404 Not Found` - User not found
- `409 Conflict` - Cannot lock yourself

**Status:** 🆕 New endpoint

---

### POST /admin/users/{userId}/unlock
Unlock user account.

**Headers:**
```
Authorization: Bearer {token}
```

**Response:** `200 OK`
```json
{
  "userId": "user-id-guid",
  "message": "User account unlocked successfully"
}
```

**Errors:**
- `401 Unauthorized` - Invalid or missing token
- `403 Forbidden` - User lacks Admin role
- `404 Not Found` - User not found

**Status:** 🆕 New endpoint

---

## User Roles Sub-Resource Endpoints

**Base Path:** `/api/v1/admin/users/{userId}/roles`
**Authorization:** Required (Admin role)

### GET /admin/users/{userId}/roles
Get all roles assigned to a user.

**Headers:**
```
Authorization: Bearer {token}
```

**Response:** `200 OK`
```json
{
  "userId": "user-id-guid",
  "roles": ["Admin", "Customer"]
}
```

**Errors:**
- `401 Unauthorized` - Invalid or missing token
- `403 Forbidden` - User lacks Admin role
- `404 Not Found` - User not found

**Status:** 🆕 New endpoint

---

### POST /admin/users/{userId}/roles/{roleName}
Assign role to user.

**Headers:**
```
Authorization: Bearer {token}
```

**Response:** `200 OK`
```json
{
  "userId": "user-id-guid",
  "roleName": "Admin",
  "message": "Role assigned successfully"
}
```

**Errors:**
- `400 Bad Request` - Role already assigned to user
- `401 Unauthorized` - Invalid or missing token
- `403 Forbidden` - User lacks Admin role
- `404 Not Found` - User or role not found

**Status:** 🆕 New endpoint

---

### DELETE /admin/users/{userId}/roles/{roleName}
Remove role from user.

**Headers:**
```
Authorization: Bearer {token}
```

**Response:** `200 OK`
```json
{
  "userId": "user-id-guid",
  "roleName": "Customer",
  "message": "Role removed successfully"
}
```

**Errors:**
- `400 Bad Request` - User doesn't have this role / Cannot remove last role
- `401 Unauthorized` - Invalid or missing token
- `403 Forbidden` - User lacks Admin role
- `404 Not Found` - User or role not found

**Status:** 🆕 New endpoint

---

## Role Management Endpoints

**Base Path:** `/api/v1/admin/roles`
**Authorization:** Required (Admin role)

### GET /admin/roles
List all roles.

**Headers:**
```
Authorization: Bearer {token}
```

**Response:** `200 OK`
```json
{
  "data": [
    {
      "roleId": "role-id-1",
      "roleName": "Admin",
      "normalizedName": "ADMIN",
      "userCount": 5
    },
    {
      "roleId": "role-id-2",
      "roleName": "Customer",
      "normalizedName": "CUSTOMER",
      "userCount": 150
    }
  ]
}
```

**Errors:**
- `401 Unauthorized` - Invalid or missing token
- `403 Forbidden` - User lacks Admin role

**Status:** 🆕 New endpoint

---

### POST /admin/roles
Create new role.

**Headers:**
```
Authorization: Bearer {token}
```

**Request:**
```json
{
  "roleName": "Manager"
}
```

**Response:** `201 Created`
```json
{
  "roleId": "role-id-guid",
  "roleName": "Manager",
  "message": "Role created successfully"
}
```

**Errors:**
- `400 Bad Request` - Validation failed (invalid name format)
- `401 Unauthorized` - Invalid or missing token
- `403 Forbidden` - User lacks Admin role
- `409 Conflict` - Role already exists

**Status:** 🆕 New endpoint

---

### GET /admin/roles/{roleId}
Get role details.

**Headers:**
```
Authorization: Bearer {token}
```

**Response:** `200 OK`
```json
{
  "roleId": "role-id-guid",
  "roleName": "Manager",
  "normalizedName": "MANAGER",
  "userCount": 12
}
```

**Errors:**
- `401 Unauthorized` - Invalid or missing token
- `403 Forbidden` - User lacks Admin role
- `404 Not Found` - Role not found

**Status:** 🆕 New endpoint

---

### PUT /admin/roles/{roleId}
Update role name.

**Headers:**
```
Authorization: Bearer {token}
```

**Request:**
```json
{
  "roleName": "SeniorManager"
}
```

**Response:** `200 OK`
```json
{
  "roleId": "role-id-guid",
  "roleName": "SeniorManager",
  "message": "Role updated successfully"
}
```

**Errors:**
- `400 Bad Request` - Validation failed
- `401 Unauthorized` - Invalid or missing token
- `403 Forbidden` - User lacks Admin role
- `404 Not Found` - Role not found
- `409 Conflict` - Role name already exists

**Status:** 🆕 New endpoint

---

### DELETE /admin/roles/{roleId}
Delete role.

**Headers:**
```
Authorization: Bearer {token}
```

**Response:** `204 No Content`

**Errors:**
- `401 Unauthorized` - Invalid or missing token
- `403 Forbidden` - User lacks Admin role
- `404 Not Found` - Role not found
- `409 Conflict` - Role is assigned to users (cannot delete)

**Status:** 🆕 New endpoint

---

### GET /admin/roles/{roleId}/users
List all users with this role.

**Headers:**
```
Authorization: Bearer {token}
```

**Query Parameters:**
- `page` (int, default: 1)
- `pageSize` (int, default: 10, max: 100)

**Response:** `200 OK`
```json
{
  "roleId": "role-id-guid",
  "roleName": "Admin",
  "data": [
    {
      "userId": "user-id-1",
      "email": "admin1@example.com",
      "userName": "admin1@example.com"
    },
    {
      "userId": "user-id-2",
      "email": "admin2@example.com",
      "userName": "admin2@example.com"
    }
  ],
  "pagination": {
    "page": 1,
    "pageSize": 10,
    "totalCount": 5,
    "totalPages": 1
  }
}
```

**Errors:**
- `401 Unauthorized` - Invalid or missing token
- `403 Forbidden` - User lacks Admin role
- `404 Not Found` - Role not found

**Status:** 🆕 New endpoint

---

## HTTP Status Codes Summary

| Code | Meaning | Usage |
|------|---------|-------|
| 200 | OK | Successful GET, PUT, PATCH, or action |
| 201 | Created | Successful POST (resource created) |
| 204 | No Content | Successful DELETE |
| 400 | Bad Request | Validation failed, malformed request |
| 401 | Unauthorized | Missing or invalid authentication token |
| 403 | Forbidden | Authenticated but lacks required role/permission |
| 404 | Not Found | Resource doesn't exist |
| 409 | Conflict | Resource already exists or state conflict |
| 423 | Locked | Account is locked |
| 500 | Internal Server Error | Unexpected server error |

---

## Response Format Standards

### Success Response (with data)
```json
{
  "data": { /* resource or array */ },
  "message": "Operation successful"
}
```

### Success Response (no data)
```json
{
  "message": "Operation successful"
}
```

### Paginated Response
```json
{
  "data": [ /* array of resources */ ],
  "pagination": {
    "page": 1,
    "pageSize": 10,
    "totalCount": 100,
    "totalPages": 10
  }
}
```

### Error Response
```json
{
  "errors": ["Error message 1", "Error message 2"],
  "message": "Operation failed",
  "statusCode": 400
}
```

---

## Summary Statistics

| Category | Existing | New | Total |
|----------|----------|-----|-------|
| **Authentication** | 3 | 0 | 3 |
| **User Self-Management** | 0 | 3 | 3 |
| **User Admin** | 0 | 7 | 7 |
| **User Roles** | 0 | 3 | 3 |
| **Role Management** | 0 | 6 | 6 |
| **TOTAL** | 3 | 19 | 22 |

---

## Notes

1. **Email features excluded**: No email confirmation, password reset, or email verification endpoints
2. **Database schema**: No changes to existing Identity tables
3. **V2 Controllers**: Remain untouched, all new endpoints in V1 Minimal APIs
4. **Pagination**: Default page size 10, max 100 items per page
5. **Search**: Case-insensitive search in email and username fields
6. **Sorting**: Default sort by email ascending
7. **Admin protection**: Cannot lock, delete, or modify own admin account
8. **Role constraints**: Users must have at least one role at all times
