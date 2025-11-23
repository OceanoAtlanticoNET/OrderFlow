import { Link } from "react-router-dom";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { useAuth } from "@/hooks/useAuth";

export function DashboardPage() {
  const { user } = useAuth();

  return (
    <div className="max-w-4xl mx-auto space-y-6">
      <div>
        <h1 className="text-3xl font-bold text-foreground">Dashboard</h1>
        <p className="text-muted-foreground mt-1">Welcome back to OrderFlow</p>
      </div>

        <Card>
          <CardHeader>
            <CardTitle>Welcome back!</CardTitle>
            <CardDescription>
              You are successfully authenticated with the OrderFlow Identity API
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            <div>
              <p className="text-sm font-medium text-muted-foreground">
                Email
              </p>
              <p className="text-lg">{user?.email || "N/A"}</p>
            </div>

            <div>
              <p className="text-sm font-medium text-muted-foreground">
                User ID
              </p>
              <p className="text-lg font-mono text-sm">
                {user?.userId || "N/A"}
              </p>
            </div>

            <div>
              <p className="text-sm font-medium text-muted-foreground">Roles</p>
              <div className="flex gap-2 flex-wrap">
                {user?.roles?.map((role) => (
                  <Badge key={role} variant="secondary">
                    {role}
                  </Badge>
                )) || <span className="text-lg">N/A</span>}
              </div>
            </div>
          </CardContent>
        </Card>

        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          <Card className="hover:shadow-lg transition-shadow cursor-pointer">
            <Link to="/catalog">
              <CardHeader>
                <CardTitle>Browse Products</CardTitle>
                <CardDescription>
                  Explore our catalog
                </CardDescription>
              </CardHeader>
              <CardContent>
                <p className="text-sm text-muted-foreground">
                  Browse products and add them to your cart
                </p>
              </CardContent>
            </Link>
          </Card>

          <Card className="hover:shadow-lg transition-shadow cursor-pointer">
            <Link to="/cart">
              <CardHeader>
                <CardTitle>Shopping Cart</CardTitle>
                <CardDescription>
                  View your cart
                </CardDescription>
              </CardHeader>
              <CardContent>
                <p className="text-sm text-muted-foreground">
                  Review items in your cart and proceed to checkout
                </p>
              </CardContent>
            </Link>
          </Card>

          <Card className="hover:shadow-lg transition-shadow cursor-pointer">
            <Link to="/orders">
              <CardHeader>
                <CardTitle>My Orders</CardTitle>
                <CardDescription>
                  Track your orders
                </CardDescription>
              </CardHeader>
              <CardContent>
                <p className="text-sm text-muted-foreground">
                  View order history and track current orders
                </p>
              </CardContent>
            </Link>
          </Card>

          <Card className="hover:shadow-lg transition-shadow cursor-pointer">
            <Link to="/profile">
              <CardHeader>
                <CardTitle>My Profile</CardTitle>
                <CardDescription>
                  View and manage your profile
                </CardDescription>
              </CardHeader>
              <CardContent>
                <p className="text-sm text-muted-foreground">
                  View your profile information and update your details
                </p>
              </CardContent>
            </Link>
          </Card>

          <Card className="hover:shadow-lg transition-shadow cursor-pointer">
            <Link to="/profile/password">
              <CardHeader>
                <CardTitle>Security</CardTitle>
                <CardDescription>
                  Manage your password
                </CardDescription>
              </CardHeader>
              <CardContent>
                <p className="text-sm text-muted-foreground">
                  Change your password to keep your account secure
                </p>
              </CardContent>
            </Link>
          </Card>
        </div>
      </div>
  );
}
