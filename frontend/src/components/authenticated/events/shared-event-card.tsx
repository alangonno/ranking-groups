import { Users } from "lucide-react";
import { AppCard } from "../../ui/app-card";
import { AppButton } from "../../ui/app-button";

interface SharedEvent {
  id: string;
  title: string;
  points: number;
  participantCount: number;
  image?: string;
}

interface SharedEventCardProps {
  event: SharedEvent;
}

export function SharedEventCard({ event }: SharedEventCardProps) {
  return (
    <AppCard className="shadow-[0_1px_3px_rgba(0,0,0,0.05)] min-w-[260px] snap-start p-0 overflow-hidden">
      {event.image ? (
        <div className="h-24 bg-gradient-to-r from-blue-400 to-purple-500 relative">
          <span className="absolute top-2 left-2 bg-white/90 text-text-primary text-[10px] font-bold px-2 py-1 rounded-full">
            Em breve
          </span>
        </div>
      ) : (
        <div className="h-24 bg-blue-50 flex items-center justify-center">
          <Users size={32} className="text-blue-400" />
        </div>
      )}
      <div className="p-4">
        <h3 className="text-sm font-semibold text-text-primary truncate">
          {event.title}
        </h3>
        <p className="text-xs text-primary font-medium mt-1">
          +{event.points} pts
        </p>
        <div className="flex items-center justify-between mt-3">
          <div className="flex items-center gap-1 text-xs text-text-muted">
            <Users size={12} />
            <span>{event.participantCount} confirmados</span>
          </div>
          <AppButton size="xs" color="light" className="text-xs px-3 py-1.5">
            Participar
          </AppButton>
        </div>
      </div>
    </AppCard>
  );
}
