export interface Notification {
  notificationId: string;
  title: string;
  description: string;
  action: string;
  eventId?: string;
  sharedEventId?: string;
  createdAt: string;
}

export interface GetNotificationsResponse {
  notificationId: string;
  title: string;
  description: string;
  action: string;
  eventId?: string;
  sharedEventId?: string;
  createdAt: string;
}

export interface MarkNotificationAsReadResponse {
  success: boolean;
}

export interface MarkAllNotificationsAsReadResponse {
  deletedCount: number;
}
