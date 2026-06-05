import { Link } from "react-router-dom";
import { ChevronRight, Users } from "lucide-react";
import type { Group } from "../../../types/group/group";

interface GroupCardProps {
  group: Group;
  memberCount?: number;
  isHighlighted?: boolean;
}

export function GroupCard({ group, memberCount, isHighlighted = false }: GroupCardProps) {
  const initials = group.name
    .split(" ")
    .map((n) => n[0])
    .join("")
    .toUpperCase()
    .slice(0, 2);

  return (
    <div className="bg-surface-container-lowest rounded-xl p-5 shadow-sm border border-surface-container hover:scale-[0.99] transition-transform duration-200 cursor-pointer">
      <Link to={`/group/${group.id}`} className="flex items-center gap-3">
        <div className={`w-12 h-12 rounded-full flex items-center justify-center font-bold text-lg flex-shrink-0 ${
          isHighlighted ? "bg-primary text-white" : "bg-gray-100 text-text-secondary"
        }`}>
          {initials}
        </div>
        <div className="flex-1 min-w-0">
          <h3 className="text-sm font-semibold text-text-primary truncate">
            {group.name}
          </h3>
          <p className="text-xs text-text-secondary truncate">
            {group.description}
          </p>
          {memberCount !== undefined && (
            <div className="flex items-center gap-2 mt-1 text-xs text-text-muted">
              <Users size={12} />
              <span>{memberCount} membros</span>
            </div>
          )}
        </div>
        <ChevronRight size={18} className="text-text-muted flex-shrink-0" />
      </Link>
    </div>
  );
}
