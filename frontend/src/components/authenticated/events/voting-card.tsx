import { useMemo, useState } from "react";
import { ImageModal } from "../../ui/image-modal";
import { AppSpinner } from "../../ui/app-spinner";
import { ArrowDown, ClipboardList, Trash2, Clock, MessageCircle } from "lucide-react";
import type { Event, EventVoteType } from "../../../types/event/event";
import { useVoteEvent } from "../../../hooks/use-events";
import { getUserIdFromToken } from "../../../lib/auth-token";
import { authStore } from "../../../store/auth-store";
import { CommentsSection } from "./comments-section";
import { useEventComments, useCreateEventComment } from "../../../hooks/use-comments";
import { useCurrentGroupId } from "../../../lib/use-group-context";

interface VotingCardProps {
  event: Event;
  compact?: boolean;
}

export function VotingCard({ event, compact = false }: VotingCardProps) {
  const vote = useVoteEvent(event.id, event.groupId);
  const [showComments, setShowComments] = useState(false);
  const groupId = useCurrentGroupId();

  const {
    data: commentsData,
    isLoading,
    hasNextPage,
    fetchNextPage,
    isFetchingNextPage,
  } = useEventComments(showComments ? event.id : "");
  const createComment = useCreateEventComment(event.id, groupId || undefined);

  const comments = commentsData?.flattened ?? [];

  const isRemovalVote = event.isPendingRemoval === true;

  const affectedUser = event.affectedUser;
  const creator = event.createdByUser;
  const currentUserId = getUserIdFromToken(authStore.getAccessToken() || "") || "";
  const isAffected = affectedUser?.id === currentUserId;

  // Verifica se usuário já votou olhando para o array de approvals
  const hasVoted = useMemo(() => {
    return event.approvals?.some(a => a.userId === currentUserId) ?? false;
  }, [event.approvals, currentUserId]);

  const canVote = !isAffected && !hasVoted;

  // Contagens reais do backend
  const removeCount = event.approvals?.filter(a => a.voteType === 3).length ?? 0;
  const keepCount = event.approvals?.filter(a => a.voteType === 4).length ?? 0;
  const quorumRequired = event.quorumRequired ?? 5;

  // Calcula tempo restante
  const timeRemaining = useMemo(() => {
    if (!event.removalVoteDeadline) return null;
    const deadline = new Date(event.removalVoteDeadline);
    const now = new Date();
    const diffMs = deadline.getTime() - now.getTime();
    if (diffMs <= 0) return "Prazo expirado";
    const hours = Math.floor(diffMs / (1000 * 60 * 60));
    const minutes = Math.floor((diffMs % (1000 * 60 * 60)) / (1000 * 60));
    if (hours > 24) {
      const days = Math.floor(hours / 24);
      return `${days}d ${hours % 24}h restantes`;
    }
    return `${hours}h ${minutes}m restantes`;
  }, [event.removalVoteDeadline]);

  function handleVote(type: "confirm" | "reject" | "remove" | "keep") {
    if (!canVote || vote.isPending) return;
    const voteType: EventVoteType = isRemovalVote
      ? type === "remove" ? 3 : 4
      : type === "confirm" ? 1 : 2;
    vote.mutate({ voteType });
  }

  const [previewImage, setPreviewImage] = useState<string | null>(null);

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
      <>
        <div className="shadow-[0_1px_3px_rgba(0,0,0,0.05)] min-w-[260px] snap-start p-4 bg-surface-container-lowest rounded-xl border border-surface-container">
          <div className="flex items-start gap-2">
            <div
              className="w-8 h-8 rounded-full bg-surface-container flex items-center justify-center text-secondary font-bold text-xs flex-shrink-0 overflow-hidden cursor-pointer"
              onClick={() => affectedUser?.avatarUrl && setPreviewImage(affectedUser.avatarUrl)}
            >
              {isRemovalVote ? (
                <Trash2 size={14} />
              ) : affectedUser?.avatarUrl ? (
                <img src={affectedUser.avatarUrl} alt={affectedUser.name} className="w-full h-full object-cover" />
              ) : (
                initials
              )}
            </div>
            <div className="flex-1 min-w-0">
              <p className="text-sm font-medium text-on-surface truncate">{event.title}</p>
              <p className="text-xs text-secondary truncate">
                {isRemovalVote ? `Remoção de ${affectedUser?.name}` : `Validar ${affectedUser?.name}`}
              </p>
              <div className="flex gap-2 mt-2">
                {isRemovalVote ? (
                  <>
                    <button
                      type="button"
                      className="flex-1 text-xs py-1.5 px-3 rounded-lg border border-surface-container text-on-surface bg-surface-container-lowest hover:bg-surface-container-low transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                      onClick={() => handleVote("keep")}
                      disabled={!canVote || vote.isPending}
                    >
                      {vote.isPending ? <AppSpinner size="xs" /> : "Manter"}
                    </button>
                    <button
                      type="button"
                      className="flex-1 text-xs py-1.5 px-3 rounded-lg bg-error text-on-primary hover:opacity-90 transition-opacity disabled:opacity-50 disabled:cursor-not-allowed"
                      onClick={() => handleVote("remove")}
                      disabled={!canVote || vote.isPending}
                    >
                      {vote.isPending ? <AppSpinner size="xs" /> : "Remover"}
                    </button>
                  </>
                ) : (
                  <>
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
                  </>
                )}
              </div>
            </div>
          </div>
        </div>

        {previewImage && (
          <ImageModal
            imageUrl={previewImage}
            alt={event.title}
            onClose={() => setPreviewImage(null)}
          />
        )}
      </>
    );
  }

  const votesCount = isRemovalVote
    ? removeCount + keepCount
    : (event.approvals?.length || 0);

  const progressPercent = Math.min((votesCount / quorumRequired) * 100, 100);

  return (
    <div className="bg-surface-container-lowest rounded-xl p-5 shadow-sm border border-outline-variant/50 relative overflow-hidden hover:scale-[0.99] transition-transform duration-200">
      {/* Progress bar at top */}
      <div className="absolute top-0 left-0 w-full h-1 bg-surface-container">
        <div
          className={`h-full rounded-full transition-all duration-500 ${isRemovalVote ? "bg-warning" : "bg-error"}`}
          style={{ width: `${progressPercent}%` }}
        />
      </div>

      <div className="flex items-start gap-4 mt-1">
        <div className="flex-1 min-w-0">
          <div className="flex items-start justify-between gap-2">
            <div>
              <div className="flex items-center gap-2">
                <div
                  className="w-8 h-8 rounded-full overflow-hidden bg-surface-container flex-shrink-0 cursor-pointer"
                  onClick={() => affectedUser?.avatarUrl && setPreviewImage(affectedUser.avatarUrl)}
                >
                  {affectedUser?.avatarUrl ? (
                    <img src={affectedUser.avatarUrl} alt={affectedUser.name} className="w-full h-full object-cover" />
                  ) : (
                    <div className="w-full h-full flex items-center justify-center text-secondary font-bold text-xs">
                      {affectedUser?.name?.charAt(0).toUpperCase() || "U"}
                    </div>
                  )}
                </div>
                <p className="text-body-md font-body-md text-on-surface">
                  <span className="font-semibold">{isRemovalVote ? "Remoção de" : null} {affectedUser?.name}</span>
                </p>
              </div>
              <div className="flex items-center gap-1.5 mt-1">
                <div
                  className="w-5 h-5 rounded-full overflow-hidden bg-surface-container flex-shrink-0 cursor-pointer"
                  onClick={() => creator?.avatarUrl && setPreviewImage(creator.avatarUrl)}
                >
                  {creator?.avatarUrl ? (
                    <img src={creator.avatarUrl} alt={creator.name} className="w-full h-full object-cover" />
                  ) : (
                    <div className="w-full h-full flex items-center justify-center text-secondary font-bold text-[8px]">
                      {creator?.name?.charAt(0).toUpperCase() || "U"}
                    </div>
                  )}
                </div>
                <p className="text-caption font-caption text-secondary mt-0.5">
                  {isRemovalVote ? `iniciado por ${creator?.name}` : `registrado por ${creator?.name}`}
                </p>
              </div>
            </div>

            <span className="flex-shrink-0 inline-flex items-center gap-0.5 bg-error-container text-on-error-container text-caption font-caption px-2.5 py-1 rounded-full">
              <ArrowDown size={12} />
              {isRemovalVote ? "" : "-"}{event.points}pts
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
                {isRemovalVote
                  ? `Votação de Remoção (${removeCount}/${quorumRequired} remover, ${keepCount}/${quorumRequired} manter)`
                  : `Validação Pendente (${votesCount}/${quorumRequired} votos)`}
              </span>
              {isRemovalVote && timeRemaining && (
                <span className="text-caption font-caption text-amber-600 flex items-center gap-1">
                  <Clock size={12} />
                  {timeRemaining}
                </span>
              )}
            </div>

            {!canVote && !hasVoted && (
              <p className="text-caption font-caption text-secondary mb-3">
                {isAffected ? "Usuário afetado não pode votar" : "Você não pode votar neste evento"}
              </p>
            )}

            {hasVoted && (
              <p className="text-caption font-caption text-success mb-3 font-medium">
                Voto registrado com sucesso!
              </p>
            )}

            <div className="grid grid-cols-2 gap-3">
              {isRemovalVote ? (
                <>
                  <button
                    type="button"
                    className="w-full py-2.5 px-4 rounded-full bg-surface-container-lowest border border-surface-container-highest text-on-surface text-label-bold font-label-bold hover:bg-surface-container-low transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                    onClick={() => handleVote("keep")}
                    disabled={!canVote || vote.isPending}
                  >
                    {vote.isPending ? <AppSpinner size="sm" /> : "Manter"}
                  </button>
                  <button
                    type="button"
                    className="w-full py-2.5 px-4 rounded-full bg-error text-on-error text-label-bold font-label-bold hover:opacity-90 transition-opacity disabled:opacity-50 disabled:cursor-not-allowed"
                    onClick={() => handleVote("remove")}
                    disabled={!canVote || vote.isPending}
                  >
                    {vote.isPending ? <AppSpinner size="sm" /> : "Remover"}
                  </button>
                </>
              ) : (
                <>
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
                </>
              )}
            </div>
          </div>

          {/* Comment Section */}
          <div className="mt-3 flex items-center gap-4 text-secondary">
            <button
              type="button"
              onClick={() => setShowComments(!showComments)}
              className="flex items-center gap-1 text-caption font-caption hover:text-primary transition-colors"
            >
              <MessageCircle size={16} />
              <span>{event.commentCount ?? 0}</span>
            </button>
          </div>

          {showComments && (
            <CommentsSection
              comments={comments || []}
              onSubmit={(content, parentId) =>
                createComment.mutate({ content, parentCommentId: parentId })
              }
              isLoading={isLoading}
              isSubmitting={createComment.isPending}
              commentCount={event.commentCount ?? 0}
              hasMoreComments={!!hasNextPage}
              onLoadMoreComments={() => fetchNextPage()}
              isLoadingMoreComments={isFetchingNextPage}
            />
          )}
        </div>
      </div>

      {previewImage && (
        <ImageModal
          imageUrl={previewImage}
          alt={event.title}
          onClose={() => setPreviewImage(null)}
        />
      )}
    </div>
  );
}
