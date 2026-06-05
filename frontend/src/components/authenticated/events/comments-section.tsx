import { useState } from "react";
import { Send, ChevronDown, ChevronUp, ChevronRight } from "lucide-react";
import { AppButton } from "../../ui/app-button";
import { AppSpinner } from "../../ui/app-spinner";
import { formatRelativeTime } from "../../../lib/format-time";
import type { Comment } from "../../../types/comment/comment";

interface CommentsSectionProps {
  comments: Comment[];
  onSubmit: (content: string, parentCommentId?: string) => void;
  isLoading: boolean;
  isSubmitting: boolean;
  commentCount: number;
}

export function CommentsSection({
  comments,
  onSubmit,
  isLoading,
  isSubmitting,
  commentCount,
}: CommentsSectionProps) {
  const [expanded, setExpanded] = useState(false);
  const [replyingTo, setReplyingTo] = useState<string | null>(null);
  const [newComment, setNewComment] = useState("");
  const [replyContent, setReplyContent] = useState("");
  const [expandedReplies, setExpandedReplies] = useState<Set<string>>(new Set());

  // Get root comments (no parentCommentId)
  const rootComments = comments.filter((c) => !c.parentCommentId);

  // Sort by createdAt desc (most recent first)
  const sortedRootComments = [...rootComments].sort(
    (a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
  );

  // Show only 2 root comments initially unless expanded
  const visibleRootComments = expanded
    ? sortedRootComments
    : sortedRootComments.slice(0, 2);

  const hasMoreComments = sortedRootComments.length > 2;

  function handleSubmitComment() {
    if (!newComment.trim()) return;
    onSubmit(newComment.trim());
    setNewComment("");
  }

  function handleSubmitReply(parentCommentId: string) {
    if (!replyContent.trim()) return;
    onSubmit(replyContent.trim(), parentCommentId);
    setReplyContent("");
    setReplyingTo(null);
  }

  function getReplies(parentId: string): Comment[] {
    return comments
      .filter((c) => c.parentCommentId === parentId)
      .sort((a, b) => new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime());
  }

  function toggleExpand(id: string) {
    setExpandedReplies((prev) => {
      const next = new Set(prev);
      if (next.has(id)) {
        next.delete(id);
      } else {
        next.add(id);
      }
      return next;
    });
  }

  if (isLoading) {
    return (
      <div className="flex items-center justify-center py-4">
        <AppSpinner size="sm" />
      </div>
    );
  }

  return (
    <div className="mt-3 border-t border-surface-container pt-3">
      {/* Root Comments */}
      <div className="space-y-3">
        {visibleRootComments.map((comment) => (
          <CommentItem
            key={comment.id}
            comment={comment}
            getReplies={getReplies}
            isExpanded={true} // Root comments always expanded
            onToggleExpand={toggleExpand}
            expandedReplies={expandedReplies}
            replyingTo={replyingTo}
            onReply={(id) => {
              setReplyingTo(id);
              setReplyContent("");
            }}
            replyContent={replyContent}
            onReplyChange={setReplyContent}
            onSubmitReply={handleSubmitReply}
            isSubmitting={isSubmitting}
            depth={0}
          />
        ))}
      </div>

      {/* Show More / Less */}
      {hasMoreComments && (
        <button
          type="button"
          onClick={() => setExpanded(!expanded)}
          className="mt-3 flex items-center gap-1 text-caption font-caption text-secondary hover:text-primary transition-colors"
        >
          {expanded ? (
            <>
              <ChevronUp size={14} />
              <span>Mostrar menos</span>
            </>
          ) : (
            <>
              <ChevronDown size={14} />
              <span>Mostrar todos os {commentCount} comentários</span>
            </>
          )}
        </button>
      )}

      {/* New Comment Input */}
      <div className="mt-3 flex items-start gap-2">
        <div className="flex-1">
          <input
            type="text"
            value={newComment}
            onChange={(e) => setNewComment(e.target.value)}
            placeholder="Escreva um comentário..."
            className="w-full bg-surface-bright rounded-lg px-3 py-2 text-body-sm font-body-sm text-on-surface placeholder:text-secondary border border-surface-container focus:outline-none focus:border-primary transition-colors"
            onKeyDown={(e) => {
              if (e.key === "Enter" && !e.shiftKey) {
                e.preventDefault();
                handleSubmitComment();
              }
            }}
          />
        </div>
        <AppButton
          size="sm"
          color="primary"
          onClick={handleSubmitComment}
          disabled={!newComment.trim() || isSubmitting}
          className="px-3 py-2"
        >
          {isSubmitting ? <AppSpinner size="xs" /> : <Send size={14} />}
        </AppButton>
      </div>
    </div>
  );
}

interface CommentItemProps {
  comment: Comment;
  getReplies: (parentId: string) => Comment[];
  isExpanded: boolean;
  onToggleExpand: (id: string) => void;
  expandedReplies: Set<string>;
  replyingTo: string | null;
  onReply: (id: string) => void;
  replyContent: string;
  onReplyChange: (value: string) => void;
  onSubmitReply: (parentId: string) => void;
  isSubmitting: boolean;
  depth: number;
}

function CommentItem({
  comment,
  getReplies,
  isExpanded,
  onToggleExpand,
  expandedReplies,
  replyingTo,
  onReply,
  replyContent,
  onReplyChange,
  onSubmitReply,
  isSubmitting,
  depth,
}: CommentItemProps) {
  const maxDepth = 2;
  const effectiveDepth = Math.min(depth, maxDepth);
  const indentClass = effectiveDepth > 0 ? `ml-${effectiveDepth * 3}` : "";

  const replies = getReplies(comment.id);
  const hasReplies = replies.length > 0;

  return (
    <div className={`${indentClass} ${effectiveDepth > 0 ? "border-l-2 border-surface-container pl-3" : ""}`}>
      <div className="flex items-start gap-2">
        <div className="w-7 h-7 rounded-full bg-primary-container flex items-center justify-center flex-shrink-0">
          <span className="text-[10px] font-bold text-primary">
            {comment.userName?.charAt(0).toUpperCase() || "U"}
          </span>
        </div>
        <div className="flex-1 min-w-0">
          <div className="flex items-center gap-1.5">
            <span className="text-caption font-caption font-semibold text-on-surface">
              {comment.userName || "Usuário"}
            </span>
            <span className="text-[10px] text-secondary">
              {formatRelativeTime(comment.createdAt)}
            </span>
          </div>
          <p className="text-body-sm font-body-sm text-on-surface mt-0.5">
            {comment.content}
          </p>

          {/* Actions: Reply + Expand/Collapse */}
          <div className="mt-1 flex items-center gap-2">
            <button
              type="button"
              onClick={() => onReply(comment.id)}
              className="text-[10px] text-secondary hover:text-primary transition-colors"
            >
              Responder
            </button>

            {hasReplies && (
              <button
                type="button"
                onClick={() => onToggleExpand(comment.id)}
                className="flex items-center gap-0.5 text-[10px] text-secondary hover:text-primary transition-colors"
              >
                {isExpanded ? (
                  <>
                    <ChevronDown size={12} />
                    <span>Ocultar</span>
                  </>
                ) : (
                  <>
                    <ChevronRight size={12} />
                    <span>{replies.length} resposta{replies.length > 1 ? "s" : ""}</span>
                  </>
                )}
              </button>
            )}
          </div>

          {/* Reply Input */}
          {replyingTo === comment.id && (
            <div className="mt-2 flex items-start gap-2">
              <input
                type="text"
                value={replyContent}
                onChange={(e) => onReplyChange(e.target.value)}
                placeholder="Escreva uma resposta..."
                className="flex-1 bg-surface-bright rounded-lg px-3 py-1.5 text-body-sm font-body-sm text-on-surface placeholder:text-secondary border border-surface-container focus:outline-none focus:border-primary transition-colors"
                onKeyDown={(e) => {
                  if (e.key === "Enter" && !e.shiftKey) {
                    e.preventDefault();
                    onSubmitReply(comment.id);
                  }
                }}
                autoFocus
              />
              <AppButton
                size="xs"
                color="primary"
                onClick={() => onSubmitReply(comment.id)}
                disabled={!replyContent.trim() || isSubmitting}
                className="px-2 py-1.5"
              >
                {isSubmitting ? <AppSpinner size="xs" /> : <Send size={12} />}
              </AppButton>
            </div>
          )}

          {/* Nested Replies */}
          {hasReplies && isExpanded && (
            <div className="mt-2 space-y-2">
              {replies.map((reply) => (
                <CommentItem
                  key={reply.id}
                  comment={reply}
                  getReplies={getReplies}
                  isExpanded={expandedReplies.has(reply.id)} // Child comments collapsed by default (Option B)
                  onToggleExpand={onToggleExpand}
                  expandedReplies={expandedReplies}
                  replyingTo={replyingTo}
                  onReply={onReply}
                  replyContent={replyContent}
                  onReplyChange={onReplyChange}
                  onSubmitReply={onSubmitReply}
                  isSubmitting={isSubmitting}
                  depth={depth + 1}
                />
              ))}
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
