import { AppCard } from "../../ui/app-card";
import { AppTooltip } from "../../ui/app-tooltip";
import { ArrowUp } from "lucide-react";
import type { Event } from "../../../types/event/event";

interface EventCardProps {
  event: Event;
}

function getRelativeTime(dateString: string): string {
  const date = new Date(dateString);
  const now = new Date();
  const diffMs = now.getTime() - date.getTime();
  const diffHours = Math.floor(diffMs / (1000 * 60 * 60));
  const diffDays = Math.floor(diffHours / 24);

  if (diffHours < 1) return "Agora mesmo";
  if (diffHours < 24) return `Há ${diffHours}h`;
  if (diffDays < 7) return `Há ${diffDays} dias`;
  return date.toLocaleDateString("pt-BR");
}

export function EventCard({ event }: EventCardProps) {
  const creator = event.createdByUser;
  const initials = creator?.name
    ? creator.name
        .split(" ")
        .map((n) => n[0])
        .join("")
        .toUpperCase()
        .slice(0, 2)
    : "??";

  return (
    <AppCard className="shadow-[0_1px_3px_rgba(0,0,0,0.05)] hover:shadow-md transition-shadow p-5">
      <div className="flex items-start gap-3">
        {/* Avatar - Círculo perfeito */}
        <AppTooltip content={creator?.name || "Usuário"}>
          <div className="w-10 h-10 rounded-full bg-primary-light flex items-center justify-center text-primary font-bold text-sm flex-shrink-0 cursor-pointer">
            {initials}
          </div>
        </AppTooltip>

        <div className="flex-1 min-w-0">
          {/* Header */}
          <div className="flex items-start justify-between gap-2">
            <div className="min-w-0">
              <p className="text-sm font-medium text-text-primary">
                <span className="font-semibold">{creator?.name}</span>{" "}
                <span className="text-text-secondary">registrou um evento</span>
              </p>
              <time className="text-xs text-text-muted" dateTime={event.createdAt}>
                {getRelativeTime(event.createdAt)}
              </time>
            </div>

            {/* Points Badge - Pill style */}
            <span className="flex-shrink-0 inline-flex items-center gap-0.5 bg-red-50 text-red-800 text-xs font-medium px-2.5 py-1 rounded-full">
              <ArrowUp size={12} />
              +{event.points}pts
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
