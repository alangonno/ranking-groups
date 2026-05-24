import { useState } from "react";
import { AppCard } from "../../ui/app-card";
import { AppSpinner } from "../../ui/app-spinner";
import { AppTooltip } from "../../ui/app-tooltip";
import { ArrowDown, ClipboardList } from "lucide-react";
import type { Event } from "../../../types/event/event";

interface VotingCardProps {
  event: Event;
  compact?: boolean;
}

export function VotingCard({ event, compact = false }: VotingCardProps) {
  const [isVoting, setIsVoting] = useState(false);
  const [hasVoted, setHasVoted] = useState(false);

  const affectedUser = event.affectedUser;
  const creator = event.createdByUser;
  const votesCount = event.approvals?.length || 0;
  const quorumNeeded = 5;

  const isCreator = creator?.id === "user-001";
  const isAffected = affectedUser?.id === "user-001";
  const canVote = !isCreator && !isAffected && !hasVoted;

  function handleVote(_type: "confirm" | "reject") {
    if (!canVote || isVoting) return;
    setIsVoting(true);
    setTimeout(() => {
      setIsVoting(false);
      setHasVoted(true);
    }, 1000);
  }

  const initials = affectedUser?.name
    ? affectedUser.name
        .split(" ")
        .map((n) => n[0])
        .join("")
        .toUpperCase()
        .slice(0, 2)
    : "??";

  if (compact) {
    return (
      <AppCard className="shadow-[0_1px_3px_rgba(0,0,0,0.05)] min-w-[260px] snap-start p-4">
        <div className="flex items-start gap-2">
          <div className="w-8 h-8 rounded-full bg-gray-100 flex items-center justify-center text-text-secondary font-bold text-xs flex-shrink-0">
            {initials}
          </div>
          <div className="flex-1 min-w-0">
            <p className="text-sm font-medium text-text-primary truncate">{event.title}</p>
            <p className="text-xs text-text-muted truncate">
              Validar {affectedUser?.name}
            </p>
            <div className="flex gap-2 mt-2">
              <button
                type="button"
                className="flex-1 text-xs py-1.5 px-3 rounded-lg border border-gray-300 text-text-primary bg-white hover:bg-gray-50 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                onClick={() => handleVote("reject")}
                disabled={!canVote || isVoting}
              >
                {isVoting ? <AppSpinner size="xs" /> : "Negar"}
              </button>
              <button
                type="button"
                className="flex-1 text-xs py-1.5 px-3 rounded-lg bg-primary text-white hover:bg-primary-hover transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                onClick={() => handleVote("confirm")}
                disabled={!canVote || isVoting}
              >
                {isVoting ? <AppSpinner size="xs" /> : "Aprovar"}
              </button>
            </div>
          </div>
        </div>
      </AppCard>
    );
  }

  return (
    <AppCard className="shadow-[0_1px_3px_rgba(0,0,0,0.05)] p-5">
      <div className="flex items-start gap-3">
        {/* Avatar */}
        <AppTooltip content={affectedUser?.name || "Usuário afetado"}>
          <div className="w-10 h-10 rounded-full bg-gray-100 flex items-center justify-center text-text-secondary font-bold text-sm flex-shrink-0 cursor-pointer">
            {initials}
          </div>
        </AppTooltip>

        <div className="flex-1 min-w-0">
          {/* Header */}
          <div className="flex items-start justify-between gap-2">
            <div>
              <p className="text-sm font-medium text-text-primary">
                <span className="font-semibold">{creator?.name}</span>{" "}
                <span className="text-text-secondary">registrou uma infração</span>
              </p>
              <p className="text-xs text-text-muted mt-0.5">
                Contra: {affectedUser?.name}
              </p>
            </div>

            {/* Points Badge - Pill style */}
            <span className="flex-shrink-0 inline-flex items-center gap-0.5 bg-red-50 text-red-800 text-xs font-medium px-2.5 py-1 rounded-full">
              <ArrowDown size={12} />
              -{event.points}pts
            </span>
          </div>

          {/* Content */}
          <div className="mt-3 bg-gray-50 rounded-lg p-3">
            <h3 className="text-sm font-semibold text-text-primary">{event.title}</h3>
            <p className="text-sm text-text-secondary mt-1">{event.description}</p>
          </div>

          {/* Voting Section */}
          <div className="mt-3 bg-gray-100 rounded-lg p-4">
            <div className="flex items-center gap-2 mb-3">
              <ClipboardList size={16} className="text-text-muted" />
              <span className="text-xs font-medium text-text-secondary">
                Votação em andamento ({votesCount}/{quorumNeeded} votos)
              </span>
            </div>

            {!canVote && !hasVoted && (
              <p className="text-xs text-text-muted mb-3">
                Você não pode votar neste evento
              </p>
            )}

            {hasVoted && (
              <p className="text-xs text-success mb-3 font-medium">
                Voto registrado com sucesso!
              </p>
            )}

            <div className="grid grid-cols-2 gap-3">
              <button
                type="button"
                className="w-full py-2.5 px-4 rounded-lg border border-gray-300 text-text-primary bg-white hover:bg-gray-50 font-medium text-sm transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                onClick={() => handleVote("reject")}
                disabled={!canVote || isVoting}
              >
                {isVoting ? <AppSpinner size="sm" /> : "Rejeitar"}
              </button>
              <button
                type="button"
                className="w-full py-2.5 px-4 rounded-lg bg-primary text-white hover:bg-primary-hover font-medium text-sm transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                onClick={() => handleVote("confirm")}
                disabled={!canVote || isVoting}
              >
                {isVoting ? <AppSpinner size="sm" /> : "Confirmar"}
              </button>
            </div>
          </div>
        </div>
      </div>
    </AppCard>
  );
}
