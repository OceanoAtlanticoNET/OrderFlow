export type OrderStatus =
  | 'Pending'
  | 'Confirmed'
  | 'Processing'
  | 'Shipped'
  | 'Delivered'
  | 'Cancelled';

export interface OrderItemResponse {
  id: number;
  productId: number;
  productName: string;
  unitPrice: number;
  quantity: number;
  subtotal: number;
}

export interface OrderResponse {
  id: number;
  userId: string;
  status: OrderStatus;
  totalAmount: number;
  shippingAddress: string | null;
  notes: string | null;
  createdAt: string;
  updatedAt: string | null;
  items: OrderItemResponse[];
}

export interface OrderListResponse {
  id: number;
  status: OrderStatus;
  totalAmount: number;
  itemCount: number;
  createdAt: string;
}

export interface CreateOrderItemRequest {
  productId: number;
  quantity: number;
}

export interface CreateOrderRequest {
  shippingAddress?: string;
  notes?: string;
  items: CreateOrderItemRequest[];
}

export interface UpdateOrderStatusRequest {
  status: OrderStatus;
}

export interface OrderQueryParams {
  status?: OrderStatus;
  userId?: string;
}
