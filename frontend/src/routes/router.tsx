import {
  createBrowserRouter,
  Navigate,
  Outlet,
} from "react-router-dom";
import { PublicLayout } from "../layouts/public-layout";
import { AuthenticatedLayout } from "../layouts/authenticated-layout";
import { GroupLayout } from "../layouts/group-layout";
import { LoginPage } from "../pages/public/login-page";
import { RegisterPage } from "../pages/public/register-page";
import { GroupsPage } from "../pages/authenticated/groups-page";
import { DashboardPage } from "../pages/authenticated/dashboard-page";
import { RankingPage } from "../pages/authenticated/ranking-page";
import { EventsPage } from "../pages/authenticated/events-page";
import { MembersPage } from "../pages/authenticated/members-page";
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
          {
            path: "group/:groupId",
            element: <GroupLayout />,
            children: [
              { path: "", element: <DashboardPage /> },
              { path: "ranking", element: <RankingPage /> },
              { path: "events", element: <EventsPage /> },
              { path: "members", element: <MembersPage /> },
            ],
          },
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
