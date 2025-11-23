import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import { LoginPage } from "@/features/auth/LoginPage";
import { RegisterPage } from "@/features/auth/RegisterPage";
import { DashboardPage } from "@/features/dashboard/DashboardPage";
import { AdminDashboard } from "@/features/admin/AdminDashboard";
import { UserManagement } from "@/features/admin/UserManagement";
import { UserDetail } from "@/features/admin/UserDetail";
import { RoleManagement } from "@/features/admin/RoleManagement";
import { ProfilePage } from "@/features/profile/ProfilePage";
import { EditProfilePage } from "@/features/profile/EditProfilePage";
import { ChangePasswordPage } from "@/features/profile/ChangePasswordPage";
import { ProtectedRoute } from "@/components/ProtectedRoute";
import { AdminRoute } from "@/components/AdminRoute";
import { RoleRedirect } from "@/components/RoleRedirect";
// Layouts
import { PublicLayout, AuthLayout, DashboardLayout, AdminLayout } from "@/components/layout";
// Catalog
import CatalogPage from "@/features/catalog/CatalogPage";
import ProductDetailPage from "@/features/catalog/ProductDetailPage";
// Cart
import CartPage from "@/features/cart/CartPage";
// Orders
import OrdersPage from "@/features/orders/OrdersPage";
import OrderDetailPage from "@/features/orders/OrderDetailPage";
// Admin Catalog & Orders
import AdminCategoriesPage from "@/features/admin/AdminCategoriesPage";
import AdminProductsPage from "@/features/admin/AdminProductsPage";
import AdminOrdersPage from "@/features/admin/AdminOrdersPage";
import AdminOrderDetailPage from "@/features/admin/AdminOrderDetailPage";

function App() {
  return (
    <BrowserRouter>
      <Routes>
        {/* Auth routes - Centered card layout */}
        <Route element={<AuthLayout />}>
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />
        </Route>

        {/* Public routes - Header only */}
        <Route element={<PublicLayout />}>
          <Route path="/catalog" element={<CatalogPage />} />
          <Route path="/catalog/:productId" element={<ProductDetailPage />} />
        </Route>

        {/* Root redirects based on role */}
        <Route path="/" element={<RoleRedirect />} />

        {/* Protected client routes - Header + Sidebar */}
        <Route element={<ProtectedRoute><DashboardLayout /></ProtectedRoute>}>
          <Route path="/dashboard" element={<DashboardPage />} />
          <Route path="/profile" element={<ProfilePage />} />
          <Route path="/profile/edit" element={<EditProfilePage />} />
          <Route path="/profile/password" element={<ChangePasswordPage />} />
          <Route path="/cart" element={<CartPage />} />
          <Route path="/orders" element={<OrdersPage />} />
          <Route path="/orders/:orderId" element={<OrderDetailPage />} />
        </Route>

        {/* Admin routes - Header + Admin Sidebar */}
        <Route element={<AdminRoute><AdminLayout /></AdminRoute>}>
          <Route path="/admin/dashboard" element={<AdminDashboard />} />
          <Route path="/admin/users" element={<UserManagement />} />
          <Route path="/admin/users/:userId" element={<UserDetail />} />
          <Route path="/admin/roles" element={<RoleManagement />} />
          <Route path="/admin/categories" element={<AdminCategoriesPage />} />
          <Route path="/admin/products" element={<AdminProductsPage />} />
          <Route path="/admin/orders" element={<AdminOrdersPage />} />
          <Route path="/admin/orders/:orderId" element={<AdminOrderDetailPage />} />
        </Route>

        {/* Catch all */}
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </BrowserRouter>
  );
}

export default App;
