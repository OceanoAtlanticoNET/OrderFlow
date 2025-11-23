import { useState } from 'react';
import { Outlet } from 'react-router-dom';
import { Header } from './Header';
import { Sidebar } from './Sidebar';
import { MobileNav } from './MobileNav';
import { Toaster } from '@/components/ui/sonner';

/**
 * PublicLayout - For public pages like catalog, product details
 * Shows header with navigation but no sidebar
 */
export function PublicLayout() {
  const [mobileNavOpen, setMobileNavOpen] = useState(false);

  return (
    <div className="min-h-screen bg-background">
      <Header onMenuClick={() => setMobileNavOpen(true)} showMenuButton />
      <MobileNav open={mobileNavOpen} onOpenChange={setMobileNavOpen} variant="user" />
      <main className="flex-1">
        <Outlet />
      </main>
      <Toaster position="bottom-right" />
    </div>
  );
}

/**
 * AuthLayout - For login and register pages
 * Centered card layout without navigation
 */
export function AuthLayout() {
  return (
    <div className="min-h-screen flex items-center justify-center bg-background p-4">
      <Outlet />
      <Toaster position="bottom-right" />
    </div>
  );
}

/**
 * DashboardLayout - For authenticated user pages
 * Header + Sidebar + Content area
 */
export function DashboardLayout() {
  const [mobileNavOpen, setMobileNavOpen] = useState(false);

  return (
    <div className="min-h-screen bg-background">
      <Header onMenuClick={() => setMobileNavOpen(true)} showMenuButton />
      <MobileNav open={mobileNavOpen} onOpenChange={setMobileNavOpen} variant="user" />
      <div className="flex">
        <Sidebar variant="user" className="hidden md:block fixed top-16 left-0 h-[calc(100vh-4rem)]" />
        <main className="flex-1 md:ml-64">
          <div className="p-4 md:p-6">
            <Outlet />
          </div>
        </main>
      </div>
      <Toaster position="bottom-right" />
    </div>
  );
}

/**
 * AdminLayout - For admin pages
 * Header + Admin Sidebar + Content area
 */
export function AdminLayout() {
  const [mobileNavOpen, setMobileNavOpen] = useState(false);

  return (
    <div className="min-h-screen bg-background">
      <Header onMenuClick={() => setMobileNavOpen(true)} showMenuButton />
      <MobileNav open={mobileNavOpen} onOpenChange={setMobileNavOpen} variant="admin" />
      <div className="flex">
        <Sidebar variant="admin" className="hidden md:block fixed top-16 left-0 h-[calc(100vh-4rem)]" />
        <main className="flex-1 md:ml-64">
          <div className="p-4 md:p-6">
            <Outlet />
          </div>
        </main>
      </div>
      <Toaster position="bottom-right" />
    </div>
  );
}

// Re-export components
export { Header } from './Header';
export { Sidebar } from './Sidebar';
export { MobileNav } from './MobileNav';
