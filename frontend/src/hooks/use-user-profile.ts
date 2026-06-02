import { useQuery } from "@tanstack/react-query";
import { getJson } from "../lib/api";
import type { GroupUserProfileResponse } from "../types/group/group";
import type { GroupRole } from "../types/group/group";


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
        timeline: Array<{
          id: string;
          itemType: string;
          title: string;
          description: string;
          points: number;
          type?: string;
          status?: string;
          createdAt: string;
          createdByUserId: string;
          createdByUserName: string;
          affectedUserId?: string;
          affectedUserName?: string;
          scoreBalance: number;
          isClosed?: boolean;
          participantCount?: number;
          isPendingRemoval: boolean;
          removalVoteDeadline?: string;
          quorumRequired?: number;
          removeCount?: number;
          keepCount?: number;
          approvals?: Array<{
            userId: string;
            userName: string;
            voteType: string;
            createdAt: string;
          }>;
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
          hasCurrentUserJoined: false,
          createdAt: "",
          groupId: "",
          createdByUserId: "",
          createdByUserName: se.createdByUserName,
          participantCount: se.participantCount,
          userRole: se.userRole as "organizer" | "participant",
        })),
        timeline: (response.timeline || []).map((t: any) => ({
          id: t.id,
          itemType: t.itemType as "event" | "shared_event",
          title: t.title,
          description: t.description,
          points: t.points,
          type: t.type,
          status: t.status,
          createdAt: t.createdAt,
          createdByUserId: t.createdByUserId,
          createdByUserName: t.createdByUserName,
          affectedUserId: t.affectedUserId,
          affectedUserName: t.affectedUserName,
          scoreBalance: t.scoreBalance,
          isClosed: t.isClosed,
          participantCount: t.participantCount,
          isPendingRemoval: t.isPendingRemoval ?? false,
          removalVoteDeadline: t.removalVoteDeadline,
          quorumRequired: t.quorumRequired,
          removeCount: t.removeCount,
          keepCount: t.keepCount,
          approvals: t.approvals,
        })),
      };
    },
    enabled: !!groupId && !!userId,
    staleTime: 5 * 60 * 1000,
  });
}
