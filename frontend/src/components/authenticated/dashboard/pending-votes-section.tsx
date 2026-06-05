import { AppBadge } from "../../ui/app-badge";
import { VotingCard } from "../events/voting-card";
import type { Event } from "../../../types/event/event";

interface PendingVotesSectionProps {
  events: Event[];
}

export function PendingVotesSection({ events }: PendingVotesSectionProps) {
  if (events.length === 0) {
    return (
      <div className="bg-surface-container-low dark:bg-surface-container rounded-xl p-4 text-center">
        <p className="text-sm font-medium text-text-primary">
          Nenhuma validação pendente!
        </p>
      </div>
    );
  }

  return (
    <div>
      <div className="flex items-center gap-2 mb-3">
        <h2 className="text-base font-semibold text-text-primary">Pendentes</h2>
        <AppBadge color="red" size="xs">
          {events.length} ações
        </AppBadge>
      </div>

      <div
        className="flex gap-3 overflow-x-auto pb-2 snap-x snap-mandatory scrollbar-hide"
        style={{ scrollbarWidth: "none", msOverflowStyle: "none" }}
      >
        {events.map((event) => (
          <VotingCard key={event.id} event={event} compact />
        ))}
      </div>
    </div>
  );
}
