import { useQuery } from "@tanstack/react-query";
import { getJson } from "../lib/api";
import type { GroupDetailsResponse, GroupMemberProfile } from "../types/group/group";

export function useMembers(groupId: string) {
  return useQuery<GroupMemberProfile[]>({
    queryKey: ["group-details", groupId],
    queryFn: async () => {
      const data = await getJson<GroupDetailsResponse>(`/api/groups/${groupId}`);
      return data.members;
    },
    enabled: !!groupId,
    staleTime: 5 * 60 * 1000,
  });
}
