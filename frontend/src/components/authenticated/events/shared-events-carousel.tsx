import { useEffect, useRef, useState } from "react";
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
  onOpenDetails?: (eventId: string) => void;
}

export function SharedEventsCarousel({ events, onOpenDetails }: SharedEventsCarouselProps) {
  const trackRef = useRef<HTMLDivElement | null>(null);
  const isDraggingRef = useRef(false);
  const startXRef = useRef(0);
  const startScrollLeftRef = useRef(0);
  const movedRef = useRef(false);
  const [isDragging, setIsDragging] = useState(false);

  useEffect(() => {
    function handleMouseMove(event: MouseEvent) {
      const track = trackRef.current;
      if (!track || !isDraggingRef.current) return;

      const delta = event.clientX - startXRef.current;
      if (Math.abs(delta) > 4) {
        movedRef.current = true;
      }

      track.scrollLeft = startScrollLeftRef.current - delta;
    }

    function stopDragging() {
      isDraggingRef.current = false;
      setIsDragging(false);
    }

    window.addEventListener("mousemove", handleMouseMove);
    window.addEventListener("mouseup", stopDragging);

    return () => {
      window.removeEventListener("mousemove", handleMouseMove);
      window.removeEventListener("mouseup", stopDragging);
    };
  }, []);

  function handleMouseDown(event: React.MouseEvent<HTMLDivElement>) {
    const track = trackRef.current;
    if (!track) return;

    isDraggingRef.current = true;
    movedRef.current = false;
    startXRef.current = event.clientX;
    startScrollLeftRef.current = track.scrollLeft;
    setIsDragging(true);
  }

  function handleClickCapture(event: React.MouseEvent<HTMLDivElement>) {
    if (!movedRef.current) return;

    event.preventDefault();
    event.stopPropagation();
    movedRef.current = false;
  }

  if (events.length === 0) return null;

  return (
    <div>
      <div className="mb-3">
        <h3 className="text-base font-semibold text-text-primary">
          Eventos em Grupo Ativos
        </h3>
      </div>
      <div
        ref={trackRef}
        onMouseDown={handleMouseDown}
        onClickCapture={handleClickCapture}
        onDragStart={(event) => event.preventDefault()}
        className={`-mx-1 flex gap-3 overflow-x-auto px-1 pb-3 snap-x snap-mandatory scroll-smooth select-none scrollbar-hide ${
          isDragging ? "cursor-grabbing" : "cursor-grab"
        }`}
        style={{ scrollbarWidth: "none", msOverflowStyle: "none" }}
      >
        {events.map((event) => (
          <div key={event.id} className="w-[280px] flex-none sm:w-[320px] snap-start">
            <SharedEventCard
              event={event}
              onOpenDetails={onOpenDetails}
              layout="carousel"
            />
          </div>
        ))}
      </div>
    </div>
  );
}
