import { useInfiniteQuery, useMutation, useQuery, useQueryClient, type QueryClient } from "@tanstack/react-query";
import { deleteJson, getJson, postJson, putJson } from "../lib/api";
import { flattenPages, getNextPageParam } from "../lib/cursor-utils";
import { invalidateFirstPage } from "../lib/query-utils";
import type {
  CreateSharedEventRequest,
  CreateSharedEventResponse,
  SharedEvent,
  SharedEventParticipant,
  UpdateSharedEventRequest,
  UpdateSharedEventResponse,
} from "../types/shared-event/shared-event";

type SharedEventSummaryBackendResponse = {
  sharedEventId: string;
  title: string;
  description: string;
  points: number;
  isClosed: boolean;
  closesAt: string | null;
  createdAt: string;
  groupId: string;
  createdByUserId: string;
  createdByUserName: string;
  participantCount: number;
  hasCurrentUserJoined: boolean;
  commentCount?: number;
  imageUrl?: string;
  createdByUserAvatarUrl?: string;
};

type SharedEventDetailsBackendResponse = {
  sharedEventId: string;
  groupId: string;
  title: string;
  description: string;
  points: number;
  isClosed: boolean;
  createdAt: string;
  createdByUserId: string;
  createdByUserName: string;
  createdByUserAvatarUrl?: string;
  commentCount: number;
  imageUrl?: string;
  participants: Array<{
    userId: string;
    userName: string;
    joinedAt: string;
  }>;
};

function mapParticipantsFromBackend(
  participants: SharedEventDetailsBackendResponse["participants"] = []
): SharedEventParticipant[] {
  return participants.map((participant) => ({
    id: `${participant.userId}-${participant.joinedAt}`,
    sharedEventId: "",
    userId: participant.userId,
    userName: participant.userName,
    joinedAt: participant.joinedAt,
    createdAt: participant.joinedAt,
    updatedAt: participant.joinedAt,
    user: {
      id: participant.userId,
      name: participant.userName,
      username: "",
      email: "",
      createdAt: "",
      updatedAt: "",
    },
  }));
}

function mapSharedEventSummaryFromBackend(se: SharedEventSummaryBackendResponse): SharedEvent {
  return {
    id: se.sharedEventId,
    title: se.title,
    description: se.description,
    points: se.points,
    isClosed: se.isClosed,
    closesAt: se.closesAt ?? undefined,
    createdAt: se.createdAt,
    updatedAt: se.createdAt,
    groupId: se.groupId,
    createdByUserId: se.createdByUserId,
    createdByUserName: se.createdByUserName,
    participantCount: se.participantCount,
    hasCurrentUserJoined: se.hasCurrentUserJoined,
    commentCount: se.commentCount ?? 0,
    imageUrl: se.imageUrl,
    createdByUserAvatarUrl: se.createdByUserAvatarUrl,
  };
}

function mapSharedEventDetailsFromBackend(se: SharedEventDetailsBackendResponse): SharedEvent {
  const participants = mapParticipantsFromBackend(se.participants);

  return {
    id: se.sharedEventId,
    title: se.title,
    description: se.description,
    points: se.points,
    isClosed: se.isClosed,
    createdAt: se.createdAt,
    updatedAt: se.createdAt,
    groupId: se.groupId,
    createdByUserId: se.createdByUserId,
    createdByUserName: se.createdByUserName,
    participantCount: participants.length,
    hasCurrentUserJoined: participants.some((participant) => participant.userId === se.createdByUserId),
    participants: participants.map((participant) => ({
      ...participant,
      sharedEventId: se.sharedEventId,
    })),
    commentCount: se.commentCount ?? 0,
    imageUrl: se.imageUrl,
    createdByUserAvatarUrl: se.createdByUserAvatarUrl,
  };
}

export function useGroupSharedEvents(groupId: string) {
  return useInfiniteQuery({
    queryKey: ["shared-events", "group", groupId],
    queryFn: async ({ pageParam }) => {
      const response = await getJson<{
        sharedEvents: SharedEventSummaryBackendResponse[];
        hasMore: boolean;
        nextCursor: string | null;
      }>(`/api/shared-events/group/${groupId}`, { cursor: pageParam });
      return {
        items: (response.sharedEvents || []).map(mapSharedEventSummaryFromBackend),
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

export function useSharedEvent(id: string) {
  return useQuery<SharedEvent>({
    queryKey: ["shared-events", id],
    queryFn: async () => {
      const response = await getJson<SharedEventDetailsBackendResponse>(`/api/shared-events/${id}`);
      return mapSharedEventDetailsFromBackend(response);
    },
    enabled: !!id,
  });
}

export function useCreateSharedEvent() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (payload: CreateSharedEventRequest) =>
      postJson<CreateSharedEventResponse>("/api/shared-events", payload),
    onSuccess: (_data, variables) => {
      invalidateFirstPage(queryClient, ["shared-events", "group", variables.groupId]);
      queryClient.invalidateQueries({ queryKey: ["ranking", variables.groupId] });
      invalidateFirstPage(queryClient, ["feed", variables.groupId]);
      queryClient.invalidateQueries({ queryKey: ["user-profile"] });
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
      invalidateAllFirstPages(queryClient, ["shared-events"]);
      invalidateAllFirstPages(queryClient, ["ranking"]);
      invalidateAllFirstPages(queryClient, ["feed"]);
      queryClient.invalidateQueries({ queryKey: ["user-profile"] });
    },
  });
}

export function useDeleteSharedEvent(id: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: () => deleteJson<void>(`/api/shared-events/${id}`),
    onSuccess: () => {
      invalidateAllFirstPages(queryClient, ["shared-events"]);
      invalidateAllFirstPages(queryClient, ["ranking"]);
      invalidateAllFirstPages(queryClient, ["feed"]);
      queryClient.invalidateQueries({ queryKey: ["user-profile"] });
    },
  });
}

export function useJoinSharedEvent(id: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: () => postJson<void>(`/api/shared-events/${id}/join`),
    onSuccess: () => {
      invalidateAllFirstPages(queryClient, ["shared-events"]);
      invalidateAllFirstPages(queryClient, ["ranking"]);
      invalidateAllFirstPages(queryClient, ["feed"]);
      queryClient.invalidateQueries({ queryKey: ["user-profile"] });
    },
  });
}

export function useLeaveSharedEvent(id: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: () => postJson<void>(`/api/shared-events/${id}/leave`),
    onSuccess: () => {
      invalidateAllFirstPages(queryClient, ["shared-events"]);
      invalidateAllFirstPages(queryClient, ["ranking"]);
      invalidateAllFirstPages(queryClient, ["feed"]);
      queryClient.invalidateQueries({ queryKey: ["user-profile"] });
    },
  });
}

export function useCloseSharedEvent(id: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: () => postJson<void>(`/api/shared-events/${id}/close`),
    onSuccess: () => {
      invalidateAllFirstPages(queryClient, ["shared-events"]);
      invalidateAllFirstPages(queryClient, ["ranking"]);
      invalidateAllFirstPages(queryClient, ["feed"]);
      queryClient.invalidateQueries({ queryKey: ["user-profile"] });
    },
  });
}

export function useRequestSharedEventParticipantRemoval(sharedEventId: string, participantId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: () =>
      postJson<void>(`/api/shared-events/${sharedEventId}/participants/${participantId}/request-removal`),
    onSuccess: () => {
      invalidateAllFirstPages(queryClient, ["shared-events"]);
      invalidateAllFirstPages(queryClient, ["ranking"]);
      invalidateAllFirstPages(queryClient, ["feed"]);
      queryClient.invalidateQueries({ queryKey: ["user-profile"] });
    },
  });
}

export function useVoteSharedEventParticipantRemoval(sharedEventId: string, participantId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (voteType: number) =>
      postJson<void>(`/api/shared-events/${sharedEventId}/participants/${participantId}/vote`, { voteType }),
    onSuccess: () => {
      invalidateAllFirstPages(queryClient, ["shared-events"]);
      invalidateAllFirstPages(queryClient, ["ranking"]);
      invalidateAllFirstPages(queryClient, ["feed"]);
      queryClient.invalidateQueries({ queryKey: ["user-profile"] });
    },
  });
}

function invalidateAllFirstPages(queryClient: QueryClient, baseKey: unknown[]) {
  const keys = queryClient.getQueriesData({ queryKey: baseKey, type: "all" });
  keys.forEach(([key]) => {
    invalidateFirstPage(queryClient, key as unknown[]);
  });
}
