import { NavLink, useLocation } from "react-router-dom";
import {
  Home,
  Trophy,
  Plus,
  Users,
  User,
} from "lucide-react";

const navItems = [
  { path: "/dashboard", label: "Início", icon: Home },
  { path: "/ranking", label: "Ranking", icon: Trophy },
  { path: "/create", label: "Novo", icon: Plus, isFab: true },
  { path: "/groups", label: "Grupos", icon: Users },
  { path: "/profile", label: "Perfil", icon: User },
];

export function BottomNav() {
  const location = useLocation();

  return (
    <nav className="lg:hidden fixed bottom-0 left-0 right-0 bg-white border-t border-border z-50 h-16 flex items-center justify-around px-2 shadow-[0_-2px_10px_rgba(0,0,0,0.05)]">
      {navItems.map((item) => {
        const Icon = item.icon;
        const isActive = location.pathname === item.path ||
          (item.path !== "/" && location.pathname.startsWith(item.path));

        if (item.isFab) {
          return (
            <NavLink
              key={item.path}
              to={item.path}
              className="relative -top-5 flex flex-col items-center"
            >
              <div className="w-14 h-14 rounded-full bg-primary text-white flex items-center justify-center shadow-lg hover:bg-primary-hover active:scale-95 transition-transform">
                <Icon size={24} />
              </div>
              <span className="text-[10px] font-medium text-text-secondary mt-0.5">
                {item.label}
              </span>
            </NavLink>
          );
        }

        return (
          <NavLink
            key={item.path}
            to={item.path}
            className={`flex flex-col items-center justify-center gap-0.5 w-14 h-full ${
              isActive ? "text-primary" : "text-text-muted"
            }`}
          >
            <Icon size={22} strokeWidth={isActive ? 2.5 : 2} />
            <span className="text-[10px] font-medium">{item.label}</span>
          </NavLink>
        );
      })}
    </nav>
  );
}
