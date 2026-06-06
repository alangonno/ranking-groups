interface TopMember {
  position: number;
  name: string;
  points: number;
  avatarUrl?: string;
}

interface TopMembersWidgetProps {
  members: TopMember[];
  onSeeAll?: () => void;
}

export function TopMembersWidget({ members, onSeeAll }: TopMembersWidgetProps) {
  return (
    <div className="bg-surface-container-lowest rounded-xl p-5 shadow-sm border border-surface-container">
      <div className="flex items-center justify-between mb-4">
        <h3 className="text-headline-md font-headline-md text-on-surface">Top 5 Membros</h3>
        <button type="button" onClick={onSeeAll} className="text-caption font-caption text-primary cursor-pointer hover:underline">
          Ver todos
        </button>
      </div>
      <div className="space-y-3">
        {members.map((member) => (
          <div
            key={member.position}
            className="flex items-center gap-3 p-2 rounded-lg hover:bg-surface-container-low transition-colors cursor-pointer"
          >
            <span
              className={`text-headline-md font-headline-md font-bold w-6 text-center ${
                member.position <= 2 ? "text-primary" : "text-secondary"
              }`}
            >
              {member.position}
            </span>
            {member.avatarUrl ? (
              <img
                src={member.avatarUrl}
                alt={member.name}
                className="w-8 h-8 rounded-full object-cover"
              />
            ) : (
              <div className="w-8 h-8 rounded-full bg-primary-light flex items-center justify-center text-primary font-bold text-xs">
                {member.name.charAt(0).toUpperCase()}
              </div>
            )}
            <span className="flex-1 text-body-md font-body-md font-semibold text-on-surface truncate">
              {member.name}
            </span>
            <span className="text-caption font-caption font-medium bg-surface-container-high text-on-surface px-2.5 py-1 rounded-full whitespace-nowrap">
              {member.points.toLocaleString()} pts
            </span>
          </div>
        ))}
      </div>
    </div>
  );
}
