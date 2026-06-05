import type { Event } from "../event/event";
import type { User } from "../auth/user";

export interface RankingEntry {
  user: User;
  score: number;
  weeklyScore: number;
}

export interface RankingQueryParams {
  fromDate?: string;
  toDate?: string;
}

export interface FeedItem {
  id: string;
  feedItemType: "event" | "shared_event";
  title: string;
  description: string;
  points: number;
  createdAt: string;
  createdByUserId: string;
  createdByUserName: string;
  createdByUserAvatarUrl?: string;

  // event-specific
  affectedUserId?: string;
  affectedUserName?: string;
  affectedUserAvatarUrl?: string;
  eventStatus?: string;
  eventType?: string;
  scoreBalance?: number;

    // shared-event-specific
    participantCount?: number;
    isClosed?: boolean;
    hasCurrentUserJoined?: boolean;
    commentCount?: number;
    imageUrl?: string;
}

export type DashboardFeedEntry =
  | { type: "event"; event: Event }
  | { type: "shared_event"; item: FeedItem };

export interface TopMemberEntry {
  position: number;
  name: string;
  points: number;
  avatarUrl?: string;
}
