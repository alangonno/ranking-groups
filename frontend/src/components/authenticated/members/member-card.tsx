import { useNavigate } from "react-router-dom";
import { GroupRole } from "../../../types/group/group";
import type { GroupMemberProfile } from "../../../types/group/group";
import { AppBadge } from "../../ui/app-badge";

interface MemberCardProps {
  member: GroupMemberProfile;
  groupId: string;
}

function roleLabel(role: GroupRole): string {
  switch (role) {
    case GroupRole.Owner:
      return "Owner";
    case GroupRole.Admin:
      return "Admin";
    case GroupRole.Member:
      return "Member";
    default:
      return "Member";
  }
}

function roleBadgeColor(role: GroupRole): "red" | "blue" | "gray" {
  switch (role) {
    case GroupRole.Owner:
      return "red";
    case GroupRole.Admin:
      return "blue";
    case GroupRole.Member:
      return "gray";
    default:
      return "gray";
  }
}

function rankColorClass(rank: number): string {
  if (rank === 1) return "text-primary";
  if (rank === 2) return "text-text-primary";
  return "text-text-muted";
}

export function MemberCard({ member, groupId }: MemberCardProps) {
  const navigate = useNavigate();
  const isFirst = member.rankPosition === 1;

  function handleClick() {
    navigate(`/group/${groupId}/profile/${member.userId}`);
  }

  return (
    <div
      onClick={handleClick}
      className={`bg-white rounded-xl p-4 cursor-pointer transition-all hover:shadow-md ${
        isFirst
          ? "border-2 border-primary shadow-[0_2px_8px_rgba(0,0,0,0.04)]"
          : "border border-border shadow-[0_1px_3px_rgba(0,0,0,0.05)]"
      }`}
    >
      <div className="flex items-start gap-4">
        {/* Avatar */}
        <div className="w-14 h-14 rounded-full bg-primary-light flex items-center justify-center text-primary font-bold text-lg shrink-0">
          {member.avatar}
        </div>

        {/* Info */}
        <div className="flex-1 min-w-0">
          <div className="flex items-start justify-between gap-2">
            <div className="min-w-0">
              <h3 className="text-base font-bold text-text-primary truncate">
                {member.name}
              </h3>
              <p className="text-sm text-text-secondary">{roleLabel(member.role)}</p>
            </div>
            <span className={`text-2xl font-bold ${rankColorClass(member.rankPosition)}`}>
              #{member.rankPosition}
            </span>
          </div>

          <div className="mt-3 flex items-center justify-between">
            <div>
              <p className="text-xs text-text-muted uppercase tracking-wide">Total Score</p>
              <p className="text-xl font-bold text-text-primary">
                {member.currentScore.toLocaleString()}
              </p>
            </div>
            <AppBadge color={roleBadgeColor(member.role)} size="sm">
              {roleLabel(member.role)}
            </AppBadge>
          </div>
        </div>
      </div>
    </div>
  );
}
