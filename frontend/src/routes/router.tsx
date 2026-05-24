import {
  createBrowserRouter,
  Navigate,
  Outlet,
} from "react-router-dom";
import { PublicLayout } from "../layouts/public-layout";
import { AuthenticatedLayout } from "../layouts/authenticated-layout";
import { LoginPage } from "../pages/public/login-page";
import { RegisterPage } from "../pages/public/register-page";
import { GroupsPage } from "../pages/authenticated/groups-page";
import { DashboardPage } from "../pages/authenticated/dashboard-page";
import { getAccessToken } from "../lib/auth-token";

function AuthGuard() {
  const token = getAccessToken();
  return token ? <Outlet /> : <Navigate to="/login" replace />;
}

function PublicGuard() {
  const token = getAccessToken();
  return token ? <Navigate to="/groups" replace /> : <Outlet />;
}

export const router = createBrowserRouter([
  {
    element: <PublicGuard />,
    children: [
      {
        element: <PublicLayout />,
        children: [
          { path: "login", element: <LoginPage /> },
          { path: "register", element: <RegisterPage /> },
        ],
      },
    ],
  },
  {
    element: <AuthGuard />,
    children: [
      {
        element: <AuthenticatedLayout />,
        children: [
          { path: "groups", element: <GroupsPage /> },
          { path: "group/:groupId", element: <DashboardPage /> },
          { path: "", element: <Navigate to="/groups" replace /> },
        ],
      },
    ],
  },
  {
    path: "*",
    element: <Navigate to="/login" replace />,
  },
]);
