import type { Event, EventStatus, EventType } from "../../types/event/event";
import type { FeedItem } from "../../types/ranking/ranking";

export const EVENT_STATUS_MAP: Record<string, EventStatus> = {
  Pending: 1,
  Approved: 2,
  Rejected: 3,
  Cancelled: 4,
};

export const EVENT_TYPE_MAP: Record<string, EventType> = {
  Positive: 1,
  Negative: 2,
};

export function mapStringToEventStatus(status: string | undefined): EventStatus {
  return EVENT_STATUS_MAP[status ?? "Pending"] ?? 1;
}

export function mapStringToEventType(type: string | undefined): EventType {
  return EVENT_TYPE_MAP[type ?? "Positive"] ?? 1;
}

export function feedItemToEvent(item: FeedItem): Event {
  return {
    id: item.id,
    groupId: "",
    createdByUserId: item.createdByUserId,
    affectedUserId: item.affectedUserId ?? "",
    title: item.title,
    description: item.description,
    points: item.points,
    type: mapStringToEventType(item.eventType),
    status: mapStringToEventStatus(item.eventStatus),
    createdAt: item.createdAt,
    createdByUser: {
      id: item.createdByUserId,
      name: item.createdByUserName,
      username: "",
      email: "",
      createdAt: "",
      updatedAt: "",
    },
    affectedUser: item.affectedUserId
      ? {
          id: item.affectedUserId,
          name: item.affectedUserName ?? "",
          username: "",
          email: "",
          createdAt: "",
          updatedAt: "",
        }
      : undefined,
    approvals: [],
    isPendingRemoval: false,
  };
}
