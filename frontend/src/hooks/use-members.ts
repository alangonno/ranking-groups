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
        ranking: Array<{
          userId: string;
          position: number;
        }>;
      }>(`/api/groups/${groupId}`);

      const positionMap = new Map(
        (data.ranking || []).map((r) => [r.userId, r.position])
      );

      return (data.members || []).map((m) => ({
        userId: m.userId,
        name: m.name,
        username: "",
        email: "",
        avatar: m.name.charAt(0).toUpperCase(),
        role: ROLE_MAP[m.role] ?? 3,
        currentScore: m.currentScore,
        rankPosition: positionMap.get(m.userId) ?? 0,
      }));
    },
    enabled: !!groupId,
    staleTime: 5 * 60 * 1000,
  });
}
