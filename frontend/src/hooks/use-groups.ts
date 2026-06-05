import { useInfiniteQuery, useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { getJson, postJson } from "../lib/api";
import { flattenPages, getNextPageParam } from "../lib/cursor-utils";
import { invalidateFirstPage } from "../lib/query-utils";
import type {
  CreateGroupRequest,
  Group,
  JoinGroupRequest,
  UserGroupSummary,
  CreateGroupBackendResponse,
  JoinGroupBackendResponse,
} from "../types/group/group";

export function useGroups() {
  return useInfiniteQuery({
    queryKey: ["groups"],
    queryFn: async ({ pageParam }) => {
      const response = await getJson<{
        groups: UserGroupSummary[];
        hasMore: boolean;
        nextCursor: string | null;
      }>("/api/groups", { cursor: pageParam });
      return {
        items: (response.groups || []).map((g) => ({
          id: g.groupId,
          name: g.name,
          inviteCode: g.inviteCode,
          description: undefined,
          createdByUserId: "",
          createdByUser: undefined,
          createdAt: "",
          updatedAt: undefined,
        })),
        hasMore: response.hasMore,
        nextCursor: response.nextCursor,
      };
    },
    getNextPageParam,
    initialPageParam: undefined as string | undefined,
    select: (data) => ({
      ...data,
      flattened: flattenPages(data.pages),
    }),
  });
}

export function useGroup(groupId: string) {
  return useQuery<Group>({
    queryKey: ["groups", groupId],
    queryFn: async () => {
      const response = await getJson<{
        groupId: string;
        name: string;
        description?: string;
        inviteCode: string;
      }>(`/api/groups/${groupId}`);
      return {
        id: response.groupId,
        name: response.name,
        description: response.description,
        inviteCode: response.inviteCode,
        createdByUserId: "",
        createdByUser: undefined,
        createdAt: "",
        updatedAt: undefined,
      };
    },
    enabled: !!groupId,
  });
}

export function useCreateGroup() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (payload: CreateGroupRequest) =>
      postJson<CreateGroupBackendResponse>("/api/groups", payload),
    onSuccess: () => {
      invalidateFirstPage(queryClient, ["groups"]);
    },
  });
}

export function useJoinGroup() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (payload: JoinGroupRequest) =>
      postJson<JoinGroupBackendResponse>("/api/groups/join", payload),
    onSuccess: () => {
      invalidateFirstPage(queryClient, ["groups"]);
    },
  });
}

export function useLeaveGroup(groupId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: () => postJson<void>(`/api/groups/${groupId}/leave`),
    onSuccess: () => {
      invalidateFirstPage(queryClient, ["groups"]);
      queryClient.invalidateQueries({ queryKey: ["groups", groupId] });
    },
  });
}
