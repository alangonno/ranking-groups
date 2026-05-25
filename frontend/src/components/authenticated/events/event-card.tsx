import { AppCard } from "../../ui/app-card";
import { AppTooltip } from "../../ui/app-tooltip";
import { ArrowUp, ArrowDown } from "lucide-react";
import { EventType } from "../../../types/event/event";
import type { Event } from "../../../types/event/event";
import { formatRelativeTime } from "../../../lib/format-time";

interface EventCardProps {
  event: Event;
}

export function EventCard({ event }: EventCardProps) {
  const isPositive = event.type === EventType.Positive;
  const affected = event.affectedUser;
  const creator = event.createdByUser;
  const initials = affected?.name
    ? affected.name
        .split(" ")
        .map((n) => n[0])
        .join("")
        .toUpperCase()
        .slice(0, 2)
    : "??";

  return (
    <AppCard className="shadow-[0_1px_3px_rgba(0,0,0,0.05)] hover:shadow-md transition-shadow p-5">
      <div className="flex items-start gap-3">
        {/* Avatar - Usuário afetado */}
        <AppTooltip content={affected?.name || "Usuário"}>
          <div className="w-10 h-10 rounded-full bg-primary-light flex items-center justify-center text-primary font-bold text-sm flex-shrink-0 cursor-pointer">
            {initials}
          </div>
        </AppTooltip>

        <div className="flex-1 min-w-0">
          {/* Header */}
          <div className="flex items-start justify-between gap-2">
            <div className="min-w-0">
              <p className="text-sm font-semibold text-text-primary">
                {affected?.name || "Usuário"}
              </p>
              <p className="text-xs text-text-muted">
                registrado por {creator?.name} • {formatRelativeTime(event.createdAt)}
              </p>
            </div>

            {/* Points Badge - Pill style */}
            <span className={`flex-shrink-0 inline-flex items-center gap-0.5 text-xs font-medium px-2.5 py-1 rounded-full ${
              isPositive ? "bg-green-50 text-green-800" : "bg-red-50 text-red-800"
            }`}>
              {isPositive ? <ArrowUp size={12} /> : <ArrowDown size={12} />}
              {isPositive ? "+" : "-"}{event.points}pts
            </span>
          </div>

          {/* Content */}
          <div className="mt-3 bg-gray-50 rounded-lg p-3">
            <h3 className="text-sm font-semibold text-text-primary">{event.title}</h3>
            <p className="text-sm text-text-secondary mt-1">{event.description}</p>
          </div>
        </div>
      </div>
    </AppCard>
  );
}
