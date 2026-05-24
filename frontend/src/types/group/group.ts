import type { BaseEntity } from "../common/base-entity";
import type { User } from "../auth/user";
import type { UserSharedEvent } from "../shared-event/shared-event";
import type { RankingEntry } from "../ranking/ranking";

export const GroupRole = {
  Owner: 1,
  Admin: 2,
  Member: 3,
} as const;

export type GroupRole = (typeof GroupRole)[keyof typeof GroupRole];

export interface Group extends BaseEntity {
  name: string;
  description?: string;
  inviteCode: string;
  createdByUserId: string;
  createdByUser?: User;
}

export interface GroupMember extends BaseEntity {
  groupId: string;
  userId: string;
  user?: User;
  role: GroupRole;
  currentScore: number;
}

export interface GroupMemberProfile {
  userId: string;
  name: string;
  username: string;
  email: string;
  avatar: string;
  role: GroupRole;
  currentScore: number;
  rankPosition: number;
}

export interface CreateGroupRequest {
  name: string;
  description?: string;
}

export type CreateGroupResponse = Group;

export interface JoinGroupRequest {
  inviteCode: string;
}

export type JoinGroupResponse = GroupMember;

export interface UserEventHistory {
  id: string;
  title: string;
  description: string;
  points: number;
  type: string;
  status: string;
  createdAt: string;
  createdByUserId: string;
  createdByUserName: string;
  affectedUserId: string;
  affectedUserName: string;
  scoreBalance: number;
}

export interface GroupUserProfileResponse {
  member: GroupMemberProfile;
  events: UserEventHistory[];
  sharedEvents: UserSharedEvent[];
}

export interface GroupDetailsResponse {
  groupId: string;
  name: string;
  description?: string;
  inviteCode: string;
  members: GroupMemberProfile[];
  ranking: RankingEntry[];
}
