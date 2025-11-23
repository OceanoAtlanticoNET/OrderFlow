import { Link, useLocation } from 'react-router-dom';
import { cn } from '@/lib/utils';
import { ScrollArea } from '@/components/ui/scroll-area';
import {
  LayoutDashboard,
  ShoppingBag,
  Package,
  ShoppingCart,
  Users,
  Tags,
  ClipboardList,
  type LucideIcon,
} from 'lucide-react';

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

interface SidebarProps {
  variant?: 'user' | 'admin';
  className?: string;
}

export function Sidebar({ variant = 'user', className }: SidebarProps) {
  const location = useLocation();
  const navItems = variant === 'admin' ? adminNavItems : userNavItems;

  return (
    <aside className={cn('pb-12 w-64 border-r bg-background', className)}>
      <ScrollArea className="h-full py-6 px-4">
        <div className="space-y-1">
          <h2 className="mb-4 px-2 text-lg font-semibold tracking-tight">
            {variant === 'admin' ? 'Admin Panel' : 'Navigation'}
          </h2>
          <nav className="flex flex-col gap-1">
            {navItems.map((item) => {
              const isActive = location.pathname === item.href;
              return (
                <Link
                  key={item.href}
                  to={item.href}
                  className={cn(
                    'flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium transition-colors',
                    'hover:bg-accent hover:text-accent-foreground',
                    'focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring',
                    isActive
                      ? 'bg-primary text-primary-foreground hover:bg-primary/90 hover:text-primary-foreground'
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
      </ScrollArea>
    </aside>
  );
}
