import { useState, useEffect } from 'react';
import { useParams, Link } from 'react-router-dom';
import { useAdminOrders } from '../../hooks/useOrders';
import { Button } from '../../components/ui/button';
import { Badge } from '../../components/ui/badge';
import { Card, CardContent, CardFooter, CardHeader, CardTitle } from '../../components/ui/card';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '../../components/ui/table';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '../../components/ui/select';
import { ApiError } from '../../services/api/client';
import type { OrderResponse, OrderStatus } from '../../types/order';

const statusColors: Record<OrderStatus, 'default' | 'secondary' | 'destructive' | 'outline'> = {
  Pending: 'secondary',
  Confirmed: 'default',
  Processing: 'default',
  Shipped: 'default',
  Delivered: 'outline',
  Cancelled: 'destructive',
};

const statusTransitions: Record<OrderStatus, OrderStatus[]> = {
  Pending: ['Confirmed', 'Cancelled'],
  Confirmed: ['Processing', 'Cancelled'],
  Processing: ['Shipped'],
  Shipped: ['Delivered'],
  Delivered: [],
  Cancelled: [],
};

export default function AdminOrderDetailPage() {
  const { orderId } = useParams<{ orderId: string }>();
  const { getOrderById, updateOrderStatus, isLoading } = useAdminOrders();

  const [order, setOrder] = useState<OrderResponse | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [isUpdating, setIsUpdating] = useState(false);

  useEffect(() => {
    if (orderId) {
      loadOrder(parseInt(orderId));
    }
  }, [orderId]);

  const loadOrder = async (id: number) => {
    try {
      setError(null);
      const data = await getOrderById(id);
      setOrder(data);
    } catch (err) {
      setError('Failed to load order');
    }
  };

  const handleStatusChange = async (newStatus: OrderStatus) => {
    if (!order) return;

    setIsUpdating(true);
    setError(null);

    try {
      await updateOrderStatus(order.id, { status: newStatus });
      setOrder({ ...order, status: newStatus });
    } catch (err) {
      if (err instanceof ApiError) {
        setError(err.errors.join(', '));
      } else {
        setError('Failed to update status');
      }
    } finally {
      setIsUpdating(false);
    }
  };

  const formatPrice = (price: number) => {
    return new Intl.NumberFormat('es-ES', {
      style: 'currency',
      currency: 'EUR',
    }).format(price);
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('es-ES', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  if (isLoading) {
    return (
      <div className="min-h-screen bg-background flex items-center justify-center">
        <p className="text-muted-foreground">Loading order...</p>
      </div>
    );
  }

  if (error || !order) {
    return (
      <div className="min-h-screen bg-background">
        <div className="max-w-4xl mx-auto px-4 py-8">
          <div className="bg-destructive/10 border border-destructive/20 text-destructive px-4 py-3 rounded mb-6">
            {error || 'Order not found'}
          </div>
          <Button variant="outline" asChild>
            <Link to="/admin/orders">Back to Orders</Link>
          </Button>
        </div>
      </div>
    );
  }

  const availableTransitions = statusTransitions[order.status];

  return (
    <div className="min-h-screen bg-background">
      <div className="max-w-4xl mx-auto px-4 py-8">
        <div className="mb-6">
          <Button variant="outline" asChild>
            <Link to="/admin/orders">← Back to Orders</Link>
          </Button>
        </div>

        {error && (
          <div className="bg-destructive/10 border border-destructive/20 text-destructive px-4 py-3 rounded mb-6">
            {error}
          </div>
        )}

        <div className="grid gap-6">
          {/* Order Header */}
          <Card>
            <CardHeader>
              <div className="flex justify-between items-start">
                <div>
                  <CardTitle className="text-2xl">Order #{order.id}</CardTitle>
                  <p className="text-muted-foreground mt-1">{formatDate(order.createdAt)}</p>
                  <p className="text-sm text-muted-foreground mt-1">User ID: {order.userId}</p>
                </div>
                <Badge variant={statusColors[order.status]} className="text-lg px-3 py-1">
                  {order.status}
                </Badge>
              </div>
            </CardHeader>
            <CardContent className="space-y-4">
              {order.shippingAddress && (
                <div>
                  <h3 className="font-semibold text-foreground">Shipping Address</h3>
                  <p className="text-muted-foreground">{order.shippingAddress}</p>
                </div>
              )}
              {order.notes && (
                <div>
                  <h3 className="font-semibold text-foreground">Customer Notes</h3>
                  <p className="text-muted-foreground">{order.notes}</p>
                </div>
              )}
            </CardContent>
            {availableTransitions.length > 0 && (
              <CardFooter className="flex items-center gap-4">
                <span className="text-sm font-medium">Update Status:</span>
                <Select
                  value=""
                  onValueChange={(value) => handleStatusChange(value as OrderStatus)}
                  disabled={isUpdating}
                >
                  <SelectTrigger className="w-48">
                    <SelectValue placeholder="Select new status" />
                  </SelectTrigger>
                  <SelectContent>
                    {availableTransitions.map((status) => (
                      <SelectItem key={status} value={status}>
                        {status}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
                {isUpdating && <span className="text-sm text-muted-foreground">Updating...</span>}
              </CardFooter>
            )}
          </Card>

          {/* Order Items */}
          <Card>
            <CardHeader>
              <CardTitle>Order Items</CardTitle>
            </CardHeader>
            <CardContent>
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Product</TableHead>
                    <TableHead className="text-center">Quantity</TableHead>
                    <TableHead className="text-right">Unit Price</TableHead>
                    <TableHead className="text-right">Subtotal</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {order.items.map((item) => (
                    <TableRow key={item.id}>
                      <TableCell className="font-medium">{item.productName}</TableCell>
                      <TableCell className="text-center">{item.quantity}</TableCell>
                      <TableCell className="text-right">{formatPrice(item.unitPrice)}</TableCell>
                      <TableCell className="text-right font-medium">
                        {formatPrice(item.subtotal)}
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </CardContent>
            <CardFooter className="justify-end">
              <div className="text-right">
                <p className="text-sm text-muted-foreground">Total</p>
                <p className="text-2xl font-bold">{formatPrice(order.totalAmount)}</p>
              </div>
            </CardFooter>
          </Card>
        </div>
      </div>
    </div>
  );
}
