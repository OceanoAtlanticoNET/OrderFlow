import { useState, useEffect } from 'react';
import { useParams, Link } from 'react-router-dom';
import { useProducts } from '../../hooks/useCatalog';
import { useCart } from '../../hooks/useCart';
import { Button } from '../../components/ui/button';
import { Badge } from '../../components/ui/badge';
import { Card, CardContent, CardHeader, CardTitle } from '../../components/ui/card';
import type { ProductResponse } from '../../types/catalog';

export default function ProductDetailPage() {
  const { productId } = useParams<{ productId: string }>();
  const { getProductById, isLoading } = useProducts();
  const { addItem, getItemQuantity, updateQuantity } = useCart();

  const [product, setProduct] = useState<ProductResponse | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (productId) {
      loadProduct(parseInt(productId));
    }
  }, [productId]);

  const loadProduct = async (id: number) => {
    try {
      setError(null);
      const data = await getProductById(id);
      setProduct(data);
    } catch (err) {
      setError('Failed to load product');
      console.error(err);
    }
  };

  const handleAddToCart = () => {
    if (!product) return;
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

  const inCartQty = product ? getItemQuantity(product.id) : 0;

  if (isLoading) {
    return (
      <div className="flex items-center justify-center py-12">
        <p className="text-muted-foreground">Loading product...</p>
      </div>
    );
  }

  if (error || !product) {
    return (
      <div className="max-w-4xl mx-auto px-4 py-8">
        <div className="bg-destructive/10 border border-destructive/20 text-destructive px-4 py-3 rounded mb-6">
          {error || 'Product not found'}
        </div>
        <Button variant="outline" asChild>
          <Link to="/catalog">Back to Catalog</Link>
        </Button>
      </div>
    );
  }

  return (
    <div className="max-w-4xl mx-auto px-4 py-8">
        <div className="mb-6">
          <Button variant="outline" asChild>
            <Link to="/catalog">← Back to Catalog</Link>
          </Button>
        </div>

        <Card>
          <CardHeader>
            <div className="flex justify-between items-start">
              <div>
                <CardTitle className="text-2xl">{product.name}</CardTitle>
                <div className="flex gap-2 mt-2">
                  <Badge variant="outline">{product.categoryName}</Badge>
                  {!product.isActive && <Badge variant="destructive">Unavailable</Badge>}
                </div>
              </div>
              {inCartQty > 0 && (
                <Badge variant="secondary" className="text-lg px-3 py-1">
                  {inCartQty} in cart
                </Badge>
              )}
            </div>
          </CardHeader>
          <CardContent className="space-y-6">
            {product.description && (
              <div>
                <h3 className="font-semibold text-foreground mb-2">Description</h3>
                <p className="text-muted-foreground">{product.description}</p>
              </div>
            )}

            <div className="flex items-end justify-between">
              <div>
                <p className="text-sm text-muted-foreground">Price</p>
                <p className="text-3xl font-bold text-foreground">
                  {formatPrice(product.price)}
                </p>
              </div>
              <div className="text-right">
                <p className="text-sm text-muted-foreground">Availability</p>
                <p className={`text-lg font-semibold ${product.stock > 0 ? 'text-success' : 'text-destructive'}`}>
                  {product.stock > 0 ? `${product.stock} in stock` : 'Out of stock'}
                </p>
              </div>
            </div>

            <div className="flex gap-4 pt-4 border-t">
              {inCartQty > 0 ? (
                <div className="flex items-center gap-4">
                  <Button
                    variant="outline"
                    onClick={() => updateQuantity(product.id, inCartQty - 1)}
                  >
                    -
                  </Button>
                  <span className="text-lg font-semibold w-12 text-center">{inCartQty}</span>
                  <Button
                    variant="outline"
                    onClick={() => updateQuantity(product.id, inCartQty + 1)}
                    disabled={inCartQty >= product.stock}
                  >
                    +
                  </Button>
                  <Button asChild className="ml-4">
                    <Link to="/cart">View Cart</Link>
                  </Button>
                </div>
              ) : (
                <Button
                  size="lg"
                  onClick={handleAddToCart}
                  disabled={product.stock === 0 || !product.isActive}
                >
                  Add to Cart
                </Button>
              )}
            </div>
          </CardContent>
        </Card>
    </div>
  );
}
