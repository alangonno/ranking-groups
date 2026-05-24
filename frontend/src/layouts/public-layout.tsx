import { Outlet } from "react-router-dom";

export function PublicLayout() {
  return (
    <div className="min-h-screen flex items-center justify-center bg-off-white px-4">
      <div className="w-full max-w-md">
        <Outlet />
      </div>
    </div>
  );
}
