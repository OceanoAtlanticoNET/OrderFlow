import { useState, useEffect } from "react";
import { authApi, tokenStorage, ApiError } from "@/features/auth/api/authApi";
import type { LoginRequest, RegisterRequest, User } from "@/types/auth";

interface AuthState {
  user: User | null;
  token: string | null;
  isLoading: boolean;
  isAuthenticated: boolean;
}

export function useAuth() {
  const [authState, setAuthState] = useState<AuthState>({
    user: null,
    token: tokenStorage.getToken(),
    isLoading: true,
    isAuthenticated: false,
  });

  useEffect(() => {
    const initAuth = async () => {
      const token = tokenStorage.getToken();
      if (token) {
        try {
          const user = await authApi.getCurrentUser(token);
          setAuthState({
            user,
            token,
            isLoading: false,
            isAuthenticated: true,
          });
        } catch {
          tokenStorage.removeToken();
          setAuthState({
            user: null,
            token: null,
            isLoading: false,
            isAuthenticated: false,
          });
        }
      } else {
        setAuthState((prev) => ({ ...prev, isLoading: false }));
      }
    };

    initAuth();
  }, []);

  const login = async (data: LoginRequest) => {
    const response = await authApi.login(data);
    tokenStorage.setToken(response.accessToken);
    
    const user: User = {
      userId: response.userId,
      email: response.email,
      roles: response.roles,
    };

    setAuthState({
      user,
      token: response.accessToken,
      isLoading: false,
      isAuthenticated: true,
    });

    return response;
  };

  const register = async (data: RegisterRequest) => {
    return await authApi.register(data);
  };

  const logout = () => {
    tokenStorage.removeToken();
    setAuthState({
      user: null,
      token: null,
      isLoading: false,
      isAuthenticated: false,
    });
  };

  return {
    ...authState,
    login,
    register,
    logout,
  };
}

export { ApiError };
