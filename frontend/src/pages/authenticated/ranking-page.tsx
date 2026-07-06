import { useState, useMemo } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { useAutoAnimate } from "@formkit/auto-animate/react";
import { PodiumCard } from "../../components/authenticated/ranking/podium-card";
import { RankingListItem } from "../../components/authenticated/ranking/ranking-list-item";
import { RankingFilter } from "../../components/authenticated/ranking/ranking-filter";
import { SearchInput } from "../../components/authenticated/ranking/search-input";
import { NotificationDropdown } from "../../components/authenticated/notifications/notification-dropdown";
import { useRanking } from "../../hooks/use-ranking";
import { useGroup } from "../../hooks/use-groups";
import { useAuthContext } from "../../providers/auth-provider";
import type { RankingQueryParams } from "../../types/ranking/ranking";

function getRankingQueryParams(filter: string): RankingQueryParams {
  const now = new Date();
  const todayStart = new Date(now.getFullYear(), now.getMonth(), now.getDate());
  const todayEnd = new Date(now.getFullYear(), now.getMonth(), now.getDate(), 23, 59, 59, 999);

  switch (filter) {
    case "month": {
      const start = new Date(todayStart.getFullYear(), todayStart.getMonth(), 1);
      return { fromDate: start.toISOString(), toDate: todayEnd.toISOString() };
    }
    case "last-month": {
      const start = new Date(todayStart.getFullYear(), todayStart.getMonth() - 1, 1);
      const end = new Date(todayStart.getFullYear(), todayStart.getMonth(), 0, 23, 59, 59, 999);
      return { fromDate: start.toISOString(), toDate: end.toISOString() };
    }
    case "last-year": {
      const start = new Date(todayStart.getFullYear() - 1, todayStart.getMonth(), todayStart.getDate());
      return { fromDate: start.toISOString(), toDate: todayEnd.toISOString() };
    }
    case "all":
    default:
      return {};
  }
}

export function RankingPage() {
  const { groupId } = useParams<{ groupId: string }>();
  const navigate = useNavigate();
  const { user } = useAuthContext();
  const [filter, setFilter] = useState("month");
  const [search, setSearch] = useState("");
  const [parent] = useAutoAnimate();

  const [userAvatarError, setUserAvatarError] = useState(false);

  const queryParams = useMemo(() => getRankingQueryParams(filter), [filter]);

  const { data: group } = useGroup(groupId || "");
  const { data: ranking = [] } = useRanking(groupId || "", queryParams);

  const filteredRanking = useMemo(() => {
    let data = [...ranking];
    if (search.trim()) {
      data = data.filter((m) =>
        m.user.name.toLowerCase().includes(search.toLowerCase())
      );
    }
    return data.sort((a, b) => b.score - a.score);
  }, [search, ranking]);

  const top1 = filteredRanking[0];
  const top2 = filteredRanking[1];
  const rest = filteredRanking.slice(2);

  const currentUserId = user?.id || "";
  const currentUserPosition = filteredRanking.findIndex(
    (m) => m.user.id === currentUserId
  );
  const currentUserData =
    currentUserPosition >= 0 ? filteredRanking[currentUserPosition] : null;

  const maxPoints = filteredRanking[0]?.score || 1;

  function navigateToProfile(userId: string) {
    navigate(`/group/${groupId}/profile/${userId}`);
  }

  return (
    <div className="p-4 lg:p-8 max-w-5xl mx-auto">
      {/* Header Mobile */}
      <div className="lg:hidden flex items-center justify-between mb-5">
        <div>
          <h1 className="text-xl font-bold text-text-primary">Ranking</h1>
          <p className="text-sm text-text-secondary">{group?.name || "Grupo"}</p>
        </div>
        <div className="flex items-center gap-2">
          <NotificationDropdown />
          <button
            type="button"
            onClick={() => navigate(`/group/${groupId}/profile/${user?.id}`)}
            className="w-9 h-9 rounded-full flex items-center justify-center overflow-hidden hover:ring-2 hover:ring-primary transition-all"
          >
            {user?.avatarUrl && !userAvatarError ? (
              <img
                src={user.avatarUrl}
                alt={user.name}
                className="w-full h-full rounded-full object-cover flex-shrink-0"
                onError={() => setUserAvatarError(true)}
              />
            ) : (
              <div className="w-full h-full rounded-full bg-primary-light flex items-center justify-center text-primary font-bold text-sm">
                {user?.name?.charAt(0).toUpperCase() || "U"}
              </div>
            )}
          </button>
        </div>
      </div>

      {/* Header Desktop */}
      <div className="hidden lg:flex items-center justify-between mb-6">
        <div>
          <h1 className="text-2xl font-bold text-on-surface">Ranking</h1>
          <p className="text-body-md font-body-md text-secondary">
            {group?.name || "Grupo"}
          </p>
        </div>
      </div>

      {/* Controls */}
      <div className="flex flex-col sm:flex-row gap-3 mb-6">
        <SearchInput
          value={search}
          onChange={setSearch}
          placeholder="Buscar membros..."
        />
        <RankingFilter value={filter} onChange={setFilter} />
      </div>

      {/* Bento Podium */}
      <div className="grid grid-cols-1 md:grid-cols-12 gap-4 mb-8">
        {top1 && (
          <PodiumCard
            position={1}
            name={top1.user.name}
            points={top1.score}
            avatarUrl={top1.user.avatarUrl}
            weeklyScore={top1.weeklyScore}
            onClick={() => navigateToProfile(top1.user.id)}
          />
        )}
        {top2 && (
          <PodiumCard
            position={2}
            name={top2.user.name}
            points={top2.score}
            avatarUrl={top2.user.avatarUrl}
            weeklyScore={top2.weeklyScore}
            onClick={() => navigateToProfile(top2.user.id)}
          />
        )}
      </div>

      {/* List */}
      <div ref={parent} className="space-y-3">
        {rest.map((member, index) => (
          <RankingListItem
            key={member.user.id}
            position={index + 3}
            name={member.user.name}
            points={member.score}
            weeklyScore={member.weeklyScore}
            avatarUrl={member.user.avatarUrl}
            maxPoints={maxPoints}
            isCurrentUser={member.user.id === currentUserId}
            onClick={() => navigateToProfile(member.user.id)}
          />
        ))}
      </div>

      {/* Empty state */}
      {filteredRanking.length === 0 && (
        <div className="text-center py-16">
          <p className="text-text-secondary">
            Ninguém pontuou neste período
          </p>
        </div>
      )}

      {/* Sticky User Row - Mobile */}
      {currentUserData && currentUserPosition >= 5 && (
        <div
          className="lg:hidden fixed bottom-20 left-4 right-4 bg-surface-container-lowest dark:bg-surface rounded-xl shadow-lg border border-border p-3 z-40 cursor-pointer"
          onClick={() => navigateToProfile(currentUserData.user.id)}
        >
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-3">
              <span className="text-sm font-bold text-primary">
                #{currentUserPosition + 1}
              </span>
              <div className="w-8 h-8 rounded-full bg-primary-light flex items-center justify-center text-primary font-bold text-xs overflow-hidden">
                {currentUserData.user.avatarUrl ? (
                  <img
                    src={currentUserData.user.avatarUrl}
                    alt={currentUserData.user.name}
                    className="w-full h-full rounded-full object-cover flex-shrink-0"
                  />
                ) : (
                  currentUserData.user.name
                    .split(" ")
                    .map((n) => n[0])
                    .join("")
                    .toUpperCase()
                    .slice(0, 2)
                )}
              </div>
              <span className="text-sm font-medium text-text-primary">
                Você
              </span>
            </div>
            <span className="text-sm font-bold text-text-primary">
              {currentUserData.score.toLocaleString()} pts
            </span>
          </div>
        </div>
      )}
    </div>
  );
}
