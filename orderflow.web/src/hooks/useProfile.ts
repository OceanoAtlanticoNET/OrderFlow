import { useState } from "react";
import { profileApi } from "@/services/api/users";
import type {
  UserDetailResponse,
  UpdateProfileRequest,
  ChangePasswordRequest,
  UserResponse,
} from "@/types/user";

export function useProfile() {
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const getMyProfile = async (): Promise<UserDetailResponse | null> => {
    setIsLoading(true);
    setError(null);
    try {
      const response = await profileApi.getMyProfile();
      return response;
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to fetch profile");
      return null;
    } finally {
      setIsLoading(false);
    }
  };

  const updateMyProfile = async (data: UpdateProfileRequest): Promise<UserResponse | null> => {
    setIsLoading(true);
    setError(null);
    try {
      const response = await profileApi.updateMyProfile(data);
      return response;
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to update profile");
      throw err;
    } finally {
      setIsLoading(false);
    }
  };

  const changeMyPassword = async (data: ChangePasswordRequest): Promise<string | null> => {
    setIsLoading(true);
    setError(null);
    try {
      const response = await profileApi.changeMyPassword(data);
      return response.message;
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to change password");
      throw err;
    } finally {
      setIsLoading(false);
    }
  };

  return {
    isLoading,
    error,
    getMyProfile,
    updateMyProfile,
    changeMyPassword,
  };
}
