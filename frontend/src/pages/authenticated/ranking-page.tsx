import { useState, useMemo } from "react";
import { useParams } from "react-router-dom";
import { useAutoAnimate } from "@formkit/auto-animate/react";
import { PodiumCard } from "../../components/authenticated/ranking/podium-card";
import { RankingListItem } from "../../components/authenticated/ranking/ranking-list-item";
import { RankingFilter } from "../../components/authenticated/ranking/ranking-filter";
import { SearchInput } from "../../components/authenticated/ranking/search-input";
import { useRanking } from "../../hooks/use-ranking";
import { useAuthContext } from "../../providers/auth-provider";

export function RankingPage() {
  const { groupId } = useParams<{ groupId: string }>();
  const { user } = useAuthContext();
  const [filter, setFilter] = useState("month");
  const [search, setSearch] = useState("");
  const [parent] = useAutoAnimate();

  const { data: ranking = [] } = useRanking(groupId || "");

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

  return (
    <div className="p-4 lg:p-8 max-w-5xl mx-auto">
      {/* Header Mobile */}
      <div className="lg:hidden mb-5">
        <h1 className="text-xl font-bold text-text-primary">Ranking</h1>
        <p className="text-sm text-text-secondary">Grupo: {groupId}</p>
      </div>

      {/* Header Desktop */}
      <div className="hidden lg:flex items-center justify-between mb-6">
        <div>
          <h1 className="text-2xl font-bold text-on-surface">Ranking do Grupo</h1>
          <p className="text-body-md font-body-md text-secondary">
            Veja quem está no topo da competição
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
            avatar={top1.user.name
              .split(" ")
              .map((n) => n[0])
              .join("")
              .toUpperCase()
              .slice(0, 2)}
          />
        )}
        {top2 && (
          <PodiumCard
            position={2}
            name={top2.user.name}
            points={top2.score}
            avatar={top2.user.name
              .split(" ")
              .map((n) => n[0])
              .join("")
              .toUpperCase()
              .slice(0, 2)}
            growth={450}
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
            avatar={member.user.name
              .split(" ")
              .map((n) => n[0])
              .join("")
              .toUpperCase()
              .slice(0, 2)}
            maxPoints={maxPoints}
            isCurrentUser={member.user.id === currentUserId}
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
        <div className="lg:hidden fixed bottom-20 left-4 right-4 bg-white rounded-xl shadow-lg border border-border p-3 z-40">
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-3">
              <span className="text-sm font-bold text-primary">
                #{currentUserPosition + 1}
              </span>
              <div className="w-8 h-8 rounded-full bg-primary-light flex items-center justify-center text-primary font-bold text-xs">
                {currentUserData.user.name
                  .split(" ")
                  .map((n) => n[0])
                  .join("")
                  .toUpperCase()
                  .slice(0, 2)}
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
