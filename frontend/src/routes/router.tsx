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
import { ProfilePage } from "../pages/authenticated/profile-page";
import { useAuthContext } from "../providers/auth-provider";
import { AppSpinner } from "../components/ui/app-spinner";

function FullPageSpinner() {
  return (
    <div className="flex items-center justify-center min-h-screen">
      <AppSpinner className="w-10 h-10" />
    </div>
  );
}

function AuthGuard() {
  const { isAuthenticated, isLoading } = useAuthContext();

  if (isLoading) {
    return <FullPageSpinner />;
  }

  return isAuthenticated ? <Outlet /> : <Navigate to="/login" replace />;
}

function PublicGuard() {
  const { isAuthenticated, isLoading } = useAuthContext();

  if (isLoading) {
    return <FullPageSpinner />;
  }

  return isAuthenticated ? <Navigate to="/groups" replace /> : <Outlet />;
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
              { path: "profile/:userId", element: <ProfilePage /> },
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
