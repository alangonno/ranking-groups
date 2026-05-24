import { Navigate, Outlet } from "react-router-dom";
import { getAccessToken } from "../lib/auth-token";

export function AuthenticatedRoutes() {
  const token = getAccessToken();
  if (!token) {
    return <Navigate to="/login" replace />;
  }
  return <Outlet />;
}
