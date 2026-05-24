import { useQuery } from "@tanstack/react-query";
import { getJson } from "../lib/api";
import type { GroupUserProfileResponse } from "../types/group/group";
import type { GroupRole } from "../types/group/group";
import type { User } from "../types/auth/user";

const ROLE_MAP: Record<string, GroupRole> = {
  Owner: 1,
  Admin: 2,
  Member: 3,
};

export function useUserProfile(groupId: string, userId: string) {
  return useQuery<GroupUserProfileResponse>({
    queryKey: ["user-profile", groupId, userId],
    queryFn: async () => {
      const response = await getJson<{
        member: {
          userId: string;
          name: string;
          username: string;
          email: string;
          avatarUrl?: string;
          role: string;
          currentScore: number;
          rankPosition: number;
        };
        events: Array<{
          eventId: string;
          title: string;
          description: string;
          points: number;
          type: string;
          status: string;
          createdAt: string;
          createdByUserId: string;
          createdByUserName: string;
          affectedUserId: string;
          affectedUserName: string;
          scoreBalance: number;
        }>;
        sharedEvents: Array<{
          id: string;
          title: string;
          description: string;
          points: number;
          isClosed: boolean;
          createdByUserName: string;
          participantCount: number;
          userRole: string;
        }>;
      }>(`/api/groups/${groupId}/members/${userId}`);
      return {
        member: {
          userId: response.member.userId,
          name: response.member.name,
          username: response.member.username,
          email: response.member.email,
          avatar: response.member.avatarUrl || response.member.name.charAt(0).toUpperCase(),
          role: ROLE_MAP[response.member.role] ?? 3,
          currentScore: response.member.currentScore,
          rankPosition: response.member.rankPosition,
        },
        events: response.events.map((e) => ({
          id: e.eventId,
          title: e.title,
          description: e.description,
          points: e.points,
          type: e.type,
          status: e.status,
          createdAt: e.createdAt,
          createdByUserId: e.createdByUserId,
          createdByUserName: e.createdByUserName,
          affectedUserId: e.affectedUserId,
          affectedUserName: e.affectedUserName,
          scoreBalance: e.scoreBalance,
        })),
        sharedEvents: response.sharedEvents.map((se) => ({
          id: se.id,
          title: se.title,
          description: se.description,
          points: se.points,
          isClosed: se.isClosed,
          createdAt: "",
          groupId: "",
          createdByUserId: "",
          createdByUserName: se.createdByUserName,
          participantCount: se.participantCount,
          userRole: se.userRole as "organizer" | "participant",
        })),
      };
    },
    enabled: !!groupId && !!userId,
    staleTime: 5 * 60 * 1000,
  });
}
