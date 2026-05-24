import type { BaseEntity } from "../common/base-entity";
import type { User } from "../auth/user";

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

export interface CreateGroupRequest {
  name: string;
  description?: string;
}

export type CreateGroupResponse = Group;

export interface JoinGroupRequest {
  inviteCode: string;
}

export type JoinGroupResponse = GroupMember;
