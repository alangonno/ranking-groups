import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { getJson, postJson } from "../lib/api";
import type {
  CreateGroupRequest,
  CreateGroupResponse,
  Group,
  JoinGroupRequest,
  JoinGroupResponse,
} from "../types/group/group";

export function useGroups() {
  return useQuery<Group[]>({
    queryKey: ["groups"],
    queryFn: () => getJson<Group[]>("/api/groups"),
  });
}

export function useGroup(groupId: string) {
  return useQuery<Group>({
    queryKey: ["groups", groupId],
    queryFn: () => getJson<Group>(`/api/groups/${groupId}`),
    enabled: !!groupId,
  });
}

export function useCreateGroup() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (payload: CreateGroupRequest) =>
      postJson<CreateGroupResponse>("/api/groups", payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["groups"] });
    },
  });
}

export function useJoinGroup() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (payload: JoinGroupRequest) =>
      postJson<JoinGroupResponse>("/api/groups/join", payload),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: ["groups"] });
      queryClient.invalidateQueries({ queryKey: ["groups", variables.inviteCode] });
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
