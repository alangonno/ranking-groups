import { useState } from "react";
import { useParams } from "react-router-dom";
import { QuickActionCards } from "../../components/authenticated/events/quick-action-card";
import { SharedEventsCarousel } from "../../components/authenticated/events/shared-events-carousel";
import { EventCard } from "../../components/authenticated/events/event-card";
import { VotingCard } from "../../components/authenticated/events/voting-card";
import { mockEvents, mockSharedEvents } from "../../lib/mock-data";
import { EventStatus } from "../../types/event/event";

export function EventsPage() {
  const { groupId } = useParams<{ groupId: string }>();
  const [activeTab, setActiveTab] = useState<"all" | "personal" | "shared" | "pending">("all");

  const allEvents = mockEvents;
  const pendingEvents = mockEvents.filter((e) => e.status === EventStatus.Pending);
  const approvedEvents = mockEvents.filter((e) => e.status === EventStatus.Approved);

  const displayEvents =
    activeTab === "pending"
      ? pendingEvents
      : activeTab === "personal"
      ? approvedEvents
      : activeTab === "shared"
      ? []
      : allEvents;

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
      </div>

      {/* Quick Actions */}
      <div className="mb-6">
        <QuickActionCards />
      </div>

      {/* Shared Events Carousel */}
      <div className="mb-6">
        <SharedEventsCarousel events={mockSharedEvents} />
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
    </div>
  );
}
