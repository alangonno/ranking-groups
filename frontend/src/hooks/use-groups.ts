import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { getJson, postJson } from "../lib/api";
import type {
  CreateGroupRequest,
  Group,
  JoinGroupRequest,
  UserGroupSummary,
  CreateGroupBackendResponse,
  JoinGroupBackendResponse,
} from "../types/group/group";

export function useGroups() {
  return useQuery<Group[]>({
    queryKey: ["groups"],
    queryFn: async () => {
      const response = await getJson<{ groups: UserGroupSummary[] }>("/api/groups");
      return response.groups.map((g) => ({
        id: g.groupId,
        name: g.name,
        inviteCode: g.inviteCode,
        description: undefined,
        createdByUserId: "",
        createdByUser: undefined,
        createdAt: "",
        updatedAt: undefined,
      }));
    },
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
      queryClient.invalidateQueries({ queryKey: ["groups"] });
    },
  });
}

export function useJoinGroup() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (payload: JoinGroupRequest) =>
      postJson<JoinGroupBackendResponse>("/api/groups/join", payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["groups"] });
    },
  });
}

export function useLeaveGroup(groupId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: () => postJson<void>(`/api/groups/${groupId}/leave`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["groups"] });
      queryClient.invalidateQueries({ queryKey: ["groups", groupId] });
    },
  });
}
