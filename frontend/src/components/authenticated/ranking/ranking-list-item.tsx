import { AppProgress } from "../../ui/app-progress";

interface RankingListItemProps {
  position: number;
  name: string;
  points: number;
  avatar: string;
  maxPoints: number;
  isCurrentUser?: boolean;
}

export function RankingListItem({
  position,
  name,
  points,
  avatar,
  maxPoints,
  isCurrentUser = false,
}: RankingListItemProps) {
  const progress = Math.round((points / maxPoints) * 100);

  return (
    <div
      className={`flex items-center gap-4 p-4 rounded-xl transition-colors ${
        isCurrentUser
          ? "bg-red-50 border border-primary/20"
          : "bg-white shadow-[0_1px_3px_rgba(0,0,0,0.05)] hover:bg-gray-50"
      }`}
    >
      <span
        className={`text-lg font-bold w-6 ${
          position <= 2 ? "text-primary" : "text-gray-700"
        }`}
      >
        {position}
      </span>

      <div className="w-10 h-10 rounded-full bg-gray-100 flex items-center justify-center text-text-secondary font-bold text-sm flex-shrink-0">
        {avatar}
      </div>

      <div className="flex-1 min-w-0">
        <p className="text-sm font-semibold text-text-primary">
          {name}
          {isCurrentUser && (
            <span className="text-primary ml-1.5 text-xs font-bold">
              Você
            </span>
          )}
        </p>
        <div className="mt-1.5">
          <AppProgress progress={progress} size="sm" color="red" />
        </div>
      </div>

      <div className="text-right flex-shrink-0">
        <p className="text-sm font-bold text-text-primary">
          {points.toLocaleString()}
        </p>
        <p className="text-xs text-text-muted">pts</p>
      </div>
    </div>
  );
}
