import type { BaseEntity } from "../common/base-entity";
import type { User } from "../auth/user";

export interface SharedEventParticipant extends BaseEntity {
  sharedEventId: string;
  userId: string;
  user?: User;
  userName?: string;
  joinedAt?: string;
}

export interface SharedEvent extends BaseEntity {
  groupId: string;
  title: string;
  description: string;
  points: number;
  isClosed: boolean;
  closesAt?: string;
  createdByUserId: string;
  createdByUserName?: string;
  participantCount: number;
  hasCurrentUserJoined: boolean;
  participants?: SharedEventParticipant[];
    isPendingRemoval?: boolean;
    removalVoteDeadline?: string;
    quorumRequired?: number;
    removeCount?: number;
    keepCount?: number;
    commentCount?: number;
    imageUrl?: string;
    createdByUserAvatarUrl?: string;
}

export interface CreateSharedEventRequest {
  groupId: string;
  title: string;
  description: string;
  points: number;
  closesAt?: string;
  imageUrl?: string;
  participantUserIds?: string[];
}

export type CreateSharedEventResponse = SharedEvent;

export interface UpdateSharedEventRequest {
  title?: string;
  description?: string;
  points?: number;
  participantUserIds?: string[];
}

export type UpdateSharedEventResponse = SharedEvent;

export type JoinSharedEventResponse = SharedEventParticipant;

export type LeaveSharedEventResponse = void;

export interface UserSharedEvent extends SharedEvent {
  userRole: "organizer" | "participant";
  participantCount: number;
}
