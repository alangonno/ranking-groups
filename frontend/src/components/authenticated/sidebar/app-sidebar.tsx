import { NavLink, useNavigate } from "react-router-dom";
import {
  LayoutDashboard,
  Users,
  Trophy,
  User,
  LogOut,
} from "lucide-react";
import { useCurrentUser, useLogout } from "../../../hooks/use-auth";
import { getLastGroupId } from "../../../lib/group-storage";

const menuItems = [
  { path: "/groups", label: "Meus Grupos", icon: Users },
  { path: "/ranking", label: "Ranking", icon: Trophy },
  { path: "/profile", label: "Perfil", icon: User },
];

export function AppSidebar() {
  const { data: user } = useCurrentUser();
  const logout = useLogout();
  const navigate = useNavigate();
  const lastGroupId = getLastGroupId();

  function handleLogout() {
    logout.mutate(undefined, {
      onSuccess: () => navigate("/login"),
    });
  }

  const dashboardPath = lastGroupId ? `/group/${lastGroupId}` : "/groups";

  return (
    <aside className="w-60 min-h-screen bg-white flex flex-col fixed left-0 top-0 z-40">
      {/* Logo */}
      <div className="px-5 py-6">
        <h1 className="text-xl font-bold tracking-tight">
          <span className="text-primary">4</span>
          <span className="text-text-primary"> Quase </span>
          <span className="text-primary">5</span>
        </h1>
      </div>

      {/* Menu */}
      <nav className="flex-1 px-3 space-y-1">
        {/* Dashboard Link - contextual ao último grupo */}
        <NavLink
          to={dashboardPath}
          className={({ isActive }) =>
            `flex items-center gap-3 px-3 py-2.5 text-sm font-medium transition-colors ${
              isActive && lastGroupId
                ? "bg-primary-light text-primary border-l-4 border-primary rounded-r-lg"
                : "text-text-secondary hover:bg-gray-50 hover:text-text-primary rounded-lg"
            }`
          }
        >
          <LayoutDashboard size={20} />
          Dashboard
        </NavLink>

        {menuItems.map((item) => {
          const Icon = item.icon;
          return (
            <NavLink
              key={item.path}
              to={item.path}
              className={({ isActive }) =>
                `flex items-center gap-3 px-3 py-2.5 text-sm font-medium transition-colors ${
                  isActive
                    ? "bg-primary-light text-primary border-l-4 border-primary rounded-r-lg"
                    : "text-text-secondary hover:bg-gray-50 hover:text-text-primary rounded-lg"
                }`
              }
            >
              <Icon size={20} />
              {item.label}
            </NavLink>
          );
        })}
      </nav>

      {/* User Footer */}
      <div className="p-4 border-t border-border">
        <div className="flex items-center gap-3 mb-3">
          <div className="w-9 h-9 rounded-full bg-primary-light flex items-center justify-center text-primary font-bold text-sm">
            {user?.name?.charAt(0).toUpperCase() || "U"}
          </div>
          <div className="flex-1 min-w-0">
            <p className="text-sm font-medium text-text-primary truncate">
              {user?.name || "Usuário"}
            </p>
            <p className="text-xs text-text-muted truncate">
              {user?.email || ""}
            </p>
          </div>
        </div>
        <button
          type="button"
          onClick={handleLogout}
          disabled={logout.isPending}
          className="w-full flex items-center justify-center gap-2 py-2 text-sm text-text-secondary hover:text-text-primary hover:bg-gray-50 rounded-lg transition-colors disabled:opacity-50"
        >
          <LogOut size={16} />
          Sair
        </button>
      </div>
    </aside>
  );
}
