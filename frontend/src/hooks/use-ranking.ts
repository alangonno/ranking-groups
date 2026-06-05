import { useInfiniteQuery, useQuery } from "@tanstack/react-query";
import { getJson } from "../lib/api";
import { flattenPages, getNextPageParam } from "../lib/cursor-utils";
import { getAvatarUrl } from "../lib/image-url";
import type { RankingEntry, RankingQueryParams, FeedItem } from "../types/ranking/ranking";
import type { User } from "../types/auth/user";

export function useRanking(groupId: string, params?: RankingQueryParams) {
  return useQuery<RankingEntry[]>({
    queryKey: ["ranking", groupId, params],
    queryFn: async () => {
      const response = await getJson<{
        members: Array<{ userId: string; name: string; score: number; position: number; weeklyScore: number; avatarUrl?: string }>;
      }>(`/api/rankings/group/${groupId}`, params as Record<string, unknown>);
      return (response.members || []).map((m) => ({
        user: { id: m.userId, name: m.name, avatarUrl: getAvatarUrl(m.avatarUrl) } as User,
        score: m.score,
        weeklyScore: m.weeklyScore,
      }));
    },
    enabled: !!groupId,
  });
}

export function useFeed(groupId: string) {
  return useInfiniteQuery({
    queryKey: ["feed", groupId],
    queryFn: async ({ pageParam }) => {
      const response = await getJson<{
        items: FeedItem[];
        hasMore: boolean;
        nextCursor: string | null;
      }>(`/api/rankings/group/${groupId}/feed`, { cursor: pageParam });
      return {
        items: (response.items || []).map((item) => ({
          ...item,
          commentCount: item.commentCount ?? 0,
        })),
        hasMore: response.hasMore,
        nextCursor: response.nextCursor,
      };
    },
    getNextPageParam,
    initialPageParam: undefined as string | undefined,
    enabled: !!groupId,
    select: (data) => ({
      ...data,
      flattened: flattenPages(data.pages),
    }),
  });
}
