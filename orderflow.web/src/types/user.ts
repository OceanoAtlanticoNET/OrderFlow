// User types matching the Identity API DTOs

export interface UserResponse {
  userId: string;
  email: string;
  userName: string;
  emailConfirmed: boolean;
  lockoutEnd: string | null;
  lockoutEnabled: boolean;
  accessFailedCount: number;
  roles: string[];
}

export interface UserDetailResponse extends UserResponse {
  phoneNumber?: string;
  phoneNumberConfirmed: boolean;
  twoFactorEnabled: boolean;
}

export interface CreateUserRequest {
  email: string;
  password: string;
  userName?: string;
  phoneNumber?: string;
  roles?: string[];
}

export interface UpdateUserRequest {
  email: string;
  userName: string;
  phoneNumber?: string;
  emailConfirmed: boolean;
  lockoutEnabled: boolean;
}

export interface UpdateProfileRequest {
  userName: string;
  phoneNumber?: string;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
  confirmNewPassword: string;
}

export interface UserQueryParameters {
  page?: number;
  pageSize?: number;
  search?: string;
  role?: string;
  sortBy?: string;
  sortDescending?: boolean;
}

export interface PaginationMetadata {
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

export interface PaginatedResponse<T> {
  data: T[];
  pagination: PaginationMetadata;
}
