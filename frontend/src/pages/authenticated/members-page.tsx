import { useState, useMemo } from "react";
import { useParams } from "react-router-dom";
import { useMembers } from "../../hooks/use-members";
import { useGroup } from "../../hooks/use-groups";
import { MemberCard } from "../../components/authenticated/members/member-card";
import { SearchInput } from "../../components/authenticated/ranking/search-input";
import { AppSelect } from "../../components/ui/app-select";

export function MembersPage() {
  const { groupId } = useParams<{ groupId: string }>();
  const { data: members, isLoading } = useMembers(groupId || "");
  const { data: group } = useGroup(groupId || "");
  const [search, setSearch] = useState("");
  const [sort, setSort] = useState<"score" | "alphabetical">("score");

  const filteredMembers = useMemo(() => {
    if (!members) return [];
    let data = [...members];
    if (search.trim()) {
      const term = search.toLowerCase();
      data = data.filter((m) => m.name.toLowerCase().includes(term));
    }
    if (sort === "alphabetical") {
      data.sort((a, b) => a.name.localeCompare(b.name));
    } else {
      data.sort((a, b) => b.currentScore - a.currentScore);
    }
    return data;
  }, [members, search, sort]);

  return (
    <div className="p-4 lg:p-8 max-w-5xl mx-auto">
      {/* Header Mobile */}
      <div className="lg:hidden mb-5">
        <h1 className="text-xl font-bold text-text-primary">Membros</h1>
        <p className="text-sm text-text-secondary">
          {members?.length || 0} membros ativos
        </p>
      </div>

      {/* Header Desktop */}
      <div className="hidden lg:flex items-center justify-between mb-6">
        <div>
          <h1 className="text-2xl font-bold text-text-primary">
            {group?.name || "Grupo"}
          </h1>
          <p className="text-sm text-text-secondary">
            {members?.length || 0} Active Members
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
        <AppSelect
          value={sort}
          onChange={(e) => setSort(e.target.value as "score" | "alphabetical")}
          className="w-full sm:w-48"
        >
          <option value="score">Highest Score</option>
          <option value="alphabetical">Alphabetical</option>
        </AppSelect>
      </div>

      {/* Loading */}
      {isLoading && (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {Array.from({ length: 6 }).map((_, i) => (
            <div
              key={i}
              className="bg-white rounded-xl border border-border p-4 animate-pulse"
            >
              <div className="flex items-start gap-4">
                <div className="w-14 h-14 rounded-full bg-gray-200 shrink-0" />
                <div className="flex-1 space-y-2">
                  <div className="h-4 bg-gray-200 rounded w-3/4" />
                  <div className="h-3 bg-gray-200 rounded w-1/2" />
                  <div className="h-6 bg-gray-200 rounded w-1/3 mt-3" />
                </div>
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Grid */}
      {!isLoading && (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {filteredMembers.map((member) => (
            <MemberCard
              key={member.userId}
              member={member}
              groupId={groupId || ""}
            />
          ))}
        </div>
      )}

      {/* Empty state */}
      {!isLoading && filteredMembers.length === 0 && (
        <div className="text-center py-16">
          <p className="text-text-secondary">Nenhum membro encontrado</p>
        </div>
      )}
    </div>
  );
}
