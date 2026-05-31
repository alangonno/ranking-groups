import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { deleteJson, getJson, postJson, putJson } from "../lib/api";
import type {
  CreateEventRequest,
  CreateEventResponse,
  Event,
  EventStatus,
  EventType,
  EventWithScoreBalance,
  RequestEventRemovalResponse,
  UpdateEventRequest,
  UpdateEventResponse,
  VoteEventRequest,
  VoteEventResponse,
} from "../types/event/event";

const EVENT_STATUS_MAP: Record<string, EventStatus> = {
  Pending: 1,
  Approved: 2,
  Rejected: 3,
  Cancelled: 4,
};

const EVENT_TYPE_MAP: Record<string, EventType> = {
  Positive: 1,
  Negative: 2,
};

function mapEventFromBackend(e: {
  eventId: string;
  title: string;
  description: string;
  points: number;
  type: string;
  status: string;
  createdAt: string;
  createdByUserId: string;
  createdByUserName: string;
  affectedUserId: string;
  affectedUserName: string;
  approvalCount?: number;
  isPendingRemoval?: boolean;
}): Event {
  return {
    id: e.eventId,
    groupId: "",
    createdByUserId: e.createdByUserId,
    affectedUserId: e.affectedUserId,
    title: e.title,
    description: e.description,
    points: e.points,
    type: EVENT_TYPE_MAP[e.type] ?? 1,
    status: EVENT_STATUS_MAP[e.status] ?? 1,
    createdAt: e.createdAt,
    createdByUser: { id: e.createdByUserId, name: e.createdByUserName, username: "", email: "" },
    affectedUser: { id: e.affectedUserId, name: e.affectedUserName, username: "", email: "" },
    isPendingRemoval: e.isPendingRemoval ?? false,
  };
}

export function useGroupEvents(groupId: string) {
  return useQuery<Event[]>({
    queryKey: ["events", "group", groupId],
    queryFn: async () => {
      const response = await getJson<{ events: Array<Parameters<typeof mapEventFromBackend>[0]> }>(
        `/api/events/group/${groupId}`
      );
      return (response.events || []).map(mapEventFromBackend);
    },
    enabled: !!groupId,
  });
}

export function useUserEvents(groupId: string, userId: string) {
  return useQuery<EventWithScoreBalance[]>({
    queryKey: ["events", "group", groupId, "user", userId],
    queryFn: async () => {
      const response = await getJson<{
        events: Array<Parameters<typeof mapEventFromBackend>[0] & { scoreBalance: number }>;
      }>(`/api/events/group/${groupId}/user/${userId}`);
      return (response.events || []).map((e) => ({
        ...mapEventFromBackend(e),
        scoreBalance: e.scoreBalance,
      }));
    },
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

export function useRequestEventRemoval(eventId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: () =>
      postJson<RequestEventRemovalResponse>(`/api/events/${eventId}/request-removal`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["events", eventId] });
      queryClient.invalidateQueries({ queryKey: ["events"] });
      queryClient.invalidateQueries({ queryKey: ["ranking"] });
      queryClient.invalidateQueries({ queryKey: ["feed"] });
    },
  });
}
