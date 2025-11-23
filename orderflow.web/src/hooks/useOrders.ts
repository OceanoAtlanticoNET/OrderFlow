import { useState } from 'react';
import { ordersApi, adminOrdersApi } from '../services/api/orders';
import type {
  OrderResponse,
  OrderListResponse,
  CreateOrderRequest,
  UpdateOrderStatusRequest,
  OrderQueryParams,
} from '../types/order';

export function useOrders() {
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const getMyOrders = async (): Promise<OrderListResponse[]> => {
    setIsLoading(true);
    setError(null);
    try {
      return await ordersApi.getMyOrders();
    } finally {
      setIsLoading(false);
    }
  };

  const getOrderById = async (id: number): Promise<OrderResponse> => {
    setIsLoading(true);
    setError(null);
    try {
      return await ordersApi.getById(id);
    } finally {
      setIsLoading(false);
    }
  };

  const createOrder = async (data: CreateOrderRequest): Promise<OrderResponse> => {
    setIsLoading(true);
    setError(null);
    try {
      return await ordersApi.create(data);
    } finally {
      setIsLoading(false);
    }
  };

  const cancelOrder = async (id: number): Promise<void> => {
    setIsLoading(true);
    setError(null);
    try {
      await ordersApi.cancel(id);
    } finally {
      setIsLoading(false);
    }
  };

  return {
    isLoading,
    error,
    getMyOrders,
    getOrderById,
    createOrder,
    cancelOrder,
  };
}

export function useAdminOrders() {
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const getAllOrders = async (params?: OrderQueryParams): Promise<OrderListResponse[]> => {
    setIsLoading(true);
    setError(null);
    try {
      return await adminOrdersApi.getAll(params);
    } finally {
      setIsLoading(false);
    }
  };

  const getOrderById = async (id: number): Promise<OrderResponse> => {
    setIsLoading(true);
    setError(null);
    try {
      return await adminOrdersApi.getById(id);
    } finally {
      setIsLoading(false);
    }
  };

  const updateOrderStatus = async (id: number, data: UpdateOrderStatusRequest): Promise<void> => {
    setIsLoading(true);
    setError(null);
    try {
      await adminOrdersApi.updateStatus(id, data);
    } finally {
      setIsLoading(false);
    }
  };

  return {
    isLoading,
    error,
    getAllOrders,
    getOrderById,
    updateOrderStatus,
  };
}
