import { apiClient } from "./client";
import type {
  RoleResponse,
  RoleDetailResponse,
  CreateRoleRequest,
  UpdateRoleRequest,
  PaginationQuery,
} from "@/types/role";
import type { PaginatedResponse, UserResponse } from "@/types/user";

// Admin Role Management API
export const rolesApi = {
  // Get all roles (Admin only)
  async getRoles(): Promise<RoleResponse[]> {
    return apiClient.get<RoleResponse[]>(
      "/api/v1/admin/roles",
      { requiresAuth: true }
    );
  },

  // Get role by ID (Admin only)
  async getRoleById(roleId: string): Promise<RoleDetailResponse> {
    return apiClient.get<RoleDetailResponse>(
      `/api/v1/admin/roles/${roleId}`,
      { requiresAuth: true }
    );
  },

  // Create role (Admin only)
  async createRole(data: CreateRoleRequest): Promise<RoleResponse> {
    return apiClient.post<RoleResponse>(
      "/api/v1/admin/roles",
      data,
      { requiresAuth: true }
    );
  },

  // Update role (Admin only)
  async updateRole(roleId: string, data: UpdateRoleRequest): Promise<RoleResponse> {
    return apiClient.put<RoleResponse>(
      `/api/v1/admin/roles/${roleId}`,
      data,
      { requiresAuth: true }
    );
  },

  // Delete role (Admin only)
  async deleteRole(roleId: string): Promise<{ message: string }> {
    return apiClient.delete<{ message: string }>(
      `/api/v1/admin/roles/${roleId}`,
      { requiresAuth: true }
    );
  },

  // Get users in a role (Admin only)
  async getRoleUsers(
    roleId: string,
    params?: PaginationQuery
  ): Promise<PaginatedResponse<UserResponse>> {
    const queryParams = new URLSearchParams();

    if (params?.page) queryParams.append("page", params.page.toString());
    if (params?.pageSize) queryParams.append("pageSize", params.pageSize.toString());

    const queryString = queryParams.toString();
    const endpoint = `/api/v1/admin/roles/${roleId}/users${queryString ? `?${queryString}` : ""}`;

    return apiClient.get<PaginatedResponse<UserResponse>>(endpoint, { requiresAuth: true });
  },
};
