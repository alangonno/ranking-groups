import { useState } from "react";
import { ArrowUp, ArrowDown, MessageCircle } from "lucide-react";
import { EventType } from "../../../types/event/event";
import type { Event } from "../../../types/event/event";
import { formatRelativeTime } from "../../../lib/format-time";
import { CommentsSection } from "./comments-section";
import { useEventComments, useCreateEventComment } from "../../../hooks/use-comments";
import { useCurrentGroupId } from "../../../lib/use-group-context";

interface EventCardProps {
  event: Event;
}

export function EventCard({ event }: EventCardProps) {
  const isPositive = event.type === EventType.Positive;
  const affected = event.affectedUser;
  const creator = event.createdByUser;
  const [showComments, setShowComments] = useState(false);
  const groupId = useCurrentGroupId();

  const { data: comments, isLoading } = useEventComments(
    showComments ? event.id : ""
  );
  const createComment = useCreateEventComment(event.id, groupId || undefined);

  return (
    <div className="bg-surface-container-lowest rounded-xl p-5 shadow-sm border border-surface-container hover:scale-[0.99] transition-transform duration-200">
      <div className="flex items-start gap-4">
        <div className="flex-1 min-w-0">
          <div className="flex items-start justify-between gap-2">
            <div className="min-w-0">
              <p className="text-body-md font-body-md font-semibold text-on-surface">
                {affected?.name || "Usuário"}
              </p>
              <p className="text-caption font-caption text-secondary mt-0.5">
                registrado por {creator?.name} • {formatRelativeTime(event.createdAt)}
              </p>
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
            />
          )}
        </div>
      </div>
    </div>
  );
}
