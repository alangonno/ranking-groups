import { useQuery } from "@tanstack/react-query";
import { mockEvents, mockMembers, mockUserSharedEvents } from "../lib/mock-data";
import { EventStatus, EventType } from "../types/event/event";
import type { EventWithScoreBalance } from "../types/event/event";
import type { GroupMemberProfile } from "../types/group/group";

interface UserProfileData {
  member: GroupMemberProfile | undefined;
  events: EventWithScoreBalance[];
  sharedEvents: typeof mockUserSharedEvents;
}

export function useUserProfile(groupId: string, userId: string) {
  return useQuery<UserProfileData>({
    queryKey: ["user-profile", groupId, userId],
    queryFn: async () => {
      // TODO: Replace with real API call when backend is ready
      await new Promise((resolve) => setTimeout(resolve, 300));

      const member = mockMembers.find((m) => m.userId === userId);

      const userEvents = mockEvents.filter(
        (e) =>
          e.groupId === groupId &&
          e.affectedUserId === userId &&
          e.status === EventStatus.Approved
      );

      const sortedEvents = [...userEvents].sort(
        (a, b) =>
          new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime()
      );

      let balance = 0;
      const eventsWithBalance: EventWithScoreBalance[] = sortedEvents.map(
        (event) => {
          const signedPoints =
            event.type === EventType.Positive ? event.points : -event.points;
          balance += signedPoints;
          return { ...event, scoreBalance: balance };
        }
      );

      return {
        member,
        events: eventsWithBalance,
        sharedEvents: mockUserSharedEvents,
      };
    },
    enabled: !!groupId && !!userId,
    staleTime: 5 * 60 * 1000,
  });
}
