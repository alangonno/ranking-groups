import { useState } from "react";
import { useParams, Link } from "react-router-dom";
import { Plus, Bell, ArrowLeft } from "lucide-react";
import { AppButton } from "../../components/ui/app-button";
import { EventCard } from "../../components/authenticated/events/event-card";
import { VotingCard } from "../../components/authenticated/events/voting-card";
import { TopMembersWidget } from "../../components/authenticated/ranking/top-members-widget";
import { HeroScoreCard } from "../../components/authenticated/dashboard/hero-score-card";
import { PendingVotesSection } from "../../components/authenticated/dashboard/pending-votes-section";
import { FeedTabs } from "../../components/authenticated/dashboard/feed-tabs";
import { CreateEventModal } from "../../components/authenticated/events/create-event-modal";
import { setLastGroupId } from "../../lib/group-storage";
import { EventStatus } from "../../types/event/event";
import { useGroup } from "../../hooks/use-groups";
import { useGroupEvents } from "../../hooks/use-events";
import { useRanking } from "../../hooks/use-ranking";
import { useUserProfile } from "../../hooks/use-user-profile";
import { useCurrentUser } from "../../hooks/use-auth";

export function DashboardPage() {
  const { groupId } = useParams<{ groupId: string }>();
  const [activeTab, setActiveTab] = useState<"all" | "pending">("all");
  const [showCreateEvent, setShowCreateEvent] = useState(false);
  const { data: user } = useCurrentUser();

  const { data: group } = useGroup(groupId || "");
  const { data: groupEvents = [] } = useGroupEvents(groupId || "");
  const { data: ranking = [] } = useRanking(groupId || "");
  const { data: profile } = useUserProfile(
    groupId || "",
    user?.id || ""
  );

  // Save last visited group
  if (groupId) {
    setLastGroupId(groupId);
  }

  if (!groupId) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <p className="text-text-secondary">Grupo não encontrado</p>
      </div>
    );
  }

  const pendingEvents = groupEvents.filter((e) => e.status === EventStatus.Pending);
  const approvedEvents = groupEvents.filter((e) => e.status === EventStatus.Approved);

  const displayEvents = activeTab === "pending" ? pendingEvents : groupEvents;

  const topMembers = ranking.slice(0, 5).map((entry, index) => ({
    position: index + 1,
    name: entry.user?.name || "",
    points: entry.score,
    avatar: entry.user?.name
      ? entry.user.name
          .split(" ")
          .map((n) => n[0])
          .join("")
          .toUpperCase()
          .slice(0, 2)
      : "??",
  }));

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
            <h1 className="text-lg font-bold text-text-primary">{group?.name || "Grupo"}</h1>
            <p className="text-xs text-text-muted">Dashboard</p>
          </div>
        </div>
        <button
          type="button"
          aria-label="Notificações"
          className="w-9 h-9 rounded-full flex items-center justify-center text-secondary hover:bg-surface-container-low transition-colors"
        >
          <Bell size={18} />
        </button>
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
        {/* Hero Score Card */}
        <HeroScoreCard score={profile?.currentScore || 0} delta={0} />

        {/* Pending Votes */}
        <PendingVotesSection events={pendingEvents} />

        {/* Tabs */}
        <FeedTabs
          activeTab={activeTab}
          onTabChange={setActiveTab}
          pendingCount={pendingEvents.length}
        />

        {/* Feed */}
        <div className="space-y-3">
          <h2 className="text-base font-semibold text-text-primary">
            Feed Recente
          </h2>

          {displayEvents.length === 0 ? (
            <div className="text-center py-12">
              <p className="text-text-secondary text-sm mb-4">
                Nenhum evento recente
              </p>
              <AppButton color="red" size="sm" onClick={() => setShowCreateEvent(true)}>
                <Plus size={16} className="mr-1" />
                Criar primeiro evento
              </AppButton>
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

        {/* Widgets embaixo no mobile */}
        <div className="pt-4 space-y-4">
          <TopMembersWidget members={topMembers} />
        </div>
      </div>

      {/* Desktop: Grid 3 colunas */}
      <div className="hidden lg:grid lg:grid-cols-12 gap-6">
        {/* Feed Central */}
        <div className="lg:col-span-7 space-y-4">
          {/* Tabs */}
          <FeedTabs
            activeTab={activeTab}
            onTabChange={setActiveTab}
            pendingCount={pendingEvents.length}
          />

          {/* Pending Highlight */}
          {pendingEvents.length > 0 && activeTab === "all" && (
            <div className="space-y-3">
              {pendingEvents.map((event) => (
                <VotingCard key={event.id} event={event} />
              ))}
            </div>
          )}

          {/* Feed List */}
          <div className="space-y-3">
            {activeTab === "all"
              ? approvedEvents.map((event) => (
                  <EventCard key={event.id} event={event} />
                ))
              : pendingEvents.map((event) => (
                  <VotingCard key={event.id} event={event} />
                ))}
          </div>

          {displayEvents.length === 0 && (
            <div className="text-center py-16">
              <p className="text-text-secondary mb-4">
                Este grupo está muito silencioso.
              </p>
              <AppButton color="red" size="sm" onClick={() => setShowCreateEvent(true)}>
                <Plus size={16} className="mr-1" />
                Criar primeiro evento
              </AppButton>
            </div>
          )}
        </div>

        {/* Sidebar Direita - Widgets */}
        <div className="lg:col-span-5 space-y-4">
          {/* Quick Stats Bento */}
          <div className="grid grid-cols-2 gap-4">
            <div className="bg-primary-container/10 p-4 rounded-xl border border-primary-container/20 flex flex-col justify-center items-center text-center">
              <span className="text-headline-md font-headline-md text-primary font-bold">
                {approvedEvents.length + pendingEvents.length}
              </span>
              <span className="text-caption font-caption text-secondary">Total de Eventos</span>
            </div>
            <div className="bg-surface-container-low p-4 rounded-xl border border-surface-container flex flex-col justify-center items-center text-center">
              <span className="text-headline-md font-headline-md text-on-surface font-bold">
                {ranking.length}
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
