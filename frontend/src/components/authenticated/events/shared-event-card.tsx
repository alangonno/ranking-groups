import { useState } from "react";
import { ImageModal } from "../../ui/image-modal";
import { Users, ArrowUp, CheckCircle2, Clock, LogOut, MessageCircle, Pencil } from "lucide-react";
import { AppButton } from "../../ui/app-button";
import { AppSpinner } from "../../ui/app-spinner";
import { useJoinSharedEvent, useLeaveSharedEvent } from "../../../hooks/use-shared-events";
import { CommentsSection } from "./comments-section";
import { useSharedEventComments, useCreateSharedEventComment } from "../../../hooks/use-comments";
import { useCurrentGroupId } from "../../../lib/use-group-context";
import { useAuthContext } from "../../../providers/auth-provider";
import { CreateSharedEventModal } from "./create-shared-event-modal";

interface SharedEvent {
  id: string;
  title: string;
  points: number;
  participantCount: number;
  isClosed: boolean;
  createdByUserId: string;
  createdByUserAvatarUrl?: string;
  hasCurrentUserJoined: boolean;
  closesAt?: string;
  imageUrl?: string;
  commentCount?: number;
}

interface SharedEventCardProps {
  event: SharedEvent;
}

export function SharedEventCard({ event }: SharedEventCardProps) {
  const joinEvent = useJoinSharedEvent(event.id);
  const leaveEvent = useLeaveSharedEvent(event.id);
  const [showComments, setShowComments] = useState(false);
  const [showEditModal, setShowEditModal] = useState(false);
  const groupId = useCurrentGroupId();
  const { user } = useAuthContext();

  const {
    data: commentsData,
    isLoading,
    hasNextPage,
    fetchNextPage,
    isFetchingNextPage,
  } = useSharedEventComments(showComments ? event.id : "");
  const createComment = useCreateSharedEventComment(event.id, groupId || undefined);

  const comments = commentsData?.flattened ?? [];

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
    if (event.hasCurrentUserJoined || joinEvent.isPending) return;
    joinEvent.mutate();
  }

  function handleLeave() {
    if (!event.hasCurrentUserJoined || leaveEvent.isPending) return;
    leaveEvent.mutate();
  }

  const isActionPending = joinEvent.isPending || leaveEvent.isPending;
  const [previewImage, setPreviewImage] = useState<string | null>(null);
  const canEdit = !event.isClosed && user?.id === event.createdByUserId;

  return (
    <div className="bg-surface-container-lowest shadow-sm rounded-2xl min-w-[260px] snap-start overflow-hidden border border-surface-container group cursor-pointer hover:border-outline-variant transition-colors">
      {event.imageUrl ? (
        <div className="h-24 relative">
          <img
            src={event.imageUrl}
            alt={event.title}
            className="w-full h-full object-cover cursor-pointer"
            onClick={() => setPreviewImage(event.imageUrl!)}
          />
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
          <div className="flex items-center gap-3">
            {event.createdByUserAvatarUrl && (
              <div
                className="w-6 h-6 rounded-full overflow-hidden flex-shrink-0 cursor-pointer"
                onClick={() => setPreviewImage(event.createdByUserAvatarUrl!)}
              >
                <img src={event.createdByUserAvatarUrl} alt="Criador" className="w-full h-full object-cover" />
              </div>
            )}
            <div className="flex items-center gap-1 text-caption font-caption text-secondary">
              <Users size={14} />
              <span>{event.participantCount} confirmados</span>
            </div>
            <button
              type="button"
              onClick={() => setShowComments(!showComments)}
              className="flex items-center gap-1 text-caption font-caption text-secondary hover:text-primary transition-colors"
            >
              <MessageCircle size={14} />
              <span>{event.commentCount ?? 0}</span>
            </button>
          </div>
          <div className="flex gap-1.5">
            {canEdit && groupId ? (
              <AppButton
                size="xs"
                color="light"
                className="text-xs px-3 py-1.5 border-0"
                onClick={() => setShowEditModal(true)}
                disabled={isActionPending}
              >
                <Pencil size={12} />
                Editar
              </AppButton>
            ) : null}

            {!event.isClosed && event.hasCurrentUserJoined && (
              <AppButton
                size="xs"
                color="light"
                className="text-xs px-3 py-1.5 border-0"
                onClick={handleLeave}
                disabled={isActionPending}
              >
                {leaveEvent.isPending ? (
                  <AppSpinner size="xs" />
                ) : (
                  <LogOut size={12} />
                )}
                {leaveEvent.isPending ? "Saindo..." : "Sair"}
              </AppButton>
            )}
            {!event.isClosed && !event.hasCurrentUserJoined && (
              <AppButton
                size="xs"
                color="primary"
                className="text-xs px-3 py-1.5"
                onClick={handleJoin}
                disabled={isActionPending}
              >
                {joinEvent.isPending ? (
                  <AppSpinner size="xs" />
                ) : (
                  "Participar"
                )}
              </AppButton>
            )}
          </div>
        </div>

        {showComments && (
          <div className="mt-3 pt-3 border-t border-surface-container">
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
          </div>
        )}
      </div>

      {previewImage && (
        <ImageModal
          imageUrl={previewImage}
          alt={event.title}
          onClose={() => setPreviewImage(null)}
        />
      )}

      {groupId ? (
        <CreateSharedEventModal
          isOpen={showEditModal}
          onClose={() => setShowEditModal(false)}
          groupId={groupId}
          sharedEventId={event.id}
        />
      ) : null}
    </div>
  );
}
