import { useState } from "react";
import { useParams, Link, useNavigate } from "react-router-dom";
import { Plus, ArrowLeft } from "lucide-react";
import { AppButton } from "../../components/ui/app-button";
import { EventCard } from "../../components/authenticated/events/event-card";
import { VotingCard } from "../../components/authenticated/events/voting-card";
import { SharedEventCard } from "../../components/authenticated/events/shared-event-card";
import { TopMembersWidget } from "../../components/authenticated/ranking/top-members-widget";
import { HeroScoreCard } from "../../components/authenticated/dashboard/hero-score-card";
import { PendingVotesSection } from "../../components/authenticated/dashboard/pending-votes-section";
import { FeedTabs } from "../../components/authenticated/dashboard/feed-tabs";
import { CreateEventModal } from "../../components/authenticated/events/create-event-modal";
import { NotificationDropdown } from "../../components/authenticated/notifications/notification-dropdown";
import { useDashboardData } from "../../hooks/use-dashboard-data";
import { useInfiniteScroll } from "../../hooks/use-infinite-scroll";

export function DashboardPage() {
  const { groupId } = useParams<{ groupId: string }>();
  const navigate = useNavigate();
  const [activeTab, setActiveTab] = useState<"all" | "pending">("all");
  const [showCreateEvent, setShowCreateEvent] = useState(false);

  const {
    user,
    userInitials,
    group,
    pendingEvents,
    allFeedItems,
    topMembers,
    totalEvents,
    activeMembersCount,
    profile,
    hasMoreFeed,
    fetchMoreFeed,
    isFetchingMoreFeed,
  } = useDashboardData(groupId);

  const { sentinelRef } = useInfiniteScroll({
    onIntersect: () => fetchMoreFeed(),
    hasMore: !!hasMoreFeed,
    isLoading: isFetchingMoreFeed,
  });

  if (!groupId) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <p className="text-text-secondary">Grupo não encontrado</p>
      </div>
    );
  }

  return (
    <div className="p-4 lg:p-8 max-w-7xl mx-auto">
      {/* Header Mobile */}
      <div className="lg:hidden flex items-center justify-between mb-5">
        <div className="flex items-center gap-2">
          <Link
            to="/groups"
            className="w-8 h-8 rounded-full flex items-center justify-center text-secondary hover:bg-surface-container-low transition-colors"
          >
            <ArrowLeft size={18} />
          </Link>
          <div>
            <h1 className="text-lg font-bold text-text-primary">Dashboard</h1>
            <p className="text-xs text-text-muted">{group?.name || "Grupo"}</p>
          </div>
        </div>
        <div className="flex items-center gap-2">
          <NotificationDropdown />
          <button
            type="button"
            onClick={() => navigate(`/group/${groupId}/profile/${user?.id}`)}
            className="w-9 h-9 rounded-full flex items-center justify-center overflow-hidden hover:ring-2 hover:ring-primary transition-all"
          >
            {user?.avatarUrl ? (
              <img
                src={user.avatarUrl}
                alt={user.name}
                className="w-full h-full object-cover"
              />
            ) : (
              <div className="w-full h-full bg-primary-light flex items-center justify-center text-primary font-bold text-sm">
                {user?.name?.charAt(0).toUpperCase() || "U"}
              </div>
            )}
          </button>
        </div>
      </div>

      {/* Header Desktop */}
      <div className="hidden lg:flex items-center justify-between mb-6">
        <div className="flex items-center gap-3">
          <Link
            to="/groups"
            className="text-sm text-secondary hover:text-on-surface flex items-center gap-1 transition-colors"
          >
            <ArrowLeft size={16} />
            Meus Grupos
          </Link>
          <span className="text-text-muted">/</span>
          <h1 className="text-xl font-bold text-text-primary">{group?.name || "Grupo"}</h1>
        </div>
        <AppButton color="red" size="sm" onClick={() => setShowCreateEvent(true)}>
          <span className="flex items-center gap-1.5">
            <Plus size={16} />
            Novo Evento
          </span>
        </AppButton>
      </div>

      {/* Mobile: Hero Score + Pendentes + Feed */}
      <div className="lg:hidden space-y-5">
        <HeroScoreCard score={profile?.currentScore || 0} delta={0} />
        <PendingVotesSection events={pendingEvents} />
        <FeedTabs
          activeTab={activeTab}
          onTabChange={setActiveTab}
          pendingCount={pendingEvents.length}
        />
        <div className="space-y-3">
          <h2 className="text-base font-semibold text-text-primary">Feed Recente</h2>
          {activeTab === "all" ? (
            allFeedItems.length === 0 ? (
              <div className="text-center py-12">
                <p className="text-text-secondary text-sm mb-4">Nenhum evento recente</p>
                <AppButton color="red" size="sm" onClick={() => setShowCreateEvent(true)}>
                  <Plus size={16} className="mr-1" />
                  Criar primeiro evento
                </AppButton>
              </div>
            ) : (
              allFeedItems.map((entry) =>
                entry.type === "shared_event" ? (
                    <SharedEventCard
                    key={`shared-${entry.item.id}`}
                    event={{
                      id: entry.item.id,
                      title: entry.item.title,
                      points: entry.item.points,
                      participantCount: entry.item.participantCount ?? 0,
                      isClosed: entry.item.isClosed ?? false,
                      createdByUserId: entry.item.createdByUserId,
                      createdByUserAvatarUrl: entry.item.createdByUserAvatarUrl,
                      hasCurrentUserJoined: entry.item.hasCurrentUserJoined ?? false,
                      imageUrl: entry.item.imageUrl,
                    }}
                  />
                ) : (
                  <EventCard key={entry.event.id} event={entry.event} />
                )
              )
            )
          ) : pendingEvents.length === 0 ? (
            <div className="text-center py-12">
              <p className="text-text-secondary text-sm mb-4">Nenhum evento pendente</p>
            </div>
          ) : (
            pendingEvents.map((event) => <VotingCard key={event.id} event={event} />)
          )}
        </div>
        <div ref={sentinelRef} className="py-4 flex justify-center">
          {isFetchingMoreFeed && (
            <span className="text-sm text-text-secondary">Carregando mais...</span>
          )}
        </div>
        <div className="pt-4 space-y-4">
          <TopMembersWidget members={topMembers} />
        </div>
      </div>

      {/* Desktop: Grid 3 colunas */}
      <div className="hidden lg:grid lg:grid-cols-12 gap-6">
        {/* Feed Central */}
        <div className="lg:col-span-7 space-y-4">
          <FeedTabs
            activeTab={activeTab}
            onTabChange={setActiveTab}
            pendingCount={pendingEvents.length}
          />
          {pendingEvents.length > 0 && activeTab === "all" && (
            <div className="space-y-3">
              {pendingEvents.map((event) => (
                <VotingCard key={event.id} event={event} />
              ))}
            </div>
          )}
          <div className="space-y-3">
            {activeTab === "all" ? (
              allFeedItems.length === 0 ? (
                <div className="text-center py-16">
                  <p className="text-text-secondary mb-4">Este grupo está muito silencioso.</p>
                  <AppButton color="red" size="sm" onClick={() => setShowCreateEvent(true)}>
                    <Plus size={16} className="mr-1" />
                    Criar primeiro evento
                  </AppButton>
                </div>
              ) : (
                allFeedItems.map((entry) =>
                  entry.type === "shared_event" ? (
                      <SharedEventCard
                        key={`shared-${entry.item.id}`}
                        event={{
                          id: entry.item.id,
                          title: entry.item.title,
                          points: entry.item.points,
                          participantCount: entry.item.participantCount ?? 0,
                          isClosed: entry.item.isClosed ?? false,
                          createdByUserId: entry.item.createdByUserId,
                          createdByUserAvatarUrl: entry.item.createdByUserAvatarUrl,
                          hasCurrentUserJoined: entry.item.hasCurrentUserJoined ?? false,
                          imageUrl: entry.item.imageUrl,
                        }}
                      />
                    ) : (
                      <EventCard key={entry.event.id} event={entry.event} />
                    )
                )
              )
            ) : (
              pendingEvents.map((event) => <VotingCard key={event.id} event={event} />)
            )}
          </div>
          <div ref={sentinelRef} className="py-4 flex justify-center">
            {isFetchingMoreFeed && (
              <span className="text-sm text-text-secondary">Carregando mais...</span>
            )}
          </div>
        </div>

        {/* Sidebar Direita - Widgets */}
        <div className="lg:col-span-5 space-y-4">
          <div className="grid grid-cols-2 gap-4">
            <div className="bg-primary-container/10 p-4 rounded-xl border border-primary-container/20 flex flex-col justify-center items-center text-center">
              <span className="text-headline-md font-headline-md text-primary font-bold">
                {totalEvents}
              </span>
              <span className="text-caption font-caption text-secondary">Total de Eventos</span>
            </div>
            <div className="bg-surface-container-low p-4 rounded-xl border border-surface-container flex flex-col justify-center items-center text-center">
              <span className="text-headline-md font-headline-md text-on-surface font-bold">
                {activeMembersCount}
              </span>
              <span className="text-caption font-caption text-secondary">Membros Ativos</span>
            </div>
          </div>
          <TopMembersWidget members={topMembers} />
        </div>
      </div>

      {/* Create Event Modal */}
      <CreateEventModal
        isOpen={showCreateEvent}
        onClose={() => setShowCreateEvent(false)}
        groupId={groupId || ""}
      />
    </div>
  );
}
