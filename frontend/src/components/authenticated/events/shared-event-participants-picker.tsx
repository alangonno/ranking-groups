import { useMemo, useState } from "react";
import { Check, Search, Users } from "lucide-react";
import { useMembers } from "../../../hooks/use-members";
import { AppInput } from "../../ui/app-input";

interface SharedEventParticipantsPickerProps {
  groupId: string;
  selectedUserIds: string[];
  onChange: (userIds: string[]) => void;
  disabled?: boolean;
}

export function SharedEventParticipantsPicker({
  groupId,
  selectedUserIds,
  onChange,
  disabled = false,
}: SharedEventParticipantsPickerProps) {
  const [search, setSearch] = useState("");
  const { data: membersData } = useMembers(groupId);
  const members = membersData?.flattened ?? [];

  const filteredMembers = useMemo(() => {
    const normalizedSearch = search.trim().toLowerCase();

    return members
      .filter((member) => {
        if (!normalizedSearch) return true;
        return member.name.toLowerCase().includes(normalizedSearch);
      })
      .sort((first, second) => {
        const firstSelected = selectedUserIds.includes(first.userId) ? 0 : 1;
        const secondSelected = selectedUserIds.includes(second.userId) ? 0 : 1;

        if (firstSelected !== secondSelected) {
          return firstSelected - secondSelected;
        }

        return first.name.localeCompare(second.name, "pt-BR");
      });
  }, [members, search, selectedUserIds]);

  function toggleUser(userId: string) {
    if (disabled) return;

    const isSelected = selectedUserIds.includes(userId);
    onChange(
      isSelected
        ? selectedUserIds.filter((selectedUserId) => selectedUserId !== userId)
        : [...selectedUserIds, userId]
    );
  }

  return (
    <div className="space-y-3">
      <label className="block text-sm font-medium text-text-secondary">
        Participantes iniciais <span className="text-text-muted">(opcional)</span>
      </label>

      <div className="relative">
        <Search size={16} className="pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-text-secondary" />
        <AppInput
          value={search}
          onChange={(event) => setSearch(event.target.value)}
          placeholder="Buscar membro"
          disabled={disabled}
          className="pl-9"
        />
      </div>

      <div className="rounded-xl border border-surface-container bg-surface-container-lowest p-2 max-h-52 overflow-y-auto space-y-2">
        {filteredMembers.length === 0 ? (
          <div className="flex items-center gap-2 px-3 py-4 text-sm text-text-secondary">
            <Users size={16} />
            <span>Nenhum membro encontrado</span>
          </div>
        ) : (
          filteredMembers.map((member) => {
            const isSelected = selectedUserIds.includes(member.userId);

            return (
              <button
                key={member.userId}
                type="button"
                onClick={() => toggleUser(member.userId)}
                disabled={disabled}
                className={`flex w-full items-center justify-between rounded-xl px-3 py-2 text-left transition-colors ${
                  isSelected
                    ? "bg-primary/10 text-primary"
                    : "bg-surface-container-low text-text-primary hover:bg-surface-container"
                } disabled:cursor-not-allowed disabled:opacity-60`}
              >
                <div className="min-w-0">
                  <p className="truncate text-sm font-medium">{member.name}</p>
                  <p className="text-xs text-text-secondary">#{member.rankPosition} no grupo</p>
                </div>

                <div
                  className={`flex h-6 w-6 items-center justify-center rounded-full border ${
                    isSelected
                      ? "border-primary bg-primary text-white"
                      : "border-surface-container-highest text-text-muted"
                  }`}
                >
                  {isSelected ? <Check size={14} /> : null}
                </div>
              </button>
            );
          })
        )}
      </div>

      {selectedUserIds.length > 0 ? (
        <p className="text-xs text-text-secondary">
          {selectedUserIds.length} participante(s) selecionado(s)
        </p>
      ) : null}
    </div>
  );
}
