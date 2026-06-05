import { useInfiniteQuery, useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { deleteJson, getJson, postJson, putJson } from "../lib/api";
import { flattenPages, getNextPageParam } from "../lib/cursor-utils";
import { invalidateFirstPage } from "../lib/query-utils";
import { mapStringToEventStatus, mapStringToEventType } from "../lib/utils/event-mappers";
import type {
  CreateEventRequest,
  CreateEventResponse,
  Event,
  RequestEventRemovalResponse,
  UpdateEventRequest,
  UpdateEventResponse,
  VoteEventRequest,
  VoteEventResponse,
} from "../types/event/event";

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
    removalVoteDeadline?: string | null;
    quorumRequired?: number;
    removeCount?: number;
    keepCount?: number;
    commentCount?: number;
    imageUrl?: string;
    approvals?: Array<{
      userId: string;
      userName: string;
      voteType: string;
      createdAt: string;
    }>;
}): Event {
  const voteTypeMap: Record<string, number> = {
    Approve: 1,
    Reject: 2,
    Remove: 3,
    Keep: 4,
  };

  return {
    id: e.eventId,
    groupId: "",
    createdByUserId: e.createdByUserId,
    affectedUserId: e.affectedUserId,
    title: e.title,
    description: e.description,
    points: e.points,
    type: mapStringToEventType(e.type),
    status: mapStringToEventStatus(e.status),
    createdAt: e.createdAt,
    createdByUser: { id: e.createdByUserId, name: e.createdByUserName, username: "", email: "", createdAt: "", updatedAt: "" },
    affectedUser: { id: e.affectedUserId, name: e.affectedUserName, username: "", email: "", createdAt: "", updatedAt: "" },
        isPendingRemoval: e.isPendingRemoval ?? false,
        removalVoteDeadline: e.removalVoteDeadline ?? undefined,
        quorumRequired: e.quorumRequired,
        commentCount: e.commentCount ?? 0,
        imageUrl: e.imageUrl,
        approvals: e.approvals?.map(a => ({
      id: "", // não usado na listagem
      eventId: e.eventId,
      userId: a.userId,
      user: { id: a.userId, name: a.userName, username: "", email: "", createdAt: "", updatedAt: "" },
      voteType: (voteTypeMap[a.voteType] ?? 1) as import("../types/event/event").EventVoteType,
      createdAt: a.createdAt,
    })),
  };
}

export function useGroupEvents(groupId: string) {
  return useInfiniteQuery({
    queryKey: ["events", "group", groupId],
    queryFn: async ({ pageParam }) => {
      const response = await getJson<{
        events: Array<Parameters<typeof mapEventFromBackend>[0]>;
        hasMore: boolean;
        nextCursor: string | null;
      }>(`/api/events/group/${groupId}`, { cursor: pageParam });
      return {
        items: (response.events || []).map(mapEventFromBackend),
        hasMore: response.hasMore,
        nextCursor: response.nextCursor,
      };
    },
    getNextPageParam,
    initialPageParam: undefined as string | undefined,
    enabled: !!groupId,
    select: (data) => ({
      ...data,
      flattened: flattenPages(data.pages),
    }),
  });
}

export function useUserEvents(groupId: string, userId: string) {
  return useInfiniteQuery({
    queryKey: ["events", "group", groupId, "user", userId],
    queryFn: async ({ pageParam }) => {
      const response = await getJson<{
        events: Array<Parameters<typeof mapEventFromBackend>[0] & { scoreBalance: number }>;
        hasMore: boolean;
        nextCursor: string | null;
      }>(`/api/events/group/${groupId}/user/${userId}`, { cursor: pageParam });
      return {
        items: (response.events || []).map((e) => ({
          ...mapEventFromBackend(e),
          scoreBalance: e.scoreBalance,
        })),
        hasMore: response.hasMore,
        nextCursor: response.nextCursor,
      };
    },
    getNextPageParam,
    initialPageParam: undefined as string | undefined,
    enabled: !!groupId && !!userId,
    select: (data) => ({
      ...data,
      flattened: flattenPages(data.pages),
    }),
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
      invalidateFirstPage(queryClient, ["events", "group", variables.groupId]);
      queryClient.invalidateQueries({ queryKey: ["ranking", variables.groupId] });
      invalidateFirstPage(queryClient, ["feed", variables.groupId]);
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
      invalidateAllFirstPages(queryClient, ["events"]);
      invalidateAllFirstPages(queryClient, ["ranking"]);
      invalidateAllFirstPages(queryClient, ["feed"]);
    },
  });
}

export function useDeleteEvent(eventId: string, groupId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: () => deleteJson<void>(`/api/events/${eventId}`),
    onSuccess: () => {
      queryClient.removeQueries({ queryKey: ["events", eventId] });
      invalidateFirstPage(queryClient, ["events", "group", groupId]);
      queryClient.invalidateQueries({ queryKey: ["ranking", groupId] });
      invalidateFirstPage(queryClient, ["feed", groupId]);
      queryClient.invalidateQueries({ queryKey: ["user-profile"] });
    },
  });
}

export function useVoteEvent(eventId: string, groupId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (payload: VoteEventRequest) =>
      postJson<VoteEventResponse>(`/api/events/${eventId}/vote`, payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["events", eventId] });
      invalidateFirstPage(queryClient, ["events", "group", groupId]);
      queryClient.invalidateQueries({ queryKey: ["ranking", groupId] });
      invalidateFirstPage(queryClient, ["feed", groupId]);
      queryClient.invalidateQueries({ queryKey: ["user-profile"] });
    },
  });
}

export function useRequestEventRemoval(eventId: string, groupId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: () =>
      postJson<RequestEventRemovalResponse>(`/api/events/${eventId}/request-removal`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["events", eventId] });
      invalidateFirstPage(queryClient, ["events", "group", groupId]);
      queryClient.invalidateQueries({ queryKey: ["ranking", groupId] });
      invalidateFirstPage(queryClient, ["feed", groupId]);
      queryClient.invalidateQueries({ queryKey: ["user-profile"] });
    },
  });
}

import type { QueryClient } from "@tanstack/react-query";

function invalidateAllFirstPages(queryClient: QueryClient, baseKey: unknown[]) {
  const keys = queryClient.getQueriesData({ queryKey: baseKey, type: "all" });
  keys.forEach(([queryKey]) => {
    invalidateFirstPage(queryClient, queryKey as unknown[]);
  });
}
