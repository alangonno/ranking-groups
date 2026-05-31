import { useState } from "react";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useParams } from "react-router-dom";
import { ArrowUp, Calendar, Trash2, ShieldAlert } from "lucide-react";
import { useUserProfile } from "../../hooks/use-user-profile";
import { useGroup } from "../../hooks/use-groups";
import { useAuthContext } from "../../providers/auth-provider";
import { GroupRole } from "../../types/group/group";
import { AppBadge } from "../../components/ui/app-badge";
import { AppSpinner } from "../../components/ui/app-spinner";
import { postJson } from "../../lib/api";
import { getUserIdFromToken } from "../../lib/auth-token";
import type { EventVoteType } from "../../types/event/event";

function roleLabel(role: GroupRole): string {
  switch (role) {
    case GroupRole.Owner:
      return "Owner";
    case GroupRole.Admin:
      return "Admin";
    case GroupRole.Member:
      return "Member";
    default:
      return "Member";
  }
}

function formatDate(iso: string): string {
  const d = new Date(iso);
  return d.toLocaleDateString("pt-BR", {
    day: "2-digit",
    month: "short",
    year: "numeric",
  });
}

export function ProfilePage() {
  const { groupId, userId } = useParams<{ groupId: string; userId: string }>();
  const { data: group } = useGroup(groupId || "");
  const { data: profile, isLoading } = useUserProfile(groupId || "", userId || "");
  const { user: currentUser } = useAuthContext();

  const member = profile?.member;
  const timeline = profile?.timeline || [];
  const sharedEvents = profile?.sharedEvents || [];
  const currentUserId = getUserIdFromToken() || "";
  const isOwnProfile = currentUserId === userId;

  const queryClient = useQueryClient();

  const requestRemoval = useMutation({
    mutationFn: (eventId: string) =>
      postJson(`/api/events/${eventId}/request-removal`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["user-profile"] });
      queryClient.invalidateQueries({ queryKey: ["events"] });
      queryClient.invalidateQueries({ queryKey: ["ranking"] });
      queryClient.invalidateQueries({ queryKey: ["feed"] });
    },
  });

  const [voteError, setVoteError] = useState<string | null>(null);

  const voteEvent = useMutation({
    mutationFn: ({ eventId, voteType }: { eventId: string; voteType: EventVoteType }) =>
      postJson(`/api/events/${eventId}/vote`, { voteType }),
    onSuccess: () => {
      setVoteError(null);
      queryClient.invalidateQueries({ queryKey: ["user-profile"] });
      queryClient.invalidateQueries({ queryKey: ["events"] });
    },
    onError: (error: any) => {
      if (error?.rule === "duplicate_vote_not_allowed" || error?.message?.includes("já votou")) {
        setVoteError("Você já votou neste evento. Aguarde o resultado da votação.");
      } else {
        setVoteError(error?.message || "Erro ao votar");
      }
    },
  });

  if (isLoading) {
    return (
      <div className="p-4 lg:p-8 max-w-5xl mx-auto">
        <div className="animate-pulse space-y-6">
          <div className="flex gap-4">
            <div className="w-20 h-20 rounded-full bg-gray-200" />
            <div className="flex-1 space-y-2">
              <div className="h-6 bg-gray-200 rounded w-1/3" />
              <div className="h-4 bg-gray-200 rounded w-1/4" />
            </div>
          </div>
          <div className="h-32 bg-gray-200 rounded-xl" />
          <div className="space-y-3">
            {Array.from({ length: 4 }).map((_, i) => (
              <div key={i} className="h-16 bg-gray-200 rounded-lg" />
            ))}
          </div>
        </div>
      </div>
    );
  }

  if (!member) {
    return (
      <div className="p-4 lg:p-8 max-w-5xl mx-auto">
        <div className="text-center py-16">
          <p className="text-text-secondary">Usuário não encontrado neste grupo</p>
        </div>
      </div>
    );
  }

  return (
    <div className="p-4 lg:p-8 max-w-5xl mx-auto">
      {/* Header Mobile */}
      <div className="lg:hidden flex items-center justify-between mb-5">
        <div>
          <h1 className="text-xl font-bold text-text-primary">Perfil</h1>
          <p className="text-sm text-text-secondary">{group?.name || "Grupo"}</p>
        </div>
      </div>

      {/* Header Desktop */}
      <div className="hidden lg:flex items-center justify-between mb-6">
        <div>
          <h1 className="text-2xl font-bold text-text-primary">Perfil</h1>
          <p className="text-sm text-text-secondary">{group?.name || "Grupo"}</p>
        </div>
      </div>

      {/* Hero Section */}
      <div className="flex flex-col lg:flex-row gap-4 mb-8">
        {/* User Card */}
        <div className="flex-1 bg-white rounded-xl border border-border shadow-[0_1px_3px_rgba(0,0,0,0.05)] p-5">
          <div className="flex items-center gap-4">
            <div className="relative">
              <div className="w-20 h-20 rounded-full bg-primary-light flex items-center justify-center text-primary font-bold text-2xl">
                {member.avatar}
              </div>
              <div className="absolute -bottom-1 -right-1 bg-primary text-white text-[10px] font-bold px-1.5 py-0.5 rounded-md">
                LVL {Math.max(1, Math.floor(member.currentScore / 100))}
              </div>
            </div>
            <div className="flex-1 min-w-0">
              <h1 className="text-xl font-bold text-text-primary truncate">
                {member.name}
              </h1>
              <div className="flex items-center gap-2 mt-1">
                <AppBadge color="gray" size="sm">
                  {roleLabel(member.role)}
                </AppBadge>
                {isOwnProfile && (
                  <span className="text-xs text-text-muted">(Você)</span>
                )}
              </div>
              <div className="flex items-center gap-1 mt-2 text-text-muted text-xs">
                <Calendar size={14} />
                <span>Membro do grupo</span>
              </div>
            </div>
          </div>
        </div>

        {/* Score Card */}
        <div className="flex-1 bg-primary rounded-xl p-5 text-white shadow-[0_2px_8px_rgba(0,0,0,0.15)]">
          <p className="text-xs font-medium uppercase tracking-wider opacity-80">
            Total Score Balance
          </p>
          <p className="text-4xl font-bold mt-1">
            {member.currentScore.toLocaleString()}
          </p>
          <div className="flex items-center gap-1 mt-2 text-sm opacity-90">
            <ArrowUp size={16} />
            <span>+450 this week</span>
          </div>
        </div>
      </div>

      {/* Content Grid: Timeline + Shared Events */}
      <div className="flex flex-col lg:flex-row gap-6">
        {/* Timeline */}
        <div className="flex-1 lg:min-w-0">
          <h2 className="text-lg font-bold text-text-primary mb-4">
            Score Timeline
          </h2>
          <div className="bg-white rounded-xl border border-border shadow-[0_1px_3px_rgba(0,0,0,0.05)] p-5">
            {voteError && (
              <div className="mb-4 p-3 bg-error/10 border border-error/20 rounded-lg">
                <p className="text-xs text-error font-medium">{voteError}</p>
              </div>
            )}
            {timeline.length === 0 ? (
              <p className="text-text-secondary text-sm text-center py-8">
                Nenhum evento no timeline
              </p>
            ) : (
              <div className="relative">
                {/* Vertical line */}
                <div className="absolute left-[7px] top-2 bottom-2 w-0.5 bg-border" />
                <div className="space-y-6">
                  {timeline.map((item) => {
                    // Pending removal events -> show inline voting
                    if (item.isPendingRemoval && item.itemType === "event") {
                      const isAffected = item.affectedUserId === currentUserId;
                      const hasVoted = item.approvals?.some(a => a.userId === currentUserId) ?? false;
                      const canVote = !isAffected && !hasVoted;
                      const removeCount = item.removeCount ?? 0;
                      const keepCount = item.keepCount ?? 0;
                      const quorum = item.quorumRequired ?? 5;
                      return (
                        <div key={item.id} className="relative flex gap-4">
                          <div className="w-4 h-4 rounded-full shrink-0 mt-1 bg-amber-500" />
                          <div className="flex-1 min-w-0 bg-amber-50 rounded-lg p-3 border border-amber-200">
                            <div className="flex items-start justify-between gap-2">
                              <div>
                                <p className="text-sm font-medium text-text-primary">
                                  {item.title}
                                </p>
                                <p className="text-xs text-amber-700 mt-0.5 flex items-center gap-1">
                                  <ShieldAlert size={12} />
                                  Remoção em votação ({removeCount}/{quorum} remover, {keepCount}/{quorum} manter)
                                </p>
                              </div>
                              <span className="text-sm font-bold shrink-0 text-amber-700">
                                {item.points}pts
                              </span>
                            </div>
                            {/* Status message above buttons */}
                            {hasVoted && (
                              <p className="text-xs text-success mt-2 font-medium">Voto registrado com sucesso!</p>
                            )}
                            {!canVote && !hasVoted && (
                              <p className="text-xs text-amber-600 mt-2">
                                {isAffected ? "Usuário afetado não pode votar" : "Você não pode votar neste evento"}
                              </p>
                            )}
                            {/* Buttons: always visible, disabled after vote */}
                            <div className="flex gap-2 mt-2">
                              <button
                                type="button"
                                className="flex-1 text-xs py-1.5 px-3 rounded-lg border border-amber-300 text-amber-800 bg-white hover:bg-amber-100 transition-colors disabled:opacity-50 disabled:cursor-not-allowed disabled:bg-gray-100"
                                onClick={() => voteEvent.mutate({ eventId: item.id, voteType: 4 })}
                                disabled={!canVote || voteEvent.isPending}
                              >
                                {voteEvent.isPending ? <AppSpinner size="xs" /> : "Manter"}
                              </button>
                              <button
                                type="button"
                                className="flex-1 text-xs py-1.5 px-3 rounded-lg bg-error text-white hover:opacity-90 transition-opacity disabled:opacity-50 disabled:cursor-not-allowed disabled:bg-gray-400"
                                onClick={() => voteEvent.mutate({ eventId: item.id, voteType: 3 })}
                                disabled={!canVote || voteEvent.isPending}
                              >
                                {voteEvent.isPending ? <AppSpinner size="xs" /> : "Remover"}
                              </button>
                            </div>
                            <div className="mt-2">
                              <p className="text-xs text-text-muted">
                                Balance:{" "}
                                <span className="font-medium text-text-primary">
                                  {item.scoreBalance.toLocaleString()}
                                </span>
                              </p>
                            </div>
                          </div>
                        </div>
                      );
                    }

                    // Shared events -> blue dot
                    if (item.itemType === "shared_event") {
                      return (
                        <div key={item.id} className="relative flex gap-4">
                          <div className="w-4 h-4 rounded-full shrink-0 mt-1 bg-blue-500" />
                          <div className="flex-1 min-w-0">
                            <div className="flex items-start justify-between gap-2">
                              <div>
                                <p className="text-sm font-medium text-text-primary">
                                  {item.title}
                                </p>
                                <p className="text-xs text-text-muted mt-0.5">
                                  {formatDate(item.createdAt)}
                                </p>
                              </div>
                              <span className="text-sm font-bold shrink-0 text-blue-600">
                                +{item.points}
                              </span>
                            </div>
                            <div className="mt-2 bg-gray-50 rounded-lg px-3 py-2 border border-border">
                              <p className="text-xs text-text-muted">
                                Balance:{" "}
                                <span className="font-medium text-text-primary">
                                  {item.scoreBalance.toLocaleString()}
                                </span>
                              </p>
                            </div>
                          </div>
                        </div>
                      );
                    }

                    // Regular events (approved, not pending removal)
                    const isPositive = item.type === "Positive";
                    const signedPoints = isPositive ? item.points : -item.points;
                    return (
                      <div key={item.id} className="relative flex gap-4">
                        <div
                          className={`w-4 h-4 rounded-full shrink-0 mt-1 ${isPositive ? "bg-primary" : "bg-text-muted"}`}
                        />
                        <div className="flex-1 min-w-0">
                          <div className="flex items-start justify-between gap-2">
                            <div>
                              <p className="text-sm font-medium text-text-primary">
                                {item.title}
                              </p>
                              <p className="text-xs text-text-muted mt-0.5">
                                {item.createdAt}
                              </p>
                            </div>
                            <span
                              className={`text-sm font-bold shrink-0 ${isPositive ? "text-primary" : "text-text-muted"}`}
                            >
                              {isPositive ? "+" : ""}
                              {signedPoints}
                            </span>
                          </div>
                          <div className="mt-2 bg-gray-50 rounded-lg px-3 py-2 border border-border">
                            <div className="flex items-center justify-between">
                              <p className="text-xs text-text-muted">
                                Balance:{" "}
                                <span className="font-medium text-text-primary">
                                  {item.scoreBalance.toLocaleString()}
                                </span>
                              </p>
                              <button
                                type="button"
                                className="flex items-center gap-1 text-xs text-error hover:text-error/80 font-medium transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                                onClick={() => requestRemoval.mutate(item.id)}
                                disabled={requestRemoval.isPending}
                              >
                                {requestRemoval.isPending ? (
                                  <AppSpinner size="xs" />
                                ) : (
                                  <Trash2 size={12} />
                                )}
                                Remover
                              </button>
                            </div>
                          </div>
                        </div>
                      </div>
                    );
                  })}
                </div>
              </div>
            )}
          </div>
        </div>

        {/* Shared Events */}
        <div className="w-full lg:w-[40%] lg:shrink-0">
          <h2 className="text-lg font-bold text-text-primary mb-4">
            Shared Events
          </h2>
          <div className="space-y-4">
            {sharedEvents.length === 0 ? (
              <div className="bg-white rounded-xl border border-border shadow-[0_1px_3px_rgba(0,0,0,0.05)] p-5 text-center">
                <p className="text-text-secondary text-sm">
                  Nenhum shared event
                </p>
              </div>
            ) : (
              sharedEvents.map((se) => (
                <div
                  key={se.id}
                  className="relative bg-gray-100 rounded-xl overflow-hidden aspect-video flex flex-col justify-end"
                >
                  {/* Gradient overlay */}
                  <div className="absolute inset-0 bg-gradient-to-t from-black/70 via-black/20 to-transparent" />
                  {/* Content */}
                  <div className="relative p-4 text-white">
                    <div className="flex items-center gap-2 mb-2">
                      <AppBadge
                        color={se.userRole === "organizer" ? "red" : "gray"}
                        size="sm"
                      >
                        {se.userRole === "organizer" ? "ORGANIZER" : "PARTICIPANT"}
                      </AppBadge>
                    </div>
                    <h3 className="text-sm font-bold">{se.title}</h3>
                    <div className="flex items-center justify-between mt-1">
                      <span className="text-xs opacity-80">
                        {se.participantCount} participants
                      </span>
                      <span className="text-xs font-bold">
                        +{se.points} pts
                      </span>
                    </div>
                  </div>
                </div>
              ))
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
