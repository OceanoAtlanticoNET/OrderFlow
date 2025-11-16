import { useState } from "react";
import { usersApi } from "@/services/api/users";
import type {
  UserResponse,
  UserDetailResponse,
  CreateUserRequest,
  UpdateUserRequest,
  UserQueryParameters,
  PaginatedResponse,
} from "@/types/user";

export function useUsers() {
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const getUsers = async (params?: UserQueryParameters): Promise<PaginatedResponse<UserResponse> | null> => {
    setIsLoading(true);
    setError(null);
    try {
      const response = await usersApi.getUsers(params);
      return response;
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to fetch users");
      return null;
    } finally {
      setIsLoading(false);
    }
  };

  const getUserById = async (userId: string): Promise<UserDetailResponse | null> => {
    setIsLoading(true);
    setError(null);
    try {
      const response = await usersApi.getUserById(userId);
      return response;
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to fetch user");
      return null;
    } finally {
      setIsLoading(false);
    }
  };

  const createUser = async (data: CreateUserRequest): Promise<UserResponse | null> => {
    setIsLoading(true);
    setError(null);
    try {
      const response = await usersApi.createUser(data);
      return response;
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to create user");
      throw err;
    } finally {
      setIsLoading(false);
    }
  };

  const updateUser = async (userId: string, data: UpdateUserRequest): Promise<UserResponse | null> => {
    setIsLoading(true);
    setError(null);
    try {
      const response = await usersApi.updateUser(userId, data);
      return response;
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to update user");
      throw err;
    } finally {
      setIsLoading(false);
    }
  };

  const deleteUser = async (userId: string): Promise<boolean> => {
    setIsLoading(true);
    setError(null);
    try {
      await usersApi.deleteUser(userId);
      return true;
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to delete user");
      throw err;
    } finally {
      setIsLoading(false);
    }
  };

  const lockUser = async (userId: string, lockoutEnd?: string, reason?: string): Promise<string | null> => {
    setIsLoading(true);
    setError(null);
    try {
      const response = await usersApi.lockUser(userId, lockoutEnd, reason);
      return response.message;
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to lock user");
      throw err;
    } finally {
      setIsLoading(false);
    }
  };

  const unlockUser = async (userId: string): Promise<string | null> => {
    setIsLoading(true);
    setError(null);
    try {
      const response = await usersApi.unlockUser(userId);
      return response.message;
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to unlock user");
      throw err;
    } finally {
      setIsLoading(false);
    }
  };

  const getUserRoles = async (userId: string): Promise<string[] | null> => {
    setIsLoading(true);
    setError(null);
    try {
      const response = await usersApi.getUserRoles(userId);
      return response.roles;
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to fetch user roles");
      return null;
    } finally {
      setIsLoading(false);
    }
  };

  const assignUserRole = async (userId: string, roleName: string): Promise<string | null> => {
    setIsLoading(true);
    setError(null);
    try {
      const response = await usersApi.assignUserRole(userId, roleName);
      return response.message;
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to assign role");
      throw err;
    } finally {
      setIsLoading(false);
    }
  };

  const removeUserRole = async (userId: string, roleName: string): Promise<string | null> => {
    setIsLoading(true);
    setError(null);
    try {
      const response = await usersApi.removeUserRole(userId, roleName);
      return response.message;
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to remove role");
      throw err;
    } finally {
      setIsLoading(false);
    }
  };

  return {
    isLoading,
    error,
    getUsers,
    getUserById,
    createUser,
    updateUser,
    deleteUser,
    lockUser,
    unlockUser,
    getUserRoles,
    assignUserRole,
    removeUserRole,
  };
}
