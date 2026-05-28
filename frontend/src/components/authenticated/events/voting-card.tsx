import { useState } from "react";
import { AppSpinner } from "../../ui/app-spinner";
import { AppTooltip } from "../../ui/app-tooltip";
import { ArrowDown, ClipboardList } from "lucide-react";
import type { Event, EventVoteType } from "../../../types/event/event";
import { useVoteEvent } from "../../../hooks/use-events";
import { getUserIdFromToken } from "../../../lib/auth-token";

interface VotingCardProps {
  event: Event;
  compact?: boolean;
}

export function VotingCard({ event, compact = false }: VotingCardProps) {
  const [hasVoted, setHasVoted] = useState(false);
  const vote = useVoteEvent(event.id);

  const affectedUser = event.affectedUser;
  const creator = event.createdByUser;
  const votesCount = event.approvals?.length || 0;
  const quorumNeeded = 5;

  const currentUserId = getUserIdFromToken() || "";
  const isCreator = creator?.id === currentUserId;
  const isAffected = affectedUser?.id === currentUserId;
  const canVote = !isCreator && !isAffected && !hasVoted;

  function handleVote(type: "confirm" | "reject") {
    if (!canVote || vote.isPending) return;
    vote.mutate(
      { voteType: type === "confirm" ? 1 : 2 as EventVoteType },
      { onSuccess: () => setHasVoted(true) }
    );
  }

  const initials = affectedUser?.name
    ? affectedUser.name
        .split(" ")
        .map((n) => n[0])
        .join("")
        .toUpperCase()
        .slice(0, 2)
    : "??";

  const progressPercent = Math.min((votesCount / quorumNeeded) * 100, 100);

  if (compact) {
    return (
      <div className="shadow-[0_1px_3px_rgba(0,0,0,0.05)] min-w-[260px] snap-start p-4 bg-surface-container-lowest rounded-xl border border-surface-container">
        <div className="flex items-start gap-2">
          <div className="w-8 h-8 rounded-full bg-surface-container flex items-center justify-center text-secondary font-bold text-xs flex-shrink-0">
            {initials}
          </div>
          <div className="flex-1 min-w-0">
            <p className="text-sm font-medium text-on-surface truncate">{event.title}</p>
            <p className="text-xs text-secondary truncate">
              Validar {affectedUser?.name}
            </p>
            <div className="flex gap-2 mt-2">
              <button
                type="button"
                className="flex-1 text-xs py-1.5 px-3 rounded-lg border border-surface-container text-on-surface bg-surface-container-lowest hover:bg-surface-container-low transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                onClick={() => handleVote("reject")}
                disabled={!canVote || vote.isPending}
              >
                {vote.isPending ? <AppSpinner size="xs" /> : "Negar"}
              </button>
              <button
                type="button"
                className="flex-1 text-xs py-1.5 px-3 rounded-lg bg-primary text-on-primary hover:opacity-90 transition-opacity disabled:opacity-50 disabled:cursor-not-allowed"
                onClick={() => handleVote("confirm")}
                disabled={!canVote || vote.isPending}
              >
                {vote.isPending ? <AppSpinner size="xs" /> : "Aprovar"}
              </button>
            </div>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="bg-surface-container-lowest rounded-xl p-5 shadow-sm border border-outline-variant/50 relative overflow-hidden hover:scale-[0.99] transition-transform duration-200">
      {/* Progress bar at top */}
      <div className="absolute top-0 left-0 w-full h-1 bg-surface-container">
        <div
          className="h-full bg-error rounded-full transition-all duration-500"
          style={{ width: `${progressPercent}%` }}
        />
      </div>

      <div className="flex items-start gap-4 mt-1">
        <div className="flex-1 min-w-0">
          <div className="flex items-start justify-between gap-2">
            <div>
              <p className="text-body-md font-body-md text-on-surface">
                <span className="font-semibold">{affectedUser?.name}</span>
              </p>
              <p className="text-caption font-caption text-secondary mt-0.5">
                registrado por {creator?.name}
              </p>
            </div>

            <span className="flex-shrink-0 inline-flex items-center gap-0.5 bg-error-container text-on-error-container text-caption font-caption px-2.5 py-1 rounded-full">
              <ArrowDown size={12} />
              -{event.points}pts
            </span>
          </div>

            <div className="mt-3 bg-surface-bright rounded-xl p-4 border border-surface-container-highest">
            <h3 className="text-label-bold font-label-bold text-on-surface">{event.title}</h3>
            <p className="text-body-md font-body-md text-secondary mt-1">{event.description}</p>
          </div>

          <div className="mt-3 bg-surface-container-low rounded-xl p-4">
            <div className="flex items-center gap-2 mb-3">
              <ClipboardList size={16} className="text-secondary" />
              <span className="text-caption font-caption text-secondary">
                Validação Pendente ({votesCount}/{quorumNeeded} votos)
              </span>
            </div>

            {!canVote && !hasVoted && (
              <p className="text-caption font-caption text-secondary mb-3">
                Você não pode votar neste evento
              </p>
            )}

            {hasVoted && (
              <p className="text-caption font-caption text-success mb-3 font-medium">
                Voto registrado com sucesso!
              </p>
            )}

            <div className="grid grid-cols-2 gap-3">
              <button
                type="button"
                className="w-full py-2.5 px-4 rounded-full bg-surface-container-lowest border border-surface-container-highest text-on-surface text-label-bold font-label-bold hover:bg-surface-container-low transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                onClick={() => handleVote("reject")}
                disabled={!canVote || vote.isPending}
              >
                {vote.isPending ? <AppSpinner size="sm" /> : "Rejeitar"}
              </button>
              <button
                type="button"
                className="w-full py-2.5 px-4 rounded-full bg-error text-on-error text-label-bold font-label-bold hover:opacity-90 transition-opacity disabled:opacity-50 disabled:cursor-not-allowed"
                onClick={() => handleVote("confirm")}
                disabled={!canVote || vote.isPending}
              >
                {vote.isPending ? <AppSpinner size="sm" /> : "Confirmar"}
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
