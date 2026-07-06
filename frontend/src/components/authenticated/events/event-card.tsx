import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { ArrowUp, ArrowDown, MessageCircle } from "lucide-react";
import { EventType } from "../../../types/event/event";
import type { Event } from "../../../types/event/event";
import { formatRelativeTime } from "../../../lib/format-time";
import { CommentsSection } from "./comments-section";
import { useEventComments, useCreateEventComment } from "../../../hooks/use-comments";
import { useCurrentGroupId } from "../../../lib/use-group-context";
import { ImageModal } from "../../ui/image-modal";

interface EventCardProps {
  event: Event;
}

export function EventCard({ event }: EventCardProps) {
  const navigate = useNavigate();
  const isPositive = event.type === EventType.Positive;
  const affected = event.affectedUser;
  const creator = event.createdByUser;
  const [previewImage, setPreviewImage] = useState<string | null>(null);
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

  function navigateToProfile(userId?: string) {
    if (!groupId || !userId) return;
    navigate(`/group/${groupId}/profile/${userId}`);
  }

  return (
    <div className="bg-surface-container-lowest rounded-xl p-5 shadow-sm border border-surface-container hover:scale-[0.99] transition-transform duration-200">
      {event.imageUrl && (
        <div
          className="mb-4 rounded-xl overflow-hidden cursor-pointer"
          onClick={() => setPreviewImage(event.imageUrl!)}
        >
          <img
            src={event.imageUrl}
            alt={event.title}
            className="w-full h-48 object-cover"
          />
        </div>
      )}
      <div className="flex items-start gap-4">
        <div className="flex-1 min-w-0">
          <div className="flex items-start justify-between gap-2">
            <div className="min-w-0">
              <div className="flex items-center gap-2">
                <button
                  type="button"
                  className="w-8 h-8 rounded-full overflow-hidden bg-surface-container flex-shrink-0 cursor-pointer"
                  onClick={() => navigateToProfile(affected?.id)}
                >
                  {affected?.avatarUrl ? (
                    <img src={affected.avatarUrl} alt={affected.name} className="w-full h-full object-cover" />
                  ) : (
                    <div className="w-full h-full flex items-center justify-center text-secondary font-bold text-xs">
                      {affected?.name?.charAt(0).toUpperCase() || "U"}
                    </div>
                  )}
                </button>
                <p className="text-body-md font-body-md font-semibold text-on-surface">
                  {affected?.name || "Usuário"}
                </p>
              </div>
              <div className="flex items-center gap-1.5 mt-1">
                <button
                  type="button"
                  className="w-5 h-5 rounded-full overflow-hidden bg-surface-container flex-shrink-0 cursor-pointer"
                  onClick={() => navigateToProfile(creator?.id)}
                >
                  {creator?.avatarUrl ? (
                    <img src={creator.avatarUrl} alt={creator.name} className="w-full h-full object-cover" />
                  ) : (
                    <div className="w-full h-full flex items-center justify-center text-secondary font-bold text-[8px]">
                      {creator?.name?.charAt(0).toUpperCase() || "U"}
                    </div>
                  )}
                </button>
                <p className="text-caption font-caption text-secondary">
                  registrado por {creator?.name} • {formatRelativeTime(event.createdAt)}
                </p>
              </div>
            </div>

            <span className={`flex-shrink-0 inline-flex items-center gap-0.5 text-caption font-caption px-2.5 py-1 rounded-full ${
              isPositive
                ? "bg-primary-fixed/10 text-primary"
                : "bg-error-container text-on-error-container"
            }`}>
              {isPositive ? <ArrowUp size={12} /> : <ArrowDown size={12} />}
              {isPositive ? "+" : "-"}{event.points}pts
            </span>
          </div>

          <div className="mt-3 bg-surface-bright rounded-xl p-4 border border-surface-container-highest">
            <h3 className="text-label-bold font-label-bold text-on-surface">{event.title}</h3>
            <p className="text-body-md font-body-md text-secondary mt-1">{event.description}</p>
          </div>

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
