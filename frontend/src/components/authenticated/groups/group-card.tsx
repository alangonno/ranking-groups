import { useState, useCallback } from "react";
import { Link } from "react-router-dom";
import { ChevronRight, Users, Copy, Check } from "lucide-react";
import type { Group } from "../../../types/group/group";

interface GroupCardProps {
  group: Group;
  memberCount?: number;
  isHighlighted?: boolean;
}

export function GroupCard({ group, memberCount, isHighlighted = false }: GroupCardProps) {
  const [hasCopied, setHasCopied] = useState(false);

  const initials = group.name
    .split(" ")
    .map((n) => n[0])
    .join("")
    .toUpperCase()
    .slice(0, 2);

  const handleCopy = useCallback(async (e: React.MouseEvent) => {
    e.preventDefault();
    e.stopPropagation();
    
    if (!group.inviteCode) return;

    try {
      await navigator.clipboard.writeText(group.inviteCode);
      setHasCopied(true);
      setTimeout(() => setHasCopied(false), 2000);
    } catch (err) {
      console.error("Failed to copy invite code:", err);
    }
  }, [group.inviteCode]);

  return (
    <div className="bg-surface-container-lowest rounded-xl p-5 shadow-sm border border-surface-container hover:scale-[0.99] transition-transform duration-200 cursor-pointer flex items-center justify-between gap-4">
      <Link to={`/group/${group.id}`} className="flex items-center gap-3 flex-1 min-w-0">
        <div className={`w-12 h-12 rounded-full flex items-center justify-center font-bold text-lg flex-shrink-0 ${
          isHighlighted ? "bg-primary text-on-primary" : "bg-surface-container-low dark:bg-surface-container text-text-secondary"
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

      <div className="flex flex-col items-end gap-1 flex-shrink-0">
        <span className="text-[10px] text-text-muted uppercase tracking-wider font-medium">
          Código
        </span>
        <div className="flex items-center gap-2">
          <span className="text-xs font-mono text-text-secondary bg-surface-container-low dark:bg-surface-container px-2 py-1 rounded">
            {group.inviteCode}
          </span>
          <button
            onClick={handleCopy}
            className="p-1.5 rounded-md hover:bg-surface-container-low dark:bg-surface-container transition-colors"
            title="Copiar código"
          >
            {hasCopied ? (
              <Check size={16} className="text-green-600" />
            ) : (
              <Copy size={16} className="text-text-muted" />
            )}
          </button>
        </div>
      </div>
    </div>
  );
}
