import { useState } from "react";
import { rolesApi } from "@/services/api/roles";
import type {
  RoleResponse,
  RoleDetailResponse,
  CreateRoleRequest,
  UpdateRoleRequest,
  PaginationQuery,
} from "@/types/role";
import type { PaginatedResponse, UserResponse } from "@/types/user";

export function useRoles() {
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const getRoles = async (): Promise<RoleResponse[] | null> => {
    setIsLoading(true);
    setError(null);
    try {
      const response = await rolesApi.getRoles();
      return response;
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to fetch roles");
      return null;
    } finally {
      setIsLoading(false);
    }
  };

  const getRoleById = async (roleId: string): Promise<RoleDetailResponse | null> => {
    setIsLoading(true);
    setError(null);
    try {
      const response = await rolesApi.getRoleById(roleId);
      return response;
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to fetch role");
      return null;
    } finally {
      setIsLoading(false);
    }
  };

  const createRole = async (data: CreateRoleRequest): Promise<RoleResponse | null> => {
    setIsLoading(true);
    setError(null);
    try {
      const response = await rolesApi.createRole(data);
      return response;
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to create role");
      throw err;
    } finally {
      setIsLoading(false);
    }
  };

  const updateRole = async (roleId: string, data: UpdateRoleRequest): Promise<RoleResponse | null> => {
    setIsLoading(true);
    setError(null);
    try {
      const response = await rolesApi.updateRole(roleId, data);
      return response;
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to update role");
      throw err;
    } finally {
      setIsLoading(false);
    }
  };

  const deleteRole = async (roleId: string): Promise<boolean> => {
    setIsLoading(true);
    setError(null);
    try {
      await rolesApi.deleteRole(roleId);
      return true;
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to delete role");
      throw err;
    } finally {
      setIsLoading(false);
    }
  };

  const getRoleUsers = async (
    roleId: string,
    params?: PaginationQuery
  ): Promise<PaginatedResponse<UserResponse> | null> => {
    setIsLoading(true);
    setError(null);
    try {
      const response = await rolesApi.getRoleUsers(roleId, params);
      return response;
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to fetch role users");
      return null;
    } finally {
      setIsLoading(false);
    }
  };

  return {
    isLoading,
    error,
    getRoles,
    getRoleById,
    createRole,
    updateRole,
    deleteRole,
    getRoleUsers,
  };
}
