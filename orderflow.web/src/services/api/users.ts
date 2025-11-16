import { apiClient } from "./client";
import type {
  UserResponse,
  UserDetailResponse,
  CreateUserRequest,
  UpdateUserRequest,
  UpdateProfileRequest,
  ChangePasswordRequest,
  UserQueryParameters,
  PaginatedResponse,
} from "@/types/user";

// Admin User Management API
export const usersApi = {
  // Get all users (Admin only)
  async getUsers(params?: UserQueryParameters): Promise<PaginatedResponse<UserResponse>> {
    const queryParams = new URLSearchParams();

    if (params?.page) queryParams.append("page", params.page.toString());
    if (params?.pageSize) queryParams.append("pageSize", params.pageSize.toString());
    if (params?.search) queryParams.append("search", params.search);
    if (params?.role) queryParams.append("role", params.role);
    if (params?.sortBy) queryParams.append("sortBy", params.sortBy);
    if (params?.sortDescending !== undefined) {
      queryParams.append("sortDescending", params.sortDescending.toString());
    }

    const queryString = queryParams.toString();
    const endpoint = `/api/v1/admin/users${queryString ? `?${queryString}` : ""}`;

    return apiClient.get<PaginatedResponse<UserResponse>>(endpoint, { requiresAuth: true });
  },

  // Get user by ID (Admin only)
  async getUserById(userId: string): Promise<UserDetailResponse> {
    return apiClient.get<UserDetailResponse>(
      `/api/v1/admin/users/${userId}`,
      { requiresAuth: true }
    );
  },

  // Create user (Admin only)
  async createUser(data: CreateUserRequest): Promise<UserResponse> {
    return apiClient.post<UserResponse>(
      "/api/v1/admin/users",
      data,
      { requiresAuth: true }
    );
  },

  // Update user (Admin only)
  async updateUser(userId: string, data: UpdateUserRequest): Promise<UserResponse> {
    return apiClient.put<UserResponse>(
      `/api/v1/admin/users/${userId}`,
      data,
      { requiresAuth: true }
    );
  },

  // Delete user (Admin only)
  async deleteUser(userId: string): Promise<void> {
    return apiClient.delete<void>(
      `/api/v1/admin/users/${userId}`,
      { requiresAuth: true }
    );
  },

  // Lock user (Admin only)
  async lockUser(userId: string, lockoutEnd?: string, reason?: string): Promise<{ message: string }> {
    return apiClient.post<{ message: string }>(
      `/api/v1/admin/users/${userId}/lock`,
      { lockoutEnd, reason },
      { requiresAuth: true }
    );
  },

  // Unlock user (Admin only)
  async unlockUser(userId: string): Promise<{ message: string }> {
    return apiClient.post<{ message: string }>(
      `/api/v1/admin/users/${userId}/unlock`,
      undefined,
      { requiresAuth: true }
    );
  },

  // Get user roles (Admin only)
  async getUserRoles(userId: string): Promise<{ userId: string; roles: string[] }> {
    return apiClient.get<{ userId: string; roles: string[] }>(
      `/api/v1/admin/users/${userId}/roles`,
      { requiresAuth: true }
    );
  },

  // Assign role to user (Admin only)
  async assignUserRole(
    userId: string,
    roleName: string
  ): Promise<{ userId: string; roleName: string; message: string }> {
    return apiClient.post<{ userId: string; roleName: string; message: string }>(
      `/api/v1/admin/users/${userId}/roles/${roleName}`,
      undefined,
      { requiresAuth: true }
    );
  },

  // Remove role from user (Admin only)
  async removeUserRole(
    userId: string,
    roleName: string
  ): Promise<{ userId: string; roleName: string; message: string }> {
    return apiClient.delete<{ userId: string; roleName: string; message: string }>(
      `/api/v1/admin/users/${userId}/roles/${roleName}`,
      { requiresAuth: true }
    );
  },
};

// User Self-Management API
export const profileApi = {
  // Get own profile
  async getMyProfile(): Promise<UserDetailResponse> {
    return apiClient.get<UserDetailResponse>(
      "/api/v1/users/me",
      { requiresAuth: true }
    );
  },

  // Update own profile
  async updateMyProfile(data: UpdateProfileRequest): Promise<UserResponse> {
    return apiClient.put<UserResponse>(
      "/api/v1/users/me",
      data,
      { requiresAuth: true }
    );
  },

  // Change own password
  async changeMyPassword(data: ChangePasswordRequest): Promise<{ message: string }> {
    return apiClient.patch<{ message: string }>(
      "/api/v1/users/me/password",
      data,
      { requiresAuth: true }
    );
  },
};
