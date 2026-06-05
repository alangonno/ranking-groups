import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { getJson, postJson } from "../lib/api";
import type {
  Comment,
  CreateCommentRequest,
  CreateCommentResponse,
  GetEventCommentsResponse,
  GetSharedEventCommentsResponse,
} from "../types/comment/comment";

export function useEventComments(eventId: string) {
  return useQuery<Comment[]>({
    queryKey: ["comments", "event", eventId],
    queryFn: async () => {
      const response = await getJson<GetEventCommentsResponse>(
        `/api/events/${eventId}/comments`
      );
      return response.comments || [];
    },
    enabled: !!eventId,
  });
}

export function useSharedEventComments(sharedEventId: string) {
  return useQuery<Comment[]>({
    queryKey: ["comments", "shared-event", sharedEventId],
    queryFn: async () => {
      const response = await getJson<GetSharedEventCommentsResponse>(
        `/api/shared-events/${sharedEventId}/comments`
      );
      return response.comments || [];
    },
    enabled: !!sharedEventId,
  });
}

export function useCreateEventComment(eventId: string, groupId?: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (payload: CreateCommentRequest) =>
      postJson<CreateCommentResponse>(`/api/events/${eventId}/comments`, payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["comments", "event", eventId] });
      if (groupId) {
        queryClient.invalidateQueries({ queryKey: ["events", "group", groupId] });
        queryClient.invalidateQueries({ queryKey: ["feed", groupId] });
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
      queryClient.invalidateQueries({ queryKey: ["comments", "shared-event", sharedEventId] });
      if (groupId) {
        queryClient.invalidateQueries({ queryKey: ["shared-events", "group", groupId] });
        queryClient.invalidateQueries({ queryKey: ["feed", groupId] });
      }
    },
  });
}
