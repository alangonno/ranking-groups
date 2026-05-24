import { useQuery } from "@tanstack/react-query";
import { getJson } from "../lib/api";
import type { RankingEntry, RankingQueryParams } from "../types/ranking/ranking";

export function useRanking(groupId: string, params?: RankingQueryParams) {
  return useQuery<RankingEntry[]>({
    queryKey: ["ranking", groupId, params],
    queryFn: () =>
      getJson<RankingEntry[]>(`/api/rankings/group/${groupId}`, params as Record<string, unknown>),
    enabled: !!groupId,
  });
}

export function useFeed(groupId: string, limit?: number) {
  return useQuery<unknown[]>({
    queryKey: ["feed", groupId, limit],
    queryFn: () =>
      getJson<unknown[]>(`/api/rankings/group/${groupId}/feed`, {
        limit,
      } as Record<string, unknown>),
    enabled: !!groupId,
  });
}
