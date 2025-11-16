// Role types matching the Identity API DTOs

export interface RoleResponse {
  roleId: string;
  roleName: string;
  normalizedName: string;
  userCount: number;
}

export interface RoleDetailResponse extends RoleResponse {
  // Same as RoleResponse for now
}

export interface CreateRoleRequest {
  roleName: string;
}

export interface UpdateRoleRequest {
  roleName: string;
}

export interface PaginationQuery {
  page?: number;
  pageSize?: number;
}
