import { useState } from 'react';
import { categoriesApi, productsApi } from '../services/api/catalog';
import type {
  CategoryResponse,
  CreateCategoryRequest,
  UpdateCategoryRequest,
  ProductResponse,
  ProductListResponse,
  CreateProductRequest,
  UpdateProductRequest,
  ProductQueryParams,
} from '../types/catalog';

export function useCategories() {
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const getCategories = async (): Promise<CategoryResponse[]> => {
    setIsLoading(true);
    setError(null);
    try {
      return await categoriesApi.getAll();
    } finally {
      setIsLoading(false);
    }
  };

  const getCategoryById = async (id: number): Promise<CategoryResponse> => {
    setIsLoading(true);
    setError(null);
    try {
      return await categoriesApi.getById(id);
    } finally {
      setIsLoading(false);
    }
  };

  const createCategory = async (data: CreateCategoryRequest): Promise<CategoryResponse> => {
    setIsLoading(true);
    setError(null);
    try {
      return await categoriesApi.create(data);
    } finally {
      setIsLoading(false);
    }
  };

  const updateCategory = async (id: number, data: UpdateCategoryRequest): Promise<CategoryResponse> => {
    setIsLoading(true);
    setError(null);
    try {
      return await categoriesApi.update(id, data);
    } finally {
      setIsLoading(false);
    }
  };

  const deleteCategory = async (id: number): Promise<void> => {
    setIsLoading(true);
    setError(null);
    try {
      await categoriesApi.delete(id);
    } finally {
      setIsLoading(false);
    }
  };

  return {
    isLoading,
    error,
    getCategories,
    getCategoryById,
    createCategory,
    updateCategory,
    deleteCategory,
  };
}

export function useProducts() {
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const getProducts = async (params?: ProductQueryParams): Promise<ProductListResponse[]> => {
    setIsLoading(true);
    setError(null);
    try {
      return await productsApi.getAll(params);
    } finally {
      setIsLoading(false);
    }
  };

  const getProductById = async (id: number): Promise<ProductResponse> => {
    setIsLoading(true);
    setError(null);
    try {
      return await productsApi.getById(id);
    } finally {
      setIsLoading(false);
    }
  };

  const createProduct = async (data: CreateProductRequest): Promise<ProductResponse> => {
    setIsLoading(true);
    setError(null);
    try {
      return await productsApi.create(data);
    } finally {
      setIsLoading(false);
    }
  };

  const updateProduct = async (id: number, data: UpdateProductRequest): Promise<ProductResponse> => {
    setIsLoading(true);
    setError(null);
    try {
      return await productsApi.update(id, data);
    } finally {
      setIsLoading(false);
    }
  };

  const updateStock = async (id: number, quantity: number): Promise<void> => {
    setIsLoading(true);
    setError(null);
    try {
      await productsApi.updateStock(id, quantity);
    } finally {
      setIsLoading(false);
    }
  };

  const deleteProduct = async (id: number): Promise<void> => {
    setIsLoading(true);
    setError(null);
    try {
      await productsApi.delete(id);
    } finally {
      setIsLoading(false);
    }
  };

  return {
    isLoading,
    error,
    getProducts,
    getProductById,
    createProduct,
    updateProduct,
    updateStock,
    deleteProduct,
  };
}
