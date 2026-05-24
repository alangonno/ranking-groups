import { useParams } from "react-router-dom";
import { ArrowUp, Calendar } from "lucide-react";
import { useUserProfile } from "../../hooks/use-user-profile";
import { useAuthContext } from "../../providers/auth-provider";
import { GroupRole } from "../../types/group/group";
import { AppBadge } from "../../components/ui/app-badge";

function roleLabel(role: GroupRole): string {
  switch (role) {
    case GroupRole.Owner:
      return "Owner";
    case GroupRole.Admin:
      return "Admin";
    case GroupRole.Member:
      return "Member";
    default:
      return "Member";
  }
}

function formatDate(iso: string): string {
  const d = new Date(iso);
  return d.toLocaleDateString("pt-BR", {
    day: "2-digit",
    month: "short",
    year: "numeric",
  });
}

export function ProfilePage() {
  const { groupId, userId } = useParams<{ groupId: string; userId: string }>();
  const { data: profile, isLoading } = useUserProfile(groupId || "", userId || "");
  const { user: currentUser } = useAuthContext();

  const member = profile?.member;
  const events = profile?.events || [];
  const sharedEvents = profile?.sharedEvents || [];
  const isOwnProfile = currentUser?.id === userId;

  if (isLoading) {
    return (
      <div className="p-4 lg:p-8 max-w-5xl mx-auto">
        <div className="animate-pulse space-y-6">
          <div className="flex gap-4">
            <div className="w-20 h-20 rounded-full bg-gray-200" />
            <div className="flex-1 space-y-2">
              <div className="h-6 bg-gray-200 rounded w-1/3" />
              <div className="h-4 bg-gray-200 rounded w-1/4" />
            </div>
          </div>
          <div className="h-32 bg-gray-200 rounded-xl" />
          <div className="space-y-3">
            {Array.from({ length: 4 }).map((_, i) => (
              <div key={i} className="h-16 bg-gray-200 rounded-lg" />
            ))}
          </div>
        </div>
      </div>
    );
  }

  if (!member) {
    return (
      <div className="p-4 lg:p-8 max-w-5xl mx-auto">
        <div className="text-center py-16">
          <p className="text-text-secondary">Usuário não encontrado neste grupo</p>
        </div>
      </div>
    );
  }

  return (
    <div className="p-4 lg:p-8 max-w-5xl mx-auto">
      {/* Hero Section */}
      <div className="flex flex-col lg:flex-row gap-4 mb-8">
        {/* User Card */}
        <div className="flex-1 bg-white rounded-xl border border-border shadow-[0_1px_3px_rgba(0,0,0,0.05)] p-5">
          <div className="flex items-center gap-4">
            <div className="relative">
              <div className="w-20 h-20 rounded-full bg-primary-light flex items-center justify-center text-primary font-bold text-2xl">
                {member.avatar}
              </div>
              <div className="absolute -bottom-1 -right-1 bg-primary text-white text-[10px] font-bold px-1.5 py-0.5 rounded-md">
                LVL {Math.max(1, Math.floor(member.currentScore / 100))}
              </div>
            </div>
            <div className="flex-1 min-w-0">
              <h1 className="text-xl font-bold text-text-primary truncate">
                {member.name}
              </h1>
              <div className="flex items-center gap-2 mt-1">
                <AppBadge color="gray" size="sm">
                  {roleLabel(member.role)}
                </AppBadge>
                {isOwnProfile && (
                  <span className="text-xs text-text-muted">(Você)</span>
                )}
              </div>
              <div className="flex items-center gap-1 mt-2 text-text-muted text-xs">
                <Calendar size={14} />
                <span>Membro do grupo</span>
              </div>
            </div>
          </div>
        </div>

        {/* Score Card */}
        <div className="flex-1 bg-primary rounded-xl p-5 text-white shadow-[0_2px_8px_rgba(0,0,0,0.15)]">
          <p className="text-xs font-medium uppercase tracking-wider opacity-80">
            Total Score Balance
          </p>
          <p className="text-4xl font-bold mt-1">
            {member.currentScore.toLocaleString()}
          </p>
          <div className="flex items-center gap-1 mt-2 text-sm opacity-90">
            <ArrowUp size={16} />
            <span>+450 this week</span>
          </div>
        </div>
      </div>

      {/* Content Grid: Timeline + Shared Events */}
      <div className="flex flex-col lg:flex-row gap-6">
        {/* Timeline */}
        <div className="flex-1 lg:min-w-0">
          <h2 className="text-lg font-bold text-text-primary mb-4">
            Score Timeline
          </h2>
          <div className="bg-white rounded-xl border border-border shadow-[0_1px_3px_rgba(0,0,0,0.05)] p-5">
            {events.length === 0 ? (
              <p className="text-text-secondary text-sm text-center py-8">
                Nenhum evento aprovado ainda
              </p>
            ) : (
              <div className="relative">
                {/* Vertical line */}
                <div className="absolute left-[7px] top-2 bottom-2 w-0.5 bg-border" />
                <div className="space-y-6">
                  {events.map((event) => {
                    const isPositive = event.type === "Positive";
                    const signedPoints = isPositive ? event.points : -event.points;
                    return (
                      <div key={event.id} className="relative flex gap-4">
                        {/* Dot */}
                        <div
                          className={`w-4 h-4 rounded-full shrink-0 mt-1 ${
                            isPositive ? "bg-primary" : "bg-text-muted"
                          }`}
                        />
                        {/* Content */}
                        <div className="flex-1 min-w-0">
                          <div className="flex items-start justify-between gap-2">
                            <div>
                              <p className="text-sm font-medium text-text-primary">
                                {event.title}
                              </p>
                              <p className="text-xs text-text-muted mt-0.5">
                                {formatDate(event.createdAt)}
                              </p>
                            </div>
                            <span
                              className={`text-sm font-bold shrink-0 ${
                                isPositive ? "text-primary" : "text-text-muted"
                              }`}
                            >
                              {isPositive ? "+" : ""}
                              {signedPoints}
                            </span>
                          </div>
                          <div className="mt-2 bg-gray-50 rounded-lg px-3 py-2 border border-border">
                            <p className="text-xs text-text-muted">
                              Balance:{" "}
                              <span className="font-medium text-text-primary">
                                {event.scoreBalance.toLocaleString()}
                              </span>
                            </p>
                          </div>
                        </div>
                      </div>
                    );
                  })}
                </div>
              </div>
            )}
          </div>
        </div>

        {/* Shared Events */}
        <div className="w-full lg:w-[40%] lg:shrink-0">
          <h2 className="text-lg font-bold text-text-primary mb-4">
            Shared Events
          </h2>
          <div className="space-y-4">
            {sharedEvents.length === 0 ? (
              <div className="bg-white rounded-xl border border-border shadow-[0_1px_3px_rgba(0,0,0,0.05)] p-5 text-center">
                <p className="text-text-secondary text-sm">
                  Nenhum shared event
                </p>
              </div>
            ) : (
              sharedEvents.map((se) => (
                <div
                  key={se.id}
                  className="relative bg-gray-100 rounded-xl overflow-hidden aspect-video flex flex-col justify-end"
                >
                  {/* Gradient overlay */}
                  <div className="absolute inset-0 bg-gradient-to-t from-black/70 via-black/20 to-transparent" />
                  {/* Content */}
                  <div className="relative p-4 text-white">
                    <div className="flex items-center gap-2 mb-2">
                      <AppBadge
                        color={se.userRole === "organizer" ? "red" : "gray"}
                        size="sm"
                      >
                        {se.userRole === "organizer" ? "ORGANIZER" : "PARTICIPANT"}
                      </AppBadge>
                    </div>
                    <h3 className="text-sm font-bold">{se.title}</h3>
                    <div className="flex items-center justify-between mt-1">
                      <span className="text-xs opacity-80">
                        {se.participantCount} participants
                      </span>
                      <span className="text-xs font-bold">
                        +{se.points} pts
                      </span>
                    </div>
                  </div>
                </div>
              ))
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
