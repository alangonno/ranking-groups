import type { BaseEntity } from "../common/base-entity";
import type { User } from "../auth/user";

export interface SharedEventParticipant extends BaseEntity {
  sharedEventId: string;
  userId: string;
  user?: User;
}

export interface SharedEvent extends BaseEntity {
  groupId: string;
  createdByUserId: string;
  title: string;
  description: string;
  points: number;
  isClosed: boolean;
  createdByUser?: User;
  participants?: SharedEventParticipant[];
  participantCount: number;
}

export interface CreateSharedEventRequest {
  groupId: string;
  title: string;
  description: string;
  points: number;
}

export type CreateSharedEventResponse = SharedEvent;

export interface UpdateSharedEventRequest {
  title?: string;
  description?: string;
  points?: number;
}

export type UpdateSharedEventResponse = SharedEvent;

export type JoinSharedEventResponse = SharedEventParticipant;

export type LeaveSharedEventResponse = void;

export interface UserSharedEvent extends SharedEvent {
  userRole: "organizer" | "participant";
  participantCount: number;
}
