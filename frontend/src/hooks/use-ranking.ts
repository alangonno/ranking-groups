import { useQuery } from "@tanstack/react-query";
import { getJson } from "../lib/api";
import type { RankingEntry, RankingQueryParams } from "../types/ranking/ranking";
import type { User } from "../types/auth/user";

export function useRanking(groupId: string, params?: RankingQueryParams) {
  return useQuery<RankingEntry[]>({
    queryKey: ["ranking", groupId, params],
    queryFn: async () => {
      const response = await getJson<{
        members: Array<{ userId: string; name: string; score: number; position: number }>;
      }>(`/api/rankings/group/${groupId}`, params as Record<string, unknown>);
      return (response.members || []).map((m) => ({
        user: { id: m.userId, name: m.name } as User,
        score: m.score,
      }));
    },
    enabled: !!groupId,
  });
}

export function useFeed(groupId: string, limit?: number) {
  return useQuery<unknown[]>({
    queryKey: ["feed", groupId, limit],
    queryFn: async () => {
      const response = await getJson<{ feed: unknown[] }>(
        `/api/rankings/group/${groupId}/feed`,
        { limit } as Record<string, unknown>
      );
      return response.feed || [];
    },
    enabled: !!groupId,
  });
}
