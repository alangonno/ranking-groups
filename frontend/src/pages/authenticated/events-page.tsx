import { useState, useEffect } from "react";
import { useParams, useLocation, useNavigate } from "react-router-dom";
import { Plus } from "lucide-react";
import { QuickActionCards } from "../../components/authenticated/events/quick-action-card";
import { SharedEventsCarousel } from "../../components/authenticated/events/shared-events-carousel";
import { EventCard } from "../../components/authenticated/events/event-card";
import { VotingCard } from "../../components/authenticated/events/voting-card";
import { CreateEventModal } from "../../components/authenticated/events/create-event-modal";
import { CreateSharedEventModal } from "../../components/authenticated/events/create-shared-event-modal";
import { EventStatus } from "../../types/event/event";
import { useGroupEvents } from "../../hooks/use-events";
import { useGroup } from "../../hooks/use-groups";
import { useGroupSharedEvents } from "../../hooks/use-shared-events";
import { useAuthContext } from "../../providers/auth-provider";

export function EventsPage() {
  const { groupId } = useParams<{ groupId: string }>();
  const location = useLocation();
  const navigate = useNavigate();
  const [activeTab, setActiveTab] = useState<"all" | "my">("all");
  const { user } = useAuthContext();
  const [showCreateEvent, setShowCreateEvent] = useState(false);
  const [showCreateSharedEvent, setShowCreateSharedEvent] = useState(false);

  const userInitials = user?.name
    ? user.name
        .split(" ")
        .map((n) => n[0])
        .join("")
        .toUpperCase()
        .slice(0, 2)
    : "U";

  useEffect(() => {
    if (location.state?.createEvent) {
      setShowCreateEvent(true);
      navigate(location.pathname, { replace: true });
    }
  }, [location.state?.createEvent]);

  const { data: group } = useGroup(groupId || "");
  const { data: allEvents = [] } = useGroupEvents(groupId || "");
  const { data: sharedEvents = [] } = useGroupSharedEvents(groupId || "");

  const myEvents = allEvents.filter(
    (e) => e.createdByUserId === user?.id || e.affectedUserId === user?.id
  );

  const filteredEvents = activeTab === "my" ? myEvents : allEvents;

  const displayEvents = [...filteredEvents]
    .filter((event, index, self) => index === self.findIndex((e) => e.id === event.id))
    .sort((a, b) => {
    const aPriority = a.status === EventStatus.Pending || a.isPendingRemoval ? 0 : 1;
    const bPriority = b.status === EventStatus.Pending || b.isPendingRemoval ? 0 : 1;
    if (aPriority !== bPriority) return aPriority - bPriority;
    return new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime();
  });

  const sharedEventsForCarousel = sharedEvents.map((se) => ({
    id: se.id,
    title: se.title,
    points: se.points,
    participantCount: se.participantCount,
    isClosed: se.isClosed,
    createdByUserId: se.createdByUserId,
    hasCurrentUserJoined: se.hasCurrentUserJoined,
    closesAt: se.closesAt,
  }));

  return (
    <div className="p-4 lg:p-8 max-w-5xl mx-auto">
      {/* Header Mobile */}
      <div className="lg:hidden flex items-center justify-between mb-5">
        <div>
          <h1 className="text-xl font-bold text-text-primary">Eventos</h1>
          <p className="text-sm text-text-secondary">{group?.name || "Grupo"}</p>
        </div>
        <button
          type="button"
          onClick={() => navigate(`/group/${groupId}/profile/${user?.id}`)}
          className="w-9 h-9 rounded-full bg-primary-light flex items-center justify-center text-primary font-bold text-sm hover:bg-primary-light/70 transition-colors"
        >
          {userInitials}
        </button>
      </div>

      {/* Header Desktop */}
      <div className="hidden lg:flex items-center justify-between mb-6">
        <div>
          <h1 className="text-2xl font-bold text-text-primary">
            Eventos
          </h1>
          <p className="text-sm text-text-secondary">
            {group?.name || "Grupo"}
          </p>
        </div>
        <button
          type="button"
          onClick={() => setShowCreateEvent(true)}
          className="flex items-center gap-2 py-2 px-4 rounded-full bg-primary text-white font-medium hover:bg-primary-hover transition-colors shadow-sm"
        >
          <Plus size={18} />
          Novo Evento
        </button>
      </div>

      {/* Quick Actions */}
      <div className="mb-6">
        <QuickActionCards
          onCreatePositive={() => setShowCreateEvent(true)}
          onCreateNegative={() => setShowCreateEvent(true)}
          onCreateShared={() => setShowCreateSharedEvent(true)}
        />
      </div>

      {/* Shared Events Carousel */}
      <div className="mb-6">
        <SharedEventsCarousel events={sharedEventsForCarousel} />
      </div>

      {/* Tabs */}
      <div className="mb-4">
        <div className="flex gap-2 flex-wrap">
          {[
          { key: "all" as const, label: "Todos" },
          { key: "my" as const, label: "Seus Eventos" },
        ].map((tab) => (
          <button
            key={tab.key}
            type="button"
            onClick={() => setActiveTab(tab.key)}
            className={`px-4 py-2 rounded-full text-sm font-medium transition-all flex items-center gap-1.5 ${
              activeTab === tab.key
                ? "bg-white text-text-primary shadow-sm border border-border"
                : "bg-gray-100 text-text-secondary hover:bg-gray-200"
            }`}
          >
            {tab.label}
          </button>
        ))}
        </div>
      </div>

      {/* Feed */}
      <div className="space-y-3">
        {displayEvents.length === 0 ? (
          <div className="text-center py-12">
            <p className="text-text-secondary text-sm mb-4">
              Nenhum evento nesta categoria
            </p>
          </div>
        ) : (
          displayEvents.map((event) =>
            event.status === EventStatus.Pending || event.isPendingRemoval ? (
              <VotingCard key={event.id} event={event} />
            ) : (
              <EventCard key={event.id} event={event} />
            )
          )
        )}
      </div>

      {/* Modals */}
      <CreateEventModal
        isOpen={showCreateEvent}
        onClose={() => setShowCreateEvent(false)}
        groupId={groupId || ""}
      />
      <CreateSharedEventModal
        isOpen={showCreateSharedEvent}
        onClose={() => setShowCreateSharedEvent(false)}
        groupId={groupId || ""}
      />
    </div>
  );
}
