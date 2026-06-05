import { SharedEventCard } from "./shared-event-card";

interface SharedEvent {
  id: string;
  title: string;
  points: number;
  participantCount: number;
  isClosed: boolean;
  createdByUserId: string;
  createdByUserAvatarUrl?: string;
  hasCurrentUserJoined: boolean;
  closesAt?: string;
  imageUrl?: string;
}

interface SharedEventsCarouselProps {
  events: SharedEvent[];
}

export function SharedEventsCarousel({ events }: SharedEventsCarouselProps) {
  if (events.length === 0) return null;

  return (
    <div>
      <div className="flex items-center justify-between mb-3">
        <h3 className="text-base font-semibold text-text-primary">
          Eventos em Grupo
        </h3>
        <span className="text-xs text-primary font-medium cursor-pointer hover:underline">
          Ver todos
        </span>
      </div>
      <div
        className="flex gap-3 overflow-x-auto pb-2 snap-x snap-mandatory"
        style={{ scrollbarWidth: "none", msOverflowStyle: "none" }}
      >
        {events.map((event) => (
          <SharedEventCard key={event.id} event={event} />
        ))}
      </div>
    </div>
  );
}
