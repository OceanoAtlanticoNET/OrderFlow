import { apiClient } from './client';
import type {
  OrderResponse,
  OrderListResponse,
  CreateOrderRequest,
  UpdateOrderStatusRequest,
  OrderQueryParams,
} from '../../types/order';

// User Orders API
export const ordersApi = {
  // Get current user's orders
  getMyOrders: () =>
    apiClient.get<OrderListResponse[]>('/api/v1/orders', { requiresAuth: true }),

  // Get order details (user can only see their own)
  getById: (id: number) =>
    apiClient.get<OrderResponse>(`/api/v1/orders/${id}`, { requiresAuth: true }),

  // Create a new order
  create: (data: CreateOrderRequest) =>
    apiClient.post<OrderResponse>('/api/v1/orders', data, { requiresAuth: true }),

  // Cancel an order
  cancel: (id: number) =>
    apiClient.put(`/api/v1/orders/${id}/cancel`, {}, { requiresAuth: true }),
};

// Admin Orders API
export const adminOrdersApi = {
  // Get all orders (admin only)
  getAll: (params?: OrderQueryParams) => {
    const searchParams = new URLSearchParams();
    if (params?.status) searchParams.append('status', params.status);
    if (params?.userId) searchParams.append('userId', params.userId);

    const queryString = searchParams.toString();
    const endpoint = `/api/v1/admin/orders${queryString ? `?${queryString}` : ''}`;

    return apiClient.get<OrderListResponse[]>(endpoint, { requiresAuth: true });
  },

  // Get order details (admin)
  getById: (id: number) =>
    apiClient.get<OrderResponse>(`/api/v1/admin/orders/${id}`, { requiresAuth: true }),

  // Update order status
  updateStatus: (id: number, data: UpdateOrderStatusRequest) =>
    apiClient.put(`/api/v1/admin/orders/${id}/status`, data, { requiresAuth: true }),
};
