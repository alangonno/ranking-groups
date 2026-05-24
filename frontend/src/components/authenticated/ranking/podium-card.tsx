import { AppCard } from "../../ui/app-card";

interface PodiumCardProps {
  position: number;
  name: string;
  points: number;
  avatar: string;
  growth?: number;
  badges?: string[];
}

export function PodiumCard({
  position,
  name,
  points,
  avatar,
  growth,
  badges = [],
}: PodiumCardProps) {
  const isFirst = position === 1;

  if (isFirst) {
    return (
      <AppCard className="shadow-[0_4px_12px_rgba(0,0,0,0.08)] p-6 flex-1 relative overflow-hidden">
        {/* Background decoration */}
        <div className="absolute top-0 right-0 w-32 h-32 bg-primary/5 rounded-full -translate-y-1/2 translate-x-1/2" />
        
        <div className="relative flex flex-col items-center text-center">
          {/* Badge #1 */}
          <div className="absolute -top-1 -right-1 w-7 h-7 bg-primary text-white rounded-full flex items-center justify-center text-xs font-bold">
            1
          </div>
          
          {/* Avatar */}
          <div className="w-24 h-24 rounded-full bg-gradient-to-br from-yellow-400 via-primary to-red-600 p-1 mb-4">
            <div className="w-full h-full rounded-full bg-white flex items-center justify-center text-2xl font-bold text-text-primary">
              {avatar}
            </div>
          </div>
          
          <h3 className="text-lg font-bold text-text-primary">{name}</h3>
          <p className="text-4xl font-bold text-primary mt-2">
            {points.toLocaleString()}
            <span className="text-lg font-medium text-text-secondary ml-1">pts</span>
          </p>
          
          {badges.length > 0 && (
            <div className="flex gap-2 mt-3">
              {badges.map((badge) => (
                <span
                  key={badge}
                  className="bg-gray-100 text-text-secondary text-xs font-medium px-2.5 py-1 rounded-full"
                >
                  {badge}
                </span>
              ))}
            </div>
          )}
        </div>
      </AppCard>
    );
  }

  // Second place
  return (
    <AppCard className="shadow-[0_2px_8px_rgba(0,0,0,0.04)] p-5 flex flex-col items-center text-center w-48">
      {/* Badge #2 */}
      <div className="w-6 h-6 bg-gray-400 text-white rounded-full flex items-center justify-center text-xs font-bold mb-3">
        2
      </div>
      
      {/* Avatar */}
      <div className="w-20 h-20 rounded-full bg-gradient-to-br from-gray-300 to-gray-500 p-1 mb-3">
        <div className="w-full h-full rounded-full bg-white flex items-center justify-center text-xl font-bold text-text-primary">
          {avatar}
        </div>
      </div>
      
      <h3 className="text-base font-bold text-text-primary">{name}</h3>
      <p className="text-2xl font-bold text-text-primary mt-1">
        {points.toLocaleString()}
        <span className="text-sm font-medium text-text-secondary ml-1">pts</span>
      </p>
      
      {growth !== undefined && (
        <span className="text-xs font-medium text-success mt-2">
          ↗ +{growth} this week
        </span>
      )}
    </AppCard>
  );
}
