import { useMemo, useEffect } from "react";
import { useGroup } from "./use-groups";
import { useGroupEvents } from "./use-events";
import { useFeed, useRanking } from "./use-ranking";
import { useUserProfile } from "./use-user-profile";
import { useCurrentUser } from "./use-auth";
import { setLastGroupId } from "../lib/group-storage";
import { EventStatus } from "../types/event/event";
import type { DashboardFeedEntry, TopMemberEntry } from "../types/ranking/ranking";

function getUserInitials(name: string | undefined): string {
  if (!name) return "U";
  return name
    .split(" ")
    .map((n) => n[0])
    .join("")
    .toUpperCase()
    .slice(0, 2);
}

export function useDashboardData(groupId: string | undefined) {
  const { data: user } = useCurrentUser();
  const { data: group } = useGroup(groupId || "");
  const groupEventsQuery = useGroupEvents(groupId || "");
  const feedQuery = useFeed(groupId || "");
  const { data: ranking = [] } = useRanking(groupId || "");
  const { data: profile } = useUserProfile(
    groupId || "",
    user?.id || ""
  );

  const groupEvents = groupEventsQuery.data?.flattened ?? [];
  const feedItems = feedQuery.data?.flattened ?? [];

  useEffect(() => {
    if (groupId) {
      setLastGroupId(groupId);
    }
  }, [groupId]);

  const userInitials = getUserInitials(user?.name);

  const pendingEvents = useMemo(
    () => groupEvents.filter((e) => e.status === EventStatus.Pending),
    [groupEvents]
  );

  const approvedEvents = useMemo(
    () => groupEvents.filter((e) => e.status === EventStatus.Approved),
    [groupEvents]
  );

  const topMembers: TopMemberEntry[] = useMemo(
    () =>
      ranking.slice(0, 5).map((entry, index) => ({
        position: index + 1,
        name: entry.user?.name || "",
        points: entry.score,
        avatarUrl: entry.user?.avatarUrl,
      })),
    [ranking]
  );

  const allFeedItems: DashboardFeedEntry[] = useMemo(() => {
    const approved: Array<DashboardFeedEntry> = approvedEvents.map((e) => ({
      type: "event",
      event: e,
    }));

    const shared: Array<DashboardFeedEntry> = feedItems
      .filter((item) => item.feedItemType === "shared_event")
      .map((item) => ({ type: "shared_event", item }));

    const deduped = new Map<string, DashboardFeedEntry>();
    [...approved, ...shared].forEach((entry) => {
      const id = entry.type === "event" ? entry.event.id : entry.item.id;
      if (!deduped.has(id)) {
        deduped.set(id, entry);
      }
    });

    return Array.from(deduped.values()).sort((a, b) => {
      const dateA =
        a.type === "event"
          ? new Date(a.event.createdAt).getTime()
          : new Date(a.item.createdAt).getTime();
      const dateB =
        b.type === "event"
          ? new Date(b.event.createdAt).getTime()
          : new Date(b.item.createdAt).getTime();
      return dateB - dateA;
    });
  }, [approvedEvents, feedItems]);

  const totalEvents = approvedEvents.length + pendingEvents.length;
  const activeMembersCount = ranking.length;

  const currentUserScore = useMemo(() => {
    if (profile?.member?.currentScore !== undefined) {
      return profile.member.currentScore;
    }
    const userEntry = ranking.find((r) => r.user?.id === user?.id);
    return userEntry?.score ?? 0;
  }, [profile?.member?.currentScore, ranking, user?.id]);

  const todayDelta = useMemo(() => {
    if (!profile?.timeline || profile.timeline.length === 0) return 0;

    const now = new Date();
    const startOfToday = new Date(now.getFullYear(), now.getMonth(), now.getDate());
    const endOfToday = new Date(now.getFullYear(), now.getMonth(), now.getDate() + 1);

    return profile.timeline.reduce((sum, item) => {
      const itemDate = new Date(item.createdAt);
      if (itemDate < startOfToday || itemDate >= endOfToday) return sum;

      if (item.itemType === "shared_event") {
        return sum + item.points;
      }

      if (item.itemType === "event") {
        if (item.status !== "Approved") return sum;
        return item.type === "Negative" ? sum - item.points : sum + item.points;
      }

      return sum;
    }, 0);
  }, [profile?.timeline]);

  return {
    user,
    userInitials,
    group,
    groupEvents,
    pendingEvents,
    approvedEvents,
    allFeedItems,
    topMembers,
    totalEvents,
    activeMembersCount,
    profile,
    todayDelta,
    currentUserScore,
    hasMoreFeed: feedQuery.hasNextPage,
    fetchMoreFeed: feedQuery.fetchNextPage,
    isFetchingMoreFeed: feedQuery.isFetchingNextPage,
  };
}

