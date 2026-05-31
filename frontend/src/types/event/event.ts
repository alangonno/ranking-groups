import type { BaseEntity } from "../common/base-entity";
import type { User } from "../auth/user";

export const EventStatus = {
  Pending: 1,
  Approved: 2,
  Rejected: 3,
  Cancelled: 4,
} as const;

export type EventStatus = (typeof EventStatus)[keyof typeof EventStatus];

export const EventType = {
  Positive: 1,
  Negative: 2,
} as const;

export type EventType = (typeof EventType)[keyof typeof EventType];

export const EventVoteType = {
  Approve: 1,
  Reject: 2,
  Remove: 3,
  Keep: 4,
} as const;

export type EventVoteType = (typeof EventVoteType)[keyof typeof EventVoteType];

export interface EventApproval extends BaseEntity {
  eventId: string;
  userId: string;
  user?: User;
  voteType: EventVoteType;
}

export interface Event extends BaseEntity {
  groupId: string;
  createdByUserId: string;
  affectedUserId: string;
  title: string;
  description: string;
  points: number;
  type: EventType;
  status: EventStatus;
  approvedAt?: string;
  rejectedAt?: string;
  cancelledAt?: string;
  createdByUser?: User;
  affectedUser?: User;
  approvals?: EventApproval[];
  isPendingRemoval?: boolean;
  removalVoteDeadline?: string;
  quorumRequired?: number;
}

export interface EventWithScoreBalance extends Event {
  scoreBalance: number;
}

export interface CreateEventRequest {
  groupId: string;
  affectedUserId: string;
  title: string;
  description: string;
  points: number;
  type: EventType;
}

export type CreateEventResponse = Event;

export interface UpdateEventRequest {
  title?: string;
  description?: string;
  points?: number;
}

export type UpdateEventResponse = Event;

export interface VoteEventRequest {
  voteType: EventVoteType;
}

export interface VoteEventResponse {
  eventId: string;
  status: string;
  approvalCount: number;
  eventApproved: boolean;
  isPendingRemoval?: boolean;
  removeCount?: number;
  keepCount?: number;
  quorumRequired?: number;
  removalResolved?: boolean;
}

export interface RequestEventRemovalResponse {
  eventId: string;
  isPendingRemoval: boolean;
  removeCount: number;
  keepCount: number;
  quorumRequired: number;
}
