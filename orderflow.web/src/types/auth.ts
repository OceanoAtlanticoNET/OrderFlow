// Auth Request/Response Types matching C# DTOs

export interface RegisterRequest {
  email: string;
  password: string;
}

export interface RegisterResponse {
  userId: string;
  email: string;
  message: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  accessToken: string;
  tokenType: string;
  expiresIn: number;
  userId: string;
  email: string;
  roles: string[];
}

export interface ErrorResponse {
  errors: string[];
}

export interface User {
  userId: string;
  email: string;
  roles: string[];
}
