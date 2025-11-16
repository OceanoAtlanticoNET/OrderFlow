import { apiClient } from "./client";
import type {
  RegisterRequest,
  RegisterResponse,
  LoginRequest,
  LoginResponse,
  User,
} from "@/types/auth";

export const authApi = {
  async register(data: RegisterRequest): Promise<RegisterResponse> {
    return apiClient.post<RegisterResponse>("/api/v1/auth/register", data);
  },

  async login(data: LoginRequest): Promise<LoginResponse> {
    return apiClient.post<LoginResponse>("/api/v1/auth/login", data);
  },

  async getCurrentUser(): Promise<User> {
    return apiClient.get<User>("/api/v1/auth/me", { requiresAuth: true });
  },
};

// Token storage utilities
export const tokenStorage = {
  getToken(): string | null {
    return localStorage.getItem("token");
  },

  setToken(token: string): void {
    localStorage.setItem("token", token);
  },

  removeToken(): void {
    localStorage.removeItem("token");
  },
};
