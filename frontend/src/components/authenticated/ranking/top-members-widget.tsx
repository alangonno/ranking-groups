import { AppCard } from "../../ui/app-card";

interface TopMember {
  position: number;
  name: string;
  points: number;
  avatar: string;
}

interface TopMembersWidgetProps {
  members: TopMember[];
}

export function TopMembersWidget({ members }: TopMembersWidgetProps) {
  return (
    <AppCard className="shadow-[0_1px_3px_rgba(0,0,0,0.05)] p-5">
      <div className="flex items-center justify-between mb-4">
        <h3 className="font-semibold text-text-primary">Top 5 Membros</h3>
        <span className="text-xs text-primary font-medium cursor-pointer hover:underline">
          Ver todos
        </span>
      </div>
      <div className="space-y-3">
        {members.map((member) => (
          <div
            key={member.position}
            className="flex items-center gap-3 p-2 -mx-2 rounded-lg hover:bg-gray-50 transition-colors cursor-pointer"
          >
            <span
              className={`text-sm font-bold w-5 ${
                member.position <= 2 ? "text-primary" : "text-gray-700"
              }`}
            >
              {member.position}
            </span>
            <div className="w-8 h-8 rounded-full bg-primary-light flex items-center justify-center text-primary font-bold text-xs">
              {member.avatar}
            </div>
            <span className="flex-1 text-sm font-medium text-text-primary truncate">
              {member.name}
            </span>
            <span className="text-xs font-medium bg-gray-100 text-text-secondary px-2.5 py-1 rounded-full">
              {member.points.toLocaleString()} pts
            </span>
          </div>
        ))}
      </div>
    </AppCard>
  );
}
