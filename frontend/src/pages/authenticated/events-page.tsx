import { useState } from "react";
import { useParams } from "react-router-dom";
import { Plus } from "lucide-react";
import { QuickActionCards } from "../../components/authenticated/events/quick-action-card";
import { SharedEventsCarousel } from "../../components/authenticated/events/shared-events-carousel";
import { EventCard } from "../../components/authenticated/events/event-card";
import { VotingCard } from "../../components/authenticated/events/voting-card";
import { CreateEventModal } from "../../components/authenticated/events/create-event-modal";
import { CreateSharedEventModal } from "../../components/authenticated/events/create-shared-event-modal";
import { EventStatus } from "../../types/event/event";
import { useGroupEvents } from "../../hooks/use-events";
import { useGroupSharedEvents } from "../../hooks/use-shared-events";

export function EventsPage() {
  const { groupId } = useParams<{ groupId: string }>();
  const [activeTab, setActiveTab] = useState<"all" | "personal" | "shared" | "pending">("all");
  const [showCreateEvent, setShowCreateEvent] = useState(false);
  const [showCreateSharedEvent, setShowCreateSharedEvent] = useState(false);

  const { data: allEvents = [] } = useGroupEvents(groupId || "");
  const { data: sharedEvents = [] } = useGroupSharedEvents(groupId || "");

  const pendingEvents = allEvents.filter((e) => e.status === EventStatus.Pending);
  const approvedEvents = allEvents.filter((e) => e.status === EventStatus.Approved);

  const displayEvents =
    activeTab === "pending"
      ? pendingEvents
      : activeTab === "personal"
      ? approvedEvents
      : activeTab === "shared"
      ? []
      : allEvents;

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
      <div className="lg:hidden mb-5">
        <h1 className="text-xl font-bold text-text-primary">Eventos</h1>
        <p className="text-sm text-text-secondary">Grupo: {groupId}</p>
      </div>

      {/* Header Desktop */}
      <div className="hidden lg:flex items-center justify-between mb-6">
        <div>
          <h1 className="text-2xl font-bold text-text-primary">
            Feed de Eventos
          </h1>
          <p className="text-sm text-text-secondary">
            Acompanhe as atividades do grupo
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
          { key: "all" as const, label: "Todos", count: 0 },
          { key: "personal" as const, label: "Pessoais", count: 0 },
          { key: "shared" as const, label: "Compartilhados", count: 0 },
          { key: "pending" as const, label: "Pendentes", count: pendingEvents.length },
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
            {tab.count > 0 && (
              <span className="bg-primary text-white text-[10px] font-bold w-4 h-4 rounded-full flex items-center justify-center">
                {tab.count}
              </span>
            )}
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
            event.status === EventStatus.Pending ? (
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
