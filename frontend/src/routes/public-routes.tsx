import { Navigate, Outlet } from "react-router-dom";
import { getAccessToken } from "../lib/auth-token";

export function PublicRoutes() {
  const token = getAccessToken();
  if (token) {
    return <Navigate to="/dashboard" replace />;
  }
  return <Outlet />;
}
