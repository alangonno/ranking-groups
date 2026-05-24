import { Outlet } from "react-router-dom";
import { AppSidebar } from "../components/authenticated/sidebar/app-sidebar";
import { BottomNav } from "../components/authenticated/navigation/bottom-nav";

export function AuthenticatedLayout() {
  return (
    <div className="min-h-screen bg-off-white flex">
      {/* Sidebar Desktop */}
      <div className="hidden lg:block">
        <AppSidebar />
      </div>

      {/* Main Content */}
      <main className="flex-1 min-h-screen overflow-y-auto pb-20 lg:pb-0 lg:pl-60">
        <Outlet />
      </main>

      {/* Bottom Nav Mobile */}
      <BottomNav />
    </div>
  );
}
