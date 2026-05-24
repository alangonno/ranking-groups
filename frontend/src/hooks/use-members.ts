import { useQuery } from "@tanstack/react-query";
import { getJson } from "../lib/api";
import type { GroupMemberProfile } from "../types/group/group";
import type { GroupRole } from "../types/group/group";

const ROLE_MAP: Record<string, GroupRole> = {
  Owner: 1,
  Admin: 2,
  Member: 3,
};

export function useMembers(groupId: string) {
  return useQuery<GroupMemberProfile[]>({
    queryKey: ["group-details", groupId],
    queryFn: async () => {
      const data = await getJson<{
        members: Array<{
          userId: string;
          name: string;
          role: string;
          currentScore: number;
        }>;
      }>(`/api/groups/${groupId}`);
      return (data.members || []).map((m, _index) => ({
        userId: m.userId,
        name: m.name,
        username: "",
        email: "",
        avatar: m.name.charAt(0).toUpperCase(),
        role: ROLE_MAP[m.role] ?? 3,
        currentScore: m.currentScore,
        rankPosition: 0,
      }));
    },
    enabled: !!groupId,
    staleTime: 5 * 60 * 1000,
  });
}
