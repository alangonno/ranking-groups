import { useQuery } from "@tanstack/react-query";
import type { GroupMemberProfile } from "../types/group/group";
import { mockMembers } from "../lib/mock-data";

export function useMembers(groupId: string) {
  return useQuery<GroupMemberProfile[]>({
    queryKey: ["members", groupId],
    queryFn: async () => {
      // TODO: Replace with real API call when backend is ready
      // return getJson<GroupMemberProfile[]>(`/api/groups/${groupId}/members`);
      await new Promise((resolve) => setTimeout(resolve, 300));
      return mockMembers;
    },
    enabled: !!groupId,
    staleTime: 5 * 60 * 1000,
  });
}
