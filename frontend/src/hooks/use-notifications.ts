import { useInfiniteQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { deleteJson, getJson } from "../lib/api";
import { flattenPages, getNextPageParam } from "../lib/cursor-utils";
import { invalidateFirstPage } from "../lib/query-utils";
import type {
  GetNotificationsResponse,
  MarkNotificationAsReadResponse,
  MarkAllNotificationsAsReadResponse,
} from "../types/notification/notification";

export function useNotifications(groupId: string | null = null) {
  return useInfiniteQuery({
    queryKey: ["notifications", groupId],
    queryFn: async ({ pageParam }) => {
      const params = { groupId, cursor: pageParam };
      const response = await getJson<{
        items: GetNotificationsResponse[];
        hasMore: boolean;
        nextCursor: string | null;
      }>("/api/notifications", params);
      return {
        items: response.items || [],
        hasMore: response.hasMore,
        nextCursor: response.nextCursor,
      };
    },
    getNextPageParam,
    initialPageParam: undefined as string | undefined,
    refetchInterval: 30000,
    select: (data) => ({
      ...data,
      flattened: flattenPages(data.pages),
    }),
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
      invalidateFirstPage(queryClient, ["notifications"]);
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
      invalidateFirstPage(queryClient, ["notifications"]);
    },
  });
}
