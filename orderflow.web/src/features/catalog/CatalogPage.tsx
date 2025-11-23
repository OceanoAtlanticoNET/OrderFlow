import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { useProducts, useCategories } from '../../hooks/useCatalog';
import { useCart } from '../../hooks/useCart';
import { Button } from '../../components/ui/button';
import { Input } from '../../components/ui/input';
import { Card, CardContent, CardFooter, CardHeader, CardTitle } from '../../components/ui/card';
import { Badge } from '../../components/ui/badge';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '../../components/ui/select';
import type { ProductListResponse } from '../../types/catalog';
import type { CategoryResponse } from '../../types/catalog';

export default function CatalogPage() {
  const { getProducts, isLoading: productsLoading } = useProducts();
  const { getCategories } = useCategories();
  const { addItem, getItemQuantity } = useCart();

  const [products, setProducts] = useState<ProductListResponse[]>([]);
  const [categories, setCategories] = useState<CategoryResponse[]>([]);
  const [selectedCategory, setSelectedCategory] = useState<string>('all');
  const [searchQuery, setSearchQuery] = useState('');
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadCategories();
    loadProducts();
  }, []);

  useEffect(() => {
    loadProducts();
  }, [selectedCategory, searchQuery]);

  const loadCategories = async () => {
    try {
      const data = await getCategories();
      setCategories(data);
    } catch (err) {
      console.error('Failed to load categories', err);
    }
  };

  const loadProducts = async () => {
    try {
      setError(null);
      const params: { categoryId?: number; search?: string; isActive?: boolean } = {
        isActive: true,
      };
      if (selectedCategory !== 'all') {
        params.categoryId = parseInt(selectedCategory);
      }
      if (searchQuery.trim()) {
        params.search = searchQuery.trim();
      }
      const data = await getProducts(params);
      setProducts(data);
    } catch (err) {
      setError('Failed to load products');
      console.error(err);
    }
  };

  const handleAddToCart = (product: ProductListResponse) => {
    addItem({
      id: product.id,
      name: product.name,
      price: product.price,
      stock: product.stock,
    });
  };

  const formatPrice = (price: number) => {
    return new Intl.NumberFormat('es-ES', {
      style: 'currency',
      currency: 'EUR',
    }).format(price);
  };

  return (
    <div className="max-w-7xl mx-auto px-4 py-8">
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-foreground">Product Catalog</h1>
        <p className="text-muted-foreground mt-2">Browse our products and add them to your cart</p>
      </div>

        {/* Filters */}
        <div className="flex flex-col sm:flex-row gap-4 mb-8">
          <div className="flex-1">
            <Input
              placeholder="Search products..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="max-w-md"
            />
          </div>
          <div className="w-full sm:w-48">
            <Select value={selectedCategory} onValueChange={setSelectedCategory}>
              <SelectTrigger>
                <SelectValue placeholder="All Categories" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All Categories</SelectItem>
                {categories.map((category) => (
                  <SelectItem key={category.id} value={category.id.toString()}>
                    {category.name}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
        </div>

        {error && (
          <div className="bg-destructive/10 border border-destructive/20 text-destructive px-4 py-3 rounded mb-6">
            {error}
          </div>
        )}

        {productsLoading ? (
          <div className="text-center py-12">
            <p className="text-muted-foreground">Loading products...</p>
          </div>
        ) : products.length === 0 ? (
          <div className="text-center py-12">
            <p className="text-muted-foreground">No products found</p>
          </div>
        ) : (
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
            {products.map((product) => {
              const inCartQty = getItemQuantity(product.id);
              return (
                <Card key={product.id} className="flex flex-col">
                  <CardHeader>
                    <div className="flex justify-between items-start">
                      <CardTitle className="text-lg">{product.name}</CardTitle>
                      {inCartQty > 0 && (
                        <Badge variant="secondary">{inCartQty} in cart</Badge>
                      )}
                    </div>
                    <Badge variant="outline">{product.categoryName}</Badge>
                  </CardHeader>
                  <CardContent className="flex-1">
                    <p className="text-2xl font-bold text-foreground">
                      {formatPrice(product.price)}
                    </p>
                    <p className="text-sm text-muted-foreground mt-2">
                      {product.stock > 0 ? `${product.stock} in stock` : 'Out of stock'}
                    </p>
                  </CardContent>
                  <CardFooter className="flex gap-2">
                    <Button
                      variant="outline"
                      className="flex-1"
                      asChild
                    >
                      <Link to={`/catalog/${product.id}`}>View</Link>
                    </Button>
                    <Button
                      className="flex-1"
                      onClick={() => handleAddToCart(product)}
                      disabled={product.stock === 0}
                    >
                      Add to Cart
                    </Button>
                  </CardFooter>
                </Card>
              );
            })}
          </div>
        )}
    </div>
  );
}
