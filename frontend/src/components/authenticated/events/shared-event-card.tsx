import { useState } from "react";
import { Users, ArrowUp, CheckCircle2, Clock, LogOut } from "lucide-react";
import { AppButton } from "../../ui/app-button";
import { AppSpinner } from "../../ui/app-spinner";
import { useJoinSharedEvent, useLeaveSharedEvent } from "../../../hooks/use-shared-events";
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
  const leaveEvent = useLeaveSharedEvent(event.id);
  const currentUserId = getUserIdFromToken();
  const isCreator = currentUserId === event.createdByUserId;
  const [hasJoined, setHasJoined] = useState(event.hasCurrentUserJoined);
  const [hasLeft, setHasLeft] = useState(false);

  function formatClosesAt(dateString: string) {
    const date = new Date(dateString);
    return date.toLocaleDateString("pt-BR", {
      day: "numeric",
      month: "short",
      hour: "2-digit",
      minute: "2-digit",
    });
  }

  function handleJoin() {
    if (hasJoined || joinEvent.isPending) return;
    joinEvent.mutate(undefined, {
      onSuccess: () => setHasJoined(true),
    });
  }

  function handleLeave() {
    if (hasLeft || leaveEvent.isPending) return;
    leaveEvent.mutate(undefined, {
      onSuccess: () => setHasLeft(true),
    });
  }

  const isActionDisabled = hasJoined || hasLeft || joinEvent.isPending || leaveEvent.isPending;

  return (
    <div className="bg-surface-container-lowest shadow-sm rounded-2xl min-w-[260px] snap-start overflow-hidden border border-surface-container group cursor-pointer hover:border-outline-variant transition-colors">
      {event.image ? (
        <div className="h-24 bg-gradient-to-r from-blue-400 to-purple-500 relative">
          <span className="absolute top-2 left-2 bg-surface/90 backdrop-blur-sm text-on-surface text-[10px] font-bold px-2 py-1 rounded-full">
            Em breve
          </span>
        </div>
      ) : (
        <div className="h-24 bg-surface-variant flex items-center justify-center">
          <Users size={32} className="text-secondary" />
        </div>
      )}
      <div className="p-5">
        <div className="flex items-start justify-between gap-2 mb-2">
          <h3 className="text-headline-md font-headline-md text-on-surface truncate">
            {event.title}
          </h3>
          {event.isClosed && (
            <span className="flex-shrink-0 inline-flex items-center gap-0.5 bg-surface-container text-secondary text-caption font-caption px-2 py-0.5 rounded-full">
              <CheckCircle2 size={12} />
              Encerrado
            </span>
          )}
        </div>

        <div className="bg-primary-container/10 text-primary px-2 py-1 rounded text-caption font-caption font-bold inline-flex items-center gap-0.5">
          <ArrowUp size={12} />
          +{event.points} Pts
        </div>

        {event.closesAt && !event.isClosed && (
          <p className="flex items-center gap-1 text-caption font-caption text-secondary mt-2">
            <Clock size={12} />
            Fecha em {formatClosesAt(event.closesAt)}
          </p>
        )}

        <div className="flex items-center justify-between mt-4 pt-4 border-t border-surface-container">
          <div className="flex items-center gap-1 text-caption font-caption text-secondary">
            <Users size={14} />
            <span>{event.participantCount} confirmados</span>
          </div>
          <div className="flex gap-1.5">
            {!event.isClosed && hasJoined && (
              <AppButton
                size="xs"
                color="light"
                className="text-xs px-3 py-1.5"
                onClick={handleLeave}
                disabled={isActionDisabled}
              >
                {leaveEvent.isPending ? (
                  <AppSpinner size="xs" />
                ) : (
                  <LogOut size={12} />
                )}
                {leaveEvent.isPending ? "Saindo..." : "Sair"}
              </AppButton>
            )}
            {!event.isClosed && !hasJoined && !hasLeft && (
              <AppButton
                size="xs"
                color="primary"
                className="text-xs px-3 py-1.5"
                onClick={handleJoin}
                disabled={isActionDisabled}
              >
                {joinEvent.isPending ? (
                  <AppSpinner size="xs" />
                ) : (
                  "Participar"
                )}
              </AppButton>
            )}
            {hasLeft && (
              <span className="text-xs text-secondary">Removido</span>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
