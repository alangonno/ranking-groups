import type { BaseEntity } from "../common/base-entity";

export interface Comment extends BaseEntity {
  userId: string;
  userName: string;
  avatarUrl?: string;
  eventId?: string;
  sharedEventId?: string;
  parentCommentId?: string;
  content: string;
  replies?: Comment[];
}

export interface CreateCommentRequest {
  content: string;
  parentCommentId?: string;
}

export interface CreateCommentResponse {
  commentId: string;
  content: string;
  parentCommentId?: string;
  createdAt: string;
  userId: string;
  userName: string;
}

export interface GetEventCommentsResponse {
  comments: Comment[];
}

export interface GetSharedEventCommentsResponse {
  comments: Comment[];
}
