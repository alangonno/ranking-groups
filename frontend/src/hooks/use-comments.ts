import { useInfiniteQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { getJson, postJson } from "../lib/api";
import { flattenPages, getNextPageParam } from "../lib/cursor-utils";
import { invalidateFirstPage } from "../lib/query-utils";
import type {
  Comment,
  CreateCommentRequest,
  CreateCommentResponse,
} from "../types/comment/comment";

export function useEventComments(eventId: string) {
  return useInfiniteQuery({
    queryKey: ["comments", "event", eventId],
    queryFn: async ({ pageParam }) => {
      const response = await getJson<{
        comments: Comment[];
        hasMore: boolean;
        nextCursor: string | null;
      }>(`/api/events/${eventId}/comments`, { cursor: pageParam });
      return {
        items: response.comments || [],
        hasMore: response.hasMore,
        nextCursor: response.nextCursor,
      };
    },
    getNextPageParam,
    initialPageParam: undefined as string | undefined,
    enabled: !!eventId,
    select: (data) => ({
      ...data,
      flattened: flattenPages(data.pages),
    }),
  });
}

export function useSharedEventComments(sharedEventId: string) {
  return useInfiniteQuery({
    queryKey: ["comments", "shared-event", sharedEventId],
    queryFn: async ({ pageParam }) => {
      const response = await getJson<{
        comments: Comment[];
        hasMore: boolean;
        nextCursor: string | null;
      }>(`/api/shared-events/${sharedEventId}/comments`, { cursor: pageParam });
      return {
        items: response.comments || [],
        hasMore: response.hasMore,
        nextCursor: response.nextCursor,
      };
    },
    getNextPageParam,
    initialPageParam: undefined as string | undefined,
    enabled: !!sharedEventId,
    select: (data) => ({
      ...data,
      flattened: flattenPages(data.pages),
    }),
  });
}

export function useCreateEventComment(eventId: string, groupId?: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (payload: CreateCommentRequest) =>
      postJson<CreateCommentResponse>(`/api/events/${eventId}/comments`, payload),
    onSuccess: () => {
      invalidateFirstPage(queryClient, ["comments", "event", eventId]);
      if (groupId) {
        invalidateFirstPage(queryClient, ["events", "group", groupId]);
        invalidateFirstPage(queryClient, ["feed", groupId]);
      }
    },
  });
}

export function useCreateSharedEventComment(sharedEventId: string, groupId?: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (payload: CreateCommentRequest) =>
      postJson<CreateCommentResponse>(`/api/shared-events/${sharedEventId}/comments`, payload),
    onSuccess: () => {
      invalidateFirstPage(queryClient, ["comments", "shared-event", sharedEventId]);
      if (groupId) {
        invalidateFirstPage(queryClient, ["shared-events", "group", groupId]);
        invalidateFirstPage(queryClient, ["feed", groupId]);
      }
    },
  });
}
