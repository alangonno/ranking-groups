import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { deleteJson, getJson, postJson, putJson } from "../lib/api";
import type {
  CreateEventRequest,
  CreateEventResponse,
  Event,
  EventWithScoreBalance,
  UpdateEventRequest,
  UpdateEventResponse,
  VoteEventRequest,
  VoteEventResponse,
} from "../types/event/event";

export function useGroupEvents(groupId: string) {
  return useQuery<Event[]>({
    queryKey: ["events", "group", groupId],
    queryFn: () => getJson<Event[]>(`/api/events/group/${groupId}`),
    enabled: !!groupId,
  });
}

export function useUserEvents(groupId: string, userId: string) {
  return useQuery<EventWithScoreBalance[]>({
    queryKey: ["events", "group", groupId, "user", userId],
    queryFn: () =>
      getJson<EventWithScoreBalance[]>(`/api/events/group/${groupId}/user/${userId}`),
    enabled: !!groupId && !!userId,
  });
}

export function useEvent(eventId: string) {
  return useQuery<Event>({
    queryKey: ["events", eventId],
    queryFn: () => getJson<Event>(`/api/events/${eventId}`),
    enabled: !!eventId,
  });
}

export function useCreateEvent() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (payload: CreateEventRequest) =>
      postJson<CreateEventResponse>("/api/events", payload),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: ["events", "group", variables.groupId] });
      queryClient.invalidateQueries({ queryKey: ["ranking", variables.groupId] });
      queryClient.invalidateQueries({ queryKey: ["feed", variables.groupId] });
    },
  });
}

export function useUpdateEvent(eventId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (payload: UpdateEventRequest) =>
      putJson<UpdateEventResponse>(`/api/events/${eventId}`, payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["events", eventId] });
      queryClient.invalidateQueries({ queryKey: ["events"] });
      queryClient.invalidateQueries({ queryKey: ["ranking"] });
      queryClient.invalidateQueries({ queryKey: ["feed"] });
    },
  });
}

export function useDeleteEvent(eventId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: () => deleteJson<void>(`/api/events/${eventId}`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["events"] });
      queryClient.invalidateQueries({ queryKey: ["ranking"] });
      queryClient.invalidateQueries({ queryKey: ["feed"] });
    },
  });
}

export function useVoteEvent(eventId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (payload: VoteEventRequest) =>
      postJson<VoteEventResponse>(`/api/events/${eventId}/vote`, payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["events", eventId] });
      queryClient.invalidateQueries({ queryKey: ["events"] });
      queryClient.invalidateQueries({ queryKey: ["ranking"] });
      queryClient.invalidateQueries({ queryKey: ["feed"] });
    },
  });
}
