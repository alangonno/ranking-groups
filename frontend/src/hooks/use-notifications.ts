import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { getJson, deleteJson } from "../lib/api";
import type {
  GetNotificationsResponse,
  MarkNotificationAsReadResponse,
  MarkAllNotificationsAsReadResponse,
} from "../types/notification/notification";

export function useNotifications(groupId: string | null = null) {
  return useQuery<GetNotificationsResponse[]>({
    queryKey: ["notifications", groupId],
    queryFn: async () => {
      const params = groupId ? { groupId } : undefined;
      return getJson<GetNotificationsResponse[]>("/api/notifications", params);
    },
    refetchInterval: 30000,
  });
}

export function useMarkNotificationAsRead() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (notificationId: string) => {
      return deleteJson<MarkNotificationAsReadResponse>(
        `/api/notifications/${notificationId}`
      );
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["notifications"] });
    },
  });
}

export function useMarkAllNotificationsAsRead() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (groupId: string | null = null) => {
      const params = groupId ? { groupId } : undefined;
      return deleteJson<MarkAllNotificationsAsReadResponse>(
        "/api/notifications",
        params
      );
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["notifications"] });
    },
  });
}
