import { useQuery } from "@tanstack/react-query";
import { getJson } from "../lib/api";
import type { GroupUserProfileResponse } from "../types/group/group";

export function useUserProfile(groupId: string, userId: string) {
  return useQuery<GroupUserProfileResponse>({
    queryKey: ["user-profile", groupId, userId],
    queryFn: () =>
      getJson<GroupUserProfileResponse>(`/api/groups/${groupId}/members/${userId}`),
    enabled: !!groupId && !!userId,
    staleTime: 5 * 60 * 1000,
  });
}
