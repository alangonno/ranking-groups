import { NavLink, useLocation, useNavigate } from "react-router-dom";
import {
  Home,
  Trophy,
  Plus,
  Users,
  Contact,
} from "lucide-react";
import { useCurrentGroupId } from "../../../lib/use-group-context";

export function BottomNav() {
  const location = useLocation();
  const navigate = useNavigate();
  const groupId = useCurrentGroupId();

  const isInGroup = !!groupId;
  const isGroupsPage = location.pathname === "/groups";

  if (!isInGroup && !isGroupsPage) {
    return null;
  }

  if (isGroupsPage && !isInGroup) {
    return (
      <nav className="lg:hidden fixed bottom-0 left-0 right-0 bg-surface-container-lowest border-t border-surface-container-highest z-50 h-16 flex items-center justify-center shadow-[0_-4px_12px_rgba(0,0,0,0.05)] rounded-t-xl">
        <NavLink
          to="/groups"
          className="flex flex-col items-center justify-center gap-0.5 w-20 h-full text-primary"
        >
          <Users size={22} strokeWidth={2.5} />
          <span className="text-[10px] font-label-bold">Grupos</span>
        </NavLink>
      </nav>
    );
  }

  const navItems = [
    { path: `/group/${groupId}`, label: "Início", icon: Home },
    { path: `/group/${groupId}/ranking`, label: "Ranking", icon: Trophy },
    { path: `/group/${groupId}/events`, label: "Novo", icon: Plus, isFab: true },
    { path: "/groups", label: "Grupos", icon: Users },
    { path: `/group/${groupId}/members`, label: "Membros", icon: Contact },
  ];

  return (
    <nav className="lg:hidden fixed bottom-0 left-0 right-0 bg-surface-container-lowest z-50 flex items-center justify-around px-2 shadow-[0_-4px_12px_rgba(0,0,0,0.05)] rounded-t-xl border-t border-surface-container-highest h-16">
      {navItems.map((item) => {
        const Icon = item.icon;
        const isActive =
          item.path !== "/groups"
            ? location.pathname === item.path ||
              (item.path !== "/" && location.pathname.startsWith(item.path))
            : location.pathname === "/groups";

        if (item.isFab) {
          return (
            <button
              key={item.path}
              type="button"
              onClick={() => navigate(item.path, { state: { createEvent: true } })}
              className="relative flex flex-col items-center -top-4"
            >
              <div className="w-12 h-12 bg-primary text-white rounded-full flex items-center justify-center shadow-lg active:scale-90 transition-transform">
                <Icon size={24} />
              </div>
              <span className="text-[10px] font-label-bold text-primary mt-0.5">
                {item.label}
              </span>
            </button>
          );
        }

        return (
          <NavLink
            key={item.path}
            to={item.path}
            className={`flex flex-col items-center justify-center gap-0.5 py-1 ${
              isActive
                ? "bg-primary-container text-on-primary-container rounded-full px-4"
                : "text-secondary"
            }`}
          >
            <Icon
              size={22}
              strokeWidth={isActive ? 2.5 : 2}
              className={isActive ? "" : ""}
            />
            <span className="text-[10px] font-label-bold">{item.label}</span>
          </NavLink>
        );
      })}
    </nav>
  );
}
