interface RankingListItemProps {
  position: number;
  name: string;
  points: number;
  weeklyScore: number;
  avatarUrl?: string;
  maxPoints: number;
  isCurrentUser?: boolean;
}

export function RankingListItem({
  position,
  name,
  points,
  weeklyScore,
  avatarUrl,
  maxPoints,
  isCurrentUser = false,
}: RankingListItemProps) {
  const fallbackAvatar = name.charAt(0).toUpperCase();
  const progress = Math.round((points / maxPoints) * 100);

  return (
    <div
      className={`bg-surface-container-lowest rounded-xl p-4 shadow-sm border flex items-center gap-4 transition-transform duration-200 hover:scale-[0.99] cursor-pointer ${
        isCurrentUser
          ? "border-primary/30 bg-primary-container/5"
          : "border-surface-container"
      }`}
    >
      <span
        className={`w-8 text-center text-headline-md font-headline-md ${
          position <= 2 ? "text-primary" : "text-secondary"
        }`}
      >
        {position}
      </span>

      {avatarUrl ? (
        <img
          src={avatarUrl}
          alt={name}
          className="w-11 h-11 rounded-full object-cover flex-shrink-0"
        />
      ) : (
        <div className="w-11 h-11 rounded-full bg-surface-container flex items-center justify-center text-secondary font-bold text-sm flex-shrink-0">
          {fallbackAvatar}
        </div>
      )}

      <div className="flex-1 min-w-0">
        <p className="text-body-md font-body-md font-bold text-on-surface">
          {name}
          {isCurrentUser && (
            <span className="text-primary ml-1.5 text-caption font-caption font-bold">
              Você
            </span>
          )}
          {weeklyScore > 0 && (
            <span className="text-primary ml-2 text-caption font-caption">
              ↑ +{weeklyScore.toLocaleString()} esta semana
            </span>
          )}
        </p>
        <div className="hidden sm:block mt-2">
          <div className="h-1.5 w-full bg-surface-container rounded-full overflow-hidden">
            <div
              className={`h-full rounded-full transition-all duration-500 ${
                position === 1 ? "bg-primary" : "bg-primary-fixed-dim"
              }`}
              style={{ width: `${Math.min(progress, 100)}%` }}
            />
          </div>
        </div>
      </div>

      <div className="text-right flex-shrink-0">
        <p className="text-headline-md font-headline-md text-on-surface">
          {points.toLocaleString()}
        </p>
        <p className="text-caption font-caption text-secondary">pts</p>
      </div>
    </div>
  );
}
