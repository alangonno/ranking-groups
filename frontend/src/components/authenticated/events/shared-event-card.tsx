import { Users, ArrowUp, CheckCircle2, Clock } from "lucide-react";
import { AppCard } from "../../ui/app-card";
import { AppButton } from "../../ui/app-button";
import { useJoinSharedEvent, useCloseSharedEvent } from "../../../hooks/use-shared-events";
import { getUserIdFromToken } from "../../../lib/auth-token";

interface SharedEvent {
  id: string;
  title: string;
  points: number;
  participantCount: number;
  isClosed: boolean;
  createdByUserId: string;
  hasCurrentUserJoined: boolean;
  closesAt?: string;
  image?: string;
}

interface SharedEventCardProps {
  event: SharedEvent;
}

export function SharedEventCard({ event }: SharedEventCardProps) {
  const joinEvent = useJoinSharedEvent(event.id);
  const closeEvent = useCloseSharedEvent(event.id);
  const currentUserId = getUserIdFromToken();
  const isCreator = currentUserId === event.createdByUserId;

  function formatClosesAt(dateString: string) {
    const date = new Date(dateString);
    return date.toLocaleDateString("pt-BR", {
      day: "numeric",
      month: "short",
      hour: "2-digit",
      minute: "2-digit",
    });
  }

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
        <div className="flex items-start justify-between gap-2">
          <h3 className="text-sm font-semibold text-text-primary truncate">
            {event.title}
          </h3>
          {event.isClosed && (
            <span className="flex-shrink-0 inline-flex items-center gap-0.5 bg-gray-100 text-text-muted text-[10px] font-medium px-2 py-0.5 rounded-full">
              <CheckCircle2 size={10} />
              Encerrado
            </span>
          )}
        </div>
        <span className="inline-flex items-center gap-0.5 bg-green-50 text-green-800 text-xs font-medium px-2.5 py-1 rounded-full mt-1">
          <ArrowUp size={12} />
          +{event.points}pts
        </span>
        {event.closesAt && !event.isClosed && (
          <p className="flex items-center gap-1 text-[10px] text-text-muted mt-1">
            <Clock size={10} />
            Fecha em {formatClosesAt(event.closesAt)}
          </p>
        )}
        <div className="flex items-center justify-between mt-3">
          <div className="flex items-center gap-1 text-xs text-text-muted">
            <Users size={12} />
            <span>{event.participantCount} confirmados</span>
          </div>
          <div className="flex gap-1.5">
            {isCreator && !event.isClosed && (
              <AppButton
                size="xs"
                color="light"
                className="text-xs px-3 py-1.5"
                onClick={() => closeEvent.mutate()}
                disabled={closeEvent.isPending}
              >
                {closeEvent.isPending ? "Finalizando..." : "Finalizar"}
              </AppButton>
            )}
            {!event.isClosed && (
              <AppButton
                size="xs"
                color="light"
                className="text-xs px-3 py-1.5"
                onClick={() => joinEvent.mutate()}
                disabled={joinEvent.isPending || event.hasCurrentUserJoined}
              >
                {joinEvent.isPending
                  ? "Entrando..."
                  : event.hasCurrentUserJoined
                  ? "Participando"
                  : "Participar"}
              </AppButton>
            )}
          </div>
        </div>
      </div>
    </AppCard>
  );
}
