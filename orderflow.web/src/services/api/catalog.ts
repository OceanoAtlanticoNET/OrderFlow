import { apiClient } from './client';
import type {
  CategoryResponse,
  CreateCategoryRequest,
  UpdateCategoryRequest,
  ProductResponse,
  ProductListResponse,
  CreateProductRequest,
  UpdateProductRequest,
  ProductQueryParams,
} from '../../types/catalog';

// Categories API
export const categoriesApi = {
  getAll: () => apiClient.get<CategoryResponse[]>('/api/v1/categories'),

  getById: (id: number) => apiClient.get<CategoryResponse>(`/api/v1/categories/${id}`),

  create: (data: CreateCategoryRequest) =>
    apiClient.post<CategoryResponse>('/api/v1/categories', data, { requiresAuth: true }),

  update: (id: number, data: UpdateCategoryRequest) =>
    apiClient.put<CategoryResponse>(`/api/v1/categories/${id}`, data, { requiresAuth: true }),

  delete: (id: number) =>
    apiClient.delete(`/api/v1/categories/${id}`, { requiresAuth: true }),
};

// Products API
export const productsApi = {
  getAll: (params?: ProductQueryParams) => {
    const searchParams = new URLSearchParams();
    if (params?.categoryId) searchParams.append('categoryId', params.categoryId.toString());
    if (params?.isActive !== undefined) searchParams.append('isActive', params.isActive.toString());
    if (params?.search) searchParams.append('search', params.search);

    const queryString = searchParams.toString();
    const endpoint = `/api/v1/products${queryString ? `?${queryString}` : ''}`;

    return apiClient.get<ProductListResponse[]>(endpoint);
  },

  getById: (id: number) => apiClient.get<ProductResponse>(`/api/v1/products/${id}`),

  create: (data: CreateProductRequest) =>
    apiClient.post<ProductResponse>('/api/v1/products', data, { requiresAuth: true }),

  update: (id: number, data: UpdateProductRequest) =>
    apiClient.put<ProductResponse>(`/api/v1/products/${id}`, data, { requiresAuth: true }),

  updateStock: (id: number, quantity: number) =>
    apiClient.patch(`/api/v1/products/${id}/stock`, { quantity }, { requiresAuth: true }),

  delete: (id: number) =>
    apiClient.delete(`/api/v1/products/${id}`, { requiresAuth: true }),
};
