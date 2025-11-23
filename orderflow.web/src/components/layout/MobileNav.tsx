import { Link, useLocation } from 'react-router-dom';
import { cn } from '@/lib/utils';
import {
  Sheet,
  SheetContent,
  SheetHeader,
  SheetTitle,
} from '@/components/ui/sheet';
import { ScrollArea } from '@/components/ui/scroll-area';
import { Separator } from '@/components/ui/separator';
import {
  LayoutDashboard,
  ShoppingBag,
  Package,
  ShoppingCart,
  Users,
  Tags,
  ClipboardList,
  LogOut,
  type LucideIcon,
} from 'lucide-react';
import { useAuth } from '@/hooks/useAuth';

interface NavItem {
  title: string;
  href: string;
  icon: LucideIcon;
}

const userNavItems: NavItem[] = [
  { title: 'Dashboard', href: '/dashboard', icon: LayoutDashboard },
  { title: 'Browse Catalog', href: '/catalog', icon: ShoppingBag },
  { title: 'Shopping Cart', href: '/cart', icon: ShoppingCart },
  { title: 'My Orders', href: '/orders', icon: Package },
];

const adminNavItems: NavItem[] = [
  { title: 'Dashboard', href: '/admin/dashboard', icon: LayoutDashboard },
  { title: 'Categories', href: '/admin/categories', icon: Tags },
  { title: 'Products', href: '/admin/products', icon: ShoppingBag },
  { title: 'Orders', href: '/admin/orders', icon: ClipboardList },
  { title: 'Users', href: '/admin/users', icon: Users },
];

interface MobileNavProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  variant?: 'user' | 'admin';
}

export function MobileNav({ open, onOpenChange, variant = 'user' }: MobileNavProps) {
  const location = useLocation();
  const { user, logout, isAuthenticated } = useAuth();
  const navItems = variant === 'admin' ? adminNavItems : userNavItems;
  const isAdmin = user?.roles?.includes('Admin');

  const handleLogout = () => {
    logout();
    onOpenChange(false);
    window.location.href = '/login';
  };

  const handleLinkClick = () => {
    onOpenChange(false);
  };

  return (
    <Sheet open={open} onOpenChange={onOpenChange}>
      <SheetContent side="left" className="w-72 p-0">
        <SheetHeader className="p-4 border-b">
          <SheetTitle className="flex items-center gap-2">
            <Package className="h-5 w-5 text-primary" />
            OrderFlow
          </SheetTitle>
        </SheetHeader>
        <ScrollArea className="h-[calc(100vh-65px)]">
          <div className="p-4 space-y-4">
            {/* User info */}
            {isAuthenticated && (
              <>
                <div className="px-2">
                  <p className="text-sm font-medium">{user?.email}</p>
                  <p className="text-xs text-muted-foreground">
                    {user?.roles?.join(', ')}
                  </p>
                </div>
                <Separator />
              </>
            )}

            {/* Navigation */}
            <div className="space-y-1">
              <h3 className="px-2 text-xs font-semibold text-muted-foreground uppercase tracking-wider">
                {variant === 'admin' ? 'Admin' : 'Menu'}
              </h3>
              <nav className="flex flex-col gap-1">
                {navItems.map((item) => {
                  const isActive = location.pathname === item.href;
                  return (
                    <Link
                      key={item.href}
                      to={item.href}
                      onClick={handleLinkClick}
                      className={cn(
                        'flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium transition-colors',
                        'hover:bg-accent hover:text-accent-foreground',
                        'focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring',
                        isActive
                          ? 'bg-primary text-primary-foreground'
                          : 'text-muted-foreground'
                      )}
                    >
                      <item.icon className="h-4 w-4" />
                      {item.title}
                    </Link>
                  );
                })}
              </nav>
            </div>

            {/* Admin link for regular users who are also admins */}
            {variant === 'user' && isAdmin && (
              <>
                <Separator />
                <div className="space-y-1">
                  <h3 className="px-2 text-xs font-semibold text-muted-foreground uppercase tracking-wider">
                    Admin
                  </h3>
                  <Link
                    to="/admin/dashboard"
                    onClick={handleLinkClick}
                    className="flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium text-muted-foreground hover:bg-accent hover:text-accent-foreground"
                  >
                    <LayoutDashboard className="h-4 w-4" />
                    Admin Panel
                  </Link>
                </div>
              </>
            )}

            {/* Logout */}
            {isAuthenticated && (
              <>
                <Separator />
                <button
                  onClick={handleLogout}
                  className="flex w-full items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium text-destructive hover:bg-destructive/10 transition-colors"
                >
                  <LogOut className="h-4 w-4" />
                  Log out
                </button>
              </>
            )}

            {/* Auth links for non-authenticated */}
            {!isAuthenticated && (
              <>
                <Separator />
                <div className="space-y-2">
                  <Link
                    to="/login"
                    onClick={handleLinkClick}
                    className="flex w-full items-center justify-center rounded-lg border px-3 py-2 text-sm font-medium hover:bg-accent"
                  >
                    Sign in
                  </Link>
                  <Link
                    to="/register"
                    onClick={handleLinkClick}
                    className="flex w-full items-center justify-center rounded-lg bg-primary text-primary-foreground px-3 py-2 text-sm font-medium hover:bg-primary/90"
                  >
                    Sign up
                  </Link>
                </div>
              </>
            )}
          </div>
        </ScrollArea>
      </SheetContent>
    </Sheet>
  );
}
