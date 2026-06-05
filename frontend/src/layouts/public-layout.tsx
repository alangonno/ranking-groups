import { Outlet } from "react-router-dom";
import { ThemeToggleButton } from "../components/ui/theme-toggle-button";

export function PublicLayout() {
  return (
    <div className="min-h-screen flex items-center justify-center bg-off-white px-4">
      <ThemeToggleButton />
      <div className="w-full max-w-md">
        <Outlet />
      </div>
    </div>
  );
}
