import { Navigate, Outlet } from "react-router-dom";
import { useAuthContext } from "../providers/auth-provider";
import { AppSpinner } from "../components/ui/app-spinner";

export function AuthenticatedRoutes() {
  const { isAuthenticated, isLoading } = useAuthContext();

  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <AppSpinner className="w-10 h-10" />
      </div>
    );
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  return <Outlet />;
}
