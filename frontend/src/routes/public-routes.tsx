import { Navigate, Outlet } from "react-router-dom";
import { authStore } from "../store/auth-store";

export function PublicRoutes() {
  const token = authStore.getAccessToken();
  if (token) {
    return <Navigate to="/groups" replace />;
  }
  return <Outlet />;
}
