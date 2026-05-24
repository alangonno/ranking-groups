import { Link } from "react-router-dom";
import { ChevronRight, Users } from "lucide-react";
import { AppCard } from "../../ui/app-card";
import type { Group } from "../../../types/group/group";

interface GroupCardProps {
  group: Group;
  memberCount?: number;
  isHighlighted?: boolean;
}

export function GroupCard({ group, memberCount = 12, isHighlighted = false }: GroupCardProps) {
  const initials = group.name
    .split(" ")
    .map((n) => n[0])
    .join("")
    .toUpperCase()
    .slice(0, 2);

  if (isHighlighted) {
    return (
      <AppCard className="shadow-[0_1px_3px_rgba(0,0,0,0.05)] p-0 overflow-hidden hover:shadow-md transition-shadow cursor-pointer">
        <Link to={`/group/${group.id}`} className="block">
          <div className="relative bg-gradient-to-br from-primary/10 to-primary/5 p-5">
            <div className="flex items-start gap-4">
              <div className="w-14 h-14 rounded-full bg-primary text-white flex items-center justify-center font-bold text-xl flex-shrink-0">
                {initials}
              </div>
              <div className="flex-1 min-w-0">
                <div className="flex items-center gap-2 mb-1">
                  <span className="bg-primary text-white text-[10px] font-bold px-2 py-0.5 rounded-full uppercase tracking-wider">
                    Ativo
                  </span>
                </div>
                <h3 className="text-lg font-bold text-text-primary truncate">
                  {group.name}
                </h3>
                <p className="text-sm text-text-secondary mt-1 line-clamp-2">
                  {group.description}
                </p>
                <div className="flex items-center gap-2 mt-3 text-xs text-text-muted">
                  <Users size={14} />
                  <span>{memberCount} membros</span>
                </div>
              </div>
              <ChevronRight size={20} className="text-text-muted flex-shrink-0 mt-1" />
            </div>
          </div>
        </Link>
      </AppCard>
    );
  }

  return (
    <AppCard className="shadow-[0_1px_3px_rgba(0,0,0,0.05)] p-4 hover:shadow-md transition-shadow cursor-pointer">
      <Link to={`/group/${group.id}`} className="flex items-center gap-3">
        <div className="w-12 h-12 rounded-full bg-gray-100 flex items-center justify-center text-text-secondary font-bold text-lg flex-shrink-0">
          {initials}
        </div>
        <div className="flex-1 min-w-0">
          <h3 className="text-sm font-semibold text-text-primary truncate">
            {group.name}
          </h3>
          <p className="text-xs text-text-secondary truncate">
            {group.description}
          </p>
          <div className="flex items-center gap-2 mt-1 text-xs text-text-muted">
            <Users size={12} />
            <span>{memberCount} membros</span>
          </div>
        </div>
        <div className="text-right flex-shrink-0">
          <span className="text-xs font-medium text-text-secondary">
            Rank #{Math.floor(Math.random() * 50) + 1}
          </span>
        </div>
        <ChevronRight size={18} className="text-text-muted flex-shrink-0" />
      </Link>
    </AppCard>
  );
}
