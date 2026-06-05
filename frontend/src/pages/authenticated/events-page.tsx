import { useState, useEffect } from "react";
import { useParams, useLocation, useNavigate } from "react-router-dom";
import { Plus } from "lucide-react";
import { QuickActionCards } from "../../components/authenticated/events/quick-action-card";
import { SharedEventsCarousel } from "../../components/authenticated/events/shared-events-carousel";
import { EventCard } from "../../components/authenticated/events/event-card";
import { VotingCard } from "../../components/authenticated/events/voting-card";
import { CreateEventModal } from "../../components/authenticated/events/create-event-modal";
import { CreateSharedEventModal } from "../../components/authenticated/events/create-shared-event-modal";
import { NotificationDropdown } from "../../components/authenticated/notifications/notification-dropdown";
import { EventStatus } from "../../types/event/event";
import type { Event } from "../../types/event/event";
import { useGroupEvents } from "../../hooks/use-events";
import { useGroup } from "../../hooks/use-groups";
import { useGroupSharedEvents } from "../../hooks/use-shared-events";
import { useInfiniteScroll } from "../../hooks/use-infinite-scroll";
import { useAuthContext } from "../../providers/auth-provider";

export function EventsPage() {
  const { groupId } = useParams<{ groupId: string }>();
  const location = useLocation();
  const navigate = useNavigate();
  const [activeTab, setActiveTab] = useState<"all" | "my" | "voting">("all");
  const { user } = useAuthContext();
  const [showCreateEvent, setShowCreateEvent] = useState(false);
  const [showCreateSharedEvent, setShowCreateSharedEvent] = useState(false);

  const [userAvatarError, setUserAvatarError] = useState(false);

  useEffect(() => {
    if (location.state?.createEvent) {
      setShowCreateEvent(true);
      navigate(location.pathname, { replace: true });
    }
  }, [location.state?.createEvent]);

  const { data: group } = useGroup(groupId || "");
  const {
    data: eventsData,
    hasNextPage,
    fetchNextPage,
    isFetchingNextPage,
  } = useGroupEvents(groupId || "");
  const { data: sharedEventsData } = useGroupSharedEvents(groupId || "");

  const allEvents = eventsData?.flattened ?? [];
  const sharedEvents = sharedEventsData?.flattened ?? [];

  const { sentinelRef } = useInfiniteScroll({
    onIntersect: () => fetchNextPage(),
    hasMore: !!hasNextPage,
    isLoading: isFetchingNextPage,
  });

  const isPendingEvent = (e: Event) =>
    e.status === EventStatus.Pending || e.isPendingRemoval;

  const myEvents = allEvents.filter(
    (e) => e.createdByUserId === user?.id || e.affectedUserId === user?.id
  );

  const votingEvents = allEvents.filter(isPendingEvent);

  const sourceEvents =
    activeTab === "voting"
      ? votingEvents
      : activeTab === "my"
        ? myEvents.filter((e) => !isPendingEvent(e))
        : allEvents.filter((e) => !isPendingEvent(e));

  const displayEvents = [...sourceEvents]
    .filter((event, index, self) => index === self.findIndex((e) => e.id === event.id))
    .sort((a, b) =>
      new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
    );

  const sharedEventsForCarousel = sharedEvents
    .filter((se) => !se.isClosed)
    .map((se) => ({
    id: se.id,
    title: se.title,
    points: se.points,
    participantCount: se.participantCount,
    isClosed: se.isClosed,
    createdByUserId: se.createdByUserId,
    createdByUserAvatarUrl: se.createdByUserAvatarUrl,
    hasCurrentUserJoined: se.hasCurrentUserJoined,
    closesAt: se.closesAt,
    imageUrl: se.imageUrl,
  }));

  return (
    <div className="p-4 lg:p-8 max-w-5xl mx-auto">
      {/* Header Mobile */}
      <div className="lg:hidden flex items-center justify-between mb-5">
        <div>
          <h1 className="text-xl font-bold text-text-primary">Eventos</h1>
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
          onCreateEvent={() => setShowCreateEvent(true)}
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
            { key: "voting" as const, label: "Votação" },
          ].map((tab) => (
          <button
            key={tab.key}
            type="button"
            onClick={() => setActiveTab(tab.key)}
            className={`px-4 py-2 rounded-full text-sm font-medium transition-all flex items-center gap-1.5 ${
              activeTab === tab.key
                ? "bg-surface-container-lowest text-text-primary shadow-sm border border-border dark:bg-surface dark:border-surface-container-high"
                : "bg-surface-container-low text-text-secondary hover:bg-surface-container dark:bg-surface-container dark:hover:bg-surface-container-high"
            }`}
          >
            {tab.label}
            {tab.key === "voting" && votingEvents.length > 0 && (
              <span className="ml-1.5 inline-flex items-center text-white justify-center w-5 h-5 bg-primary text-on-primary rounded-full text-[10px] font-bold">
                {votingEvents.length}
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
              {activeTab === "voting"
                ? "Nenhuma votação no momento"
                : "Nenhum evento nesta categoria"}
            </p>
          </div>
        ) : (
          displayEvents.map((event) =>
            activeTab === "voting" ? (
              <VotingCard key={event.id} event={event} />
            ) : (
              <EventCard key={event.id} event={event} />
            )
          )
        )}
      </div>

      {/* Infinite scroll sentinel */}
      <div ref={sentinelRef} className="py-4 flex justify-center">
        {isFetchingNextPage && (
          <span className="text-sm text-text-secondary">Carregando mais...</span>
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
