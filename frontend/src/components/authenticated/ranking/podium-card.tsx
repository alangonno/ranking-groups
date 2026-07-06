interface PodiumCardProps {
  position: number;
  name: string;
  points: number;
  avatarUrl?: string;
  weeklyScore?: number;
  badges?: string[];
  onClick?: () => void;
}

export function PodiumCard({
  position,
  name,
  points,
  avatarUrl,
  weeklyScore,
  badges = [],
  onClick,
}: PodiumCardProps) {
  const fallbackAvatar = name.charAt(0).toUpperCase();
  const isFirst = position === 1;

  if (isFirst) {
    return (
      <div
        onClick={onClick}
        className="bg-surface-container-lowest rounded-xl p-6 shadow-sm border border-surface-container flex flex-col md:flex-row items-center gap-6 relative overflow-hidden transition-transform duration-200 hover:scale-[0.99] cursor-pointer md:col-span-8"
      >
        {/* Trophy decoration */}
        <div className="absolute -right-10 -top-10 text-primary/5 pointer-events-none">
          <span className="text-[200px] font-display">🏆</span>
        </div>

        <div className="flex-shrink-0 relative">
          <div className="w-28 h-28 md:w-32 md:h-32 rounded-full bg-gradient-to-br from-yellow-400 via-primary to-red-600 p-1">
            {avatarUrl ? (
              <img
                src={avatarUrl}
                alt={name}
                className="w-full h-full rounded-full object-cover"
              />
            ) : (
              <div className="w-full h-full rounded-full bg-surface-container-lowest flex items-center justify-center text-3xl font-bold text-on-surface">
                {fallbackAvatar}
              </div>
            )}
          </div>
          <div className="absolute -bottom-2 left-1/2 -translate-x-1/2 bg-primary text-on-primary px-3 py-1 rounded-full text-label-bold font-label-bold shadow-md border-2 border-surface-container-lowest whitespace-nowrap">
            #1
          </div>
        </div>

        <div className="flex-1 text-center md:text-left z-10">
          <h3 className="text-headline-md font-headline-md text-on-surface mb-1">{name}</h3>

          {badges.length > 0 && (
            <div className="flex flex-wrap justify-center md:justify-start gap-2 mb-3">
              {badges.map((badge) => (
                <span
                  key={badge}
                  className="bg-surface-container text-on-surface px-3 py-1 rounded-full text-caption font-caption"
                >
                  {badge}
                </span>
              ))}
            </div>
          )}

          <div className="flex items-end gap-2 justify-center md:justify-start">
            <span className="text-4xl md:text-display font-display font-bold text-primary">
              {points.toLocaleString()}
            </span>
            <span className="text-body-md font-body-md text-secondary pb-1">pts</span>
          </div>
          {weeklyScore !== undefined && weeklyScore > 0 && (
            <div className="mt-2 flex items-center gap-1 text-primary text-caption font-caption">
              <span className="text-lg">↑</span>
              +{weeklyScore.toLocaleString()} esta semana
            </div>
          )}
        </div>
      </div>
    );
  }

  return (
    <div
      onClick={onClick}
      className="bg-surface-container-lowest rounded-xl p-6 shadow-sm border border-surface-container flex flex-col items-center text-center transition-transform duration-200 hover:scale-[0.98] cursor-pointer md:col-span-4"
    >
      <div className="relative mb-4">
        <div className="w-20 h-20 md:w-24 md:h-24 rounded-full bg-gradient-to-br from-gray-300 to-gray-500 p-1">
          {avatarUrl ? (
            <img
              src={avatarUrl}
              alt={name}
              className="w-full h-full rounded-full object-cover"
            />
          ) : (
            <div className="w-full h-full rounded-full bg-surface-container-lowest flex items-center justify-center text-2xl font-bold text-on-surface">
              {fallbackAvatar}
            </div>
          )}
        </div>
        <div className="absolute -bottom-2 left-1/2 -translate-x-1/2 bg-surface-dim text-on-surface px-3 py-1 rounded-full text-label-bold font-label-bold border-2 border-surface-container-lowest whitespace-nowrap">
          #2
        </div>
      </div>

      <h3 className="text-body-lg font-body-lg font-bold text-on-surface">{name}</h3>

      <div className="text-headline-md font-headline-md text-on-surface mt-2">
        {points.toLocaleString()} pts
      </div>

      {weeklyScore !== undefined && weeklyScore > 0 && (
        <div className="mt-3 flex items-center gap-1 text-primary text-caption font-caption">
          <span className="text-lg">↑</span>
          +{weeklyScore.toLocaleString()} esta semana
        </div>
      )}
    </div>
  );
}
