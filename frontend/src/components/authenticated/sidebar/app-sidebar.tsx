import { NavLink, useNavigate } from "react-router-dom";
import {
  LayoutDashboard,
  Users,
  Trophy,
  User,
  Contact,
  LogOut,
  Plus,
  Settings,
} from "lucide-react";
import { useLogout } from "../../../hooks/use-auth";
import { useAuthContext } from "../../../providers/auth-provider";
import { useCurrentGroupId } from "../../../lib/use-group-context";

export function AppSidebar() {
  const { user } = useAuthContext();
  const logout = useLogout();
  const navigate = useNavigate();
  const groupId = useCurrentGroupId();

  function handleLogout() {
    logout.mutate(undefined, {
      onSuccess: () => navigate("/login"),
    });
  }

  const navItems = groupId
    ? [
        { path: `/group/${groupId}`, label: "Dashboard", icon: LayoutDashboard, exact: true },
        { path: `/group/${groupId}/ranking`, label: "Ranking", icon: Trophy },
        { path: `/group/${groupId}/events`, label: "Eventos", icon: Trophy },
        { path: `/group/${groupId}/members`, label: "Membros", icon: Contact },
        { path: `/group/${groupId}/profile/${user!.id}`, label: "Perfil", icon: User },
      ]
    : [
        { path: null, label: "Dashboard", icon: LayoutDashboard },
        { path: null, label: "Ranking", icon: Trophy },
        { path: null, label: "Eventos", icon: Trophy },
        { path: null, label: "Membros", icon: Contact },
        { path: null, label: "Perfil", icon: User },
      ];

  return (
    <aside className="w-64 min-h-screen bg-surface-container-lowest flex flex-col fixed left-0 top-0 z-40 shadow-sm border-r border-surface-container">
      {/* Logo */}
      <div className="px-6 py-6">
        <h1 className="text-headline-md font-headline-md font-black text-primary tracking-tight">
          4Quase5
        </h1>
      </div>

      {/* User Profile */}
      <div className="px-6 mb-6">
        <div className="flex items-center gap-3">
          <div className="w-12 h-12 rounded-full bg-primary-light flex items-center justify-center text-primary font-bold text-lg flex-shrink-0">
            {user?.name?.charAt(0).toUpperCase() || "U"}
          </div>
          <div className="min-w-0">
            <p className="text-label-bold font-label-bold text-on-surface truncate">
              {user?.name || "Usuário"}
            </p>
            <p className="text-caption font-caption text-secondary truncate">
              Membro ativo
            </p>
          </div>
        </div>
      </div>

      {/* Create Event Button */}
      {groupId && (
        <div className="px-6 mb-6">
          <button
            type="button"
            className="w-full bg-primary text-white rounded-full py-2.5 px-4 text-label-bold font-label-bold flex items-center justify-center gap-2 hover:opacity-90 transition-opacity shadow-sm"
          >
            <Plus size={18} />
            Create Event
          </button>
        </div>
      )}

      {/* Navigation */}
      <nav className="flex-1 px-4 space-y-1">
        {navItems.map((item) => {
          const Icon = item.icon;
          if (item.path) {
            return (
              <NavLink
                key={item.path}
                to={item.path}
                end={item.exact}
                className={({ isActive }) =>
                  `flex items-center gap-3 px-4 py-2.5 rounded-xl text-label-bold font-label-bold transition-all duration-200 ${
                    isActive
                      ? "text-primary font-bold border-r-4 border-primary bg-primary-container/10"
                      : "text-secondary hover:bg-surface-container hover:text-on-surface"
                  }`
                }
              >
                <Icon size={20} />
                {item.label}
              </NavLink>
            );
          }
          return (
            <div
              key={item.label}
              className="flex items-center gap-3 px-4 py-2.5 rounded-xl text-label-bold font-label-bold text-text-muted cursor-not-allowed"
            >
              <Icon size={20} />
              {item.label}
            </div>
          );
        })}
      </nav>

      {/* Meus Grupos - always active when present */}
      <div className="px-4 mb-2">
        <NavLink
          to="/groups"
          className={({ isActive }) =>
            `flex items-center gap-3 px-4 py-2.5 rounded-xl text-label-bold font-label-bold transition-all duration-200 ${
              isActive
                ? "text-primary font-bold border-r-4 border-primary bg-primary-container/10"
                : "text-secondary hover:bg-surface-container hover:text-on-surface"
            }`
          }
        >
          <Users size={20} />
          Meus Grupos
        </NavLink>
      </div>

      {/* Bottom section */}
      <div className="mt-auto border-t border-surface-container pt-3 px-4 pb-6 space-y-1">
        <button
          type="button"
          className="w-full flex items-center gap-3 px-4 py-2 rounded-xl text-label-bold font-label-bold text-secondary hover:bg-surface-container transition-all duration-200"
        >
          <Settings size={20} />
          Settings
        </button>
        <button
          type="button"
          onClick={handleLogout}
          disabled={logout.isPending}
          className="w-full flex items-center gap-3 px-4 py-2 rounded-xl text-label-bold font-label-bold text-secondary hover:bg-surface-container transition-all duration-200 disabled:opacity-50"
        >
          <LogOut size={20} />
          Sair
        </button>
      </div>
    </aside>
  );
}
