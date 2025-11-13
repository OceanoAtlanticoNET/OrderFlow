import type {
  RegisterRequest,
  RegisterResponse,
  LoginRequest,
  LoginResponse,
  ErrorResponse,
  User,
} from "@/types/auth";

// Aspire injects VITE_IDENTITY_URL via WithEnvironment in AppHost
const API_BASE_URL = import.meta.env.VITE_IDENTITY_URL;

// Using V1 Minimal API endpoints
const API_VERSION = "v1";

console.log('API_BASE_URL:', API_BASE_URL);

class ApiError extends Error {
  errors: string[];
  
  constructor(errors: string[]) {
    super(errors.join(", "));
    this.name = "ApiError";
    this.errors = errors;
  }
}

async function handleResponse<T>(response: Response): Promise<T> {
  if (!response.ok) {
    if (response.status === 400 || response.status === 401) {
      const errorData: ErrorResponse = await response.json();
      throw new ApiError(errorData.errors);
    }
    throw new ApiError([`HTTP Error: ${response.status}`]);
  }
  return response.json();
}

export const authApi = {
  async register(data: RegisterRequest): Promise<RegisterResponse> {
    const response = await fetch(`${API_BASE_URL}/api/${API_VERSION}/auth/register`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(data),
    });
    return handleResponse<RegisterResponse>(response);
  },

  async login(data: LoginRequest): Promise<LoginResponse> {
    const response = await fetch(`${API_BASE_URL}/api/${API_VERSION}/auth/login`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(data),
    });
    return handleResponse<LoginResponse>(response);
  },

  async getCurrentUser(token: string): Promise<User> {
    const response = await fetch(`${API_BASE_URL}/api/${API_VERSION}/auth/me`, {
      headers: {
        Authorization: `Bearer ${token}`,
      },
    });
    return handleResponse<User>(response);
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

export { ApiError };
