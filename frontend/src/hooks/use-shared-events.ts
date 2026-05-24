import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { deleteJson, getJson, postJson, putJson } from "../lib/api";
import type {
  CreateSharedEventRequest,
  CreateSharedEventResponse,
  SharedEvent,
  UpdateSharedEventRequest,
  UpdateSharedEventResponse,
} from "../types/shared-event/shared-event";

export function useGroupSharedEvents(groupId: string) {
  return useQuery<SharedEvent[]>({
    queryKey: ["shared-events", "group", groupId],
    queryFn: async () => {
      const response = await getJson<{
        sharedEvents: Array<{
          sharedEventId: string;
          title: string;
          description: string;
          points: number;
          isClosed: boolean;
          createdAt: string;
          groupId: string;
          createdByUserId: string;
          createdByUserName: string;
          participantCount: number;
        }>;
      }>(`/api/shared-events/group/${groupId}`);
      return (response.sharedEvents || []).map((se) => ({
        id: se.sharedEventId,
        title: se.title,
        description: se.description,
        points: se.points,
        isClosed: se.isClosed,
        createdAt: se.createdAt,
        groupId: se.groupId,
        createdByUserId: se.createdByUserId,
        participantCount: se.participantCount,
      }));
    },
    enabled: !!groupId,
  });
}

export function useSharedEvent(id: string) {
  return useQuery<SharedEvent>({
    queryKey: ["shared-events", id],
    queryFn: () => getJson<SharedEvent>(`/api/shared-events/${id}`),
    enabled: !!id,
  });
}

export function useCreateSharedEvent() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (payload: CreateSharedEventRequest) =>
      postJson<CreateSharedEventResponse>("/api/shared-events", payload),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: ["shared-events", "group", variables.groupId] });
      queryClient.invalidateQueries({ queryKey: ["ranking", variables.groupId] });
      queryClient.invalidateQueries({ queryKey: ["feed", variables.groupId] });
    },
  });
}

export function useUpdateSharedEvent(id: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (payload: UpdateSharedEventRequest) =>
      putJson<UpdateSharedEventResponse>(`/api/shared-events/${id}`, payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["shared-events", id] });
      queryClient.invalidateQueries({ queryKey: ["shared-events"] });
      queryClient.invalidateQueries({ queryKey: ["ranking"] });
      queryClient.invalidateQueries({ queryKey: ["feed"] });
    },
  });
}

export function useDeleteSharedEvent(id: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: () => deleteJson<void>(`/api/shared-events/${id}`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["shared-events"] });
      queryClient.invalidateQueries({ queryKey: ["ranking"] });
      queryClient.invalidateQueries({ queryKey: ["feed"] });
    },
  });
}

export function useJoinSharedEvent(id: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: () => postJson<void>(`/api/shared-events/${id}/join`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["shared-events", id] });
      queryClient.invalidateQueries({ queryKey: ["ranking"] });
      queryClient.invalidateQueries({ queryKey: ["feed"] });
    },
  });
}

export function useLeaveSharedEvent(id: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: () => postJson<void>(`/api/shared-events/${id}/leave`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["shared-events", id] });
      queryClient.invalidateQueries({ queryKey: ["ranking"] });
      queryClient.invalidateQueries({ queryKey: ["feed"] });
    },
  });
}

export function useCloseSharedEvent(id: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: () => postJson<void>(`/api/shared-events/${id}/close`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["shared-events", id] });
      queryClient.invalidateQueries({ queryKey: ["shared-events"] });
      queryClient.invalidateQueries({ queryKey: ["ranking"] });
      queryClient.invalidateQueries({ queryKey: ["feed"] });
    },
  });
}
