export interface CategoryResponse {
  id: number;
  name: string;
  description: string | null;
  productCount: number;
}

export interface CreateCategoryRequest {
  name: string;
  description?: string;
}

export interface UpdateCategoryRequest {
  name: string;
  description?: string;
}

export interface ProductResponse {
  id: number;
  name: string;
  description: string | null;
  price: number;
  stock: number;
  isActive: boolean;
  categoryId: number;
  categoryName: string;
}

export interface ProductListResponse {
  id: number;
  name: string;
  price: number;
  stock: number;
  isActive: boolean;
  categoryName: string;
}

export interface CreateProductRequest {
  name: string;
  description?: string;
  price: number;
  stock: number;
  categoryId: number;
}

export interface UpdateProductRequest {
  name: string;
  description?: string;
  price: number;
  stock: number;
  isActive: boolean;
  categoryId: number;
}

export interface ProductQueryParams {
  categoryId?: number;
  isActive?: boolean;
  search?: string;
}
