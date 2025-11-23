import { useState, useEffect, useCallback } from 'react';
import type { CartItem, Cart } from '../types/cart';

const CART_STORAGE_KEY = 'orderflow_cart';

function loadCartFromStorage(): CartItem[] {
  try {
    const stored = localStorage.getItem(CART_STORAGE_KEY);
    return stored ? JSON.parse(stored) : [];
  } catch {
    return [];
  }
}

function saveCartToStorage(items: CartItem[]): void {
  localStorage.setItem(CART_STORAGE_KEY, JSON.stringify(items));
}

export function useCart() {
  const [items, setItems] = useState<CartItem[]>(() => loadCartFromStorage());

  // Persist to localStorage when items change
  useEffect(() => {
    saveCartToStorage(items);
  }, [items]);

  const addItem = useCallback((product: { id: number; name: string; price: number; stock: number }) => {
    setItems(current => {
      const existingIndex = current.findIndex(item => item.productId === product.id);

      if (existingIndex >= 0) {
        // Update quantity if already in cart
        const updated = [...current];
        const newQuantity = updated[existingIndex].quantity + 1;
        if (newQuantity <= product.stock) {
          updated[existingIndex] = { ...updated[existingIndex], quantity: newQuantity };
        }
        return updated;
      }

      // Add new item
      return [...current, {
        productId: product.id,
        name: product.name,
        price: product.price,
        quantity: 1,
        stock: product.stock,
      }];
    });
  }, []);

  const removeItem = useCallback((productId: number) => {
    setItems(current => current.filter(item => item.productId !== productId));
  }, []);

  const updateQuantity = useCallback((productId: number, quantity: number) => {
    if (quantity < 1) {
      removeItem(productId);
      return;
    }

    setItems(current => {
      return current.map(item => {
        if (item.productId === productId) {
          const newQuantity = Math.min(quantity, item.stock);
          return { ...item, quantity: newQuantity };
        }
        return item;
      });
    });
  }, [removeItem]);

  const clearCart = useCallback(() => {
    setItems([]);
  }, []);

  const getItemQuantity = useCallback((productId: number): number => {
    const item = items.find(i => i.productId === productId);
    return item?.quantity ?? 0;
  }, [items]);

  const cart: Cart = {
    items,
    totalItems: items.reduce((sum, item) => sum + item.quantity, 0),
    totalAmount: items.reduce((sum, item) => sum + item.price * item.quantity, 0),
  };

  return {
    cart,
    addItem,
    removeItem,
    updateQuantity,
    clearCart,
    getItemQuantity,
  };
}
