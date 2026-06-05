import { useInfiniteQuery } from "@tanstack/react-query";
import { getJson } from "../lib/api";
import { flattenPages, getNextPageParam } from "../lib/cursor-utils";
import type { GroupRole } from "../types/group/group";

const ROLE_MAP: Record<string, GroupRole> = {
  Owner: 1,
  Admin: 2,
  Member: 3,
};

export function useMembers(groupId: string) {
  return useInfiniteQuery({
    queryKey: ["group-details", groupId],
    queryFn: async ({ pageParam }) => {
      const data = await getJson<{
        members: Array<{
          userId: string;
          name: string;
          role: string;
          currentScore: number;
        }>;
        membersHasMore: boolean;
        membersNextCursor: string | null;
        ranking: Array<{
          userId: string;
          position: number;
        }>;
      }>(`/api/groups/${groupId}`, { membersCursor: pageParam });

      const positionMap = new Map(
        (data.ranking || []).map((r) => [r.userId, r.position])
      );

      const items = (data.members || []).map((m) => ({
        userId: m.userId,
        name: m.name,
        username: "",
        email: "",
        avatar: m.name.charAt(0).toUpperCase(),
        role: ROLE_MAP[m.role] ?? 3,
        currentScore: m.currentScore,
        rankPosition: positionMap.get(m.userId) ?? 0,
      }));

      return {
        items,
        hasMore: data.membersHasMore,
        nextCursor: data.membersNextCursor,
      };
    },
    getNextPageParam,
    initialPageParam: undefined as string | undefined,
    enabled: !!groupId,
    staleTime: 5 * 60 * 1000,
    select: (data) => ({
      ...data,
      flattened: flattenPages(data.pages),
    }),
  });
}
