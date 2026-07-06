import { useEffect } from "react";
import { ArrowUp, CheckCircle2, Clock, Users, X } from "lucide-react";
import { useNavigate } from "react-router-dom";
import { useSharedEvent } from "../../../hooks/use-shared-events";
import { AppSpinner } from "../../ui/app-spinner";

interface SharedEventViewModalProps {
  isOpen: boolean;
  onClose: () => void;
  sharedEventId: string | null;
  groupId: string;
}

function formatDateTime(dateString?: string) {
  if (!dateString) return null;

  return new Date(dateString).toLocaleString("pt-BR", {
    day: "2-digit",
    month: "short",
    year: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  });
}

export function SharedEventViewModal({
  isOpen,
  onClose,
  sharedEventId,
  groupId,
}: SharedEventViewModalProps) {
  const navigate = useNavigate();
  const { data: sharedEvent, isLoading } = useSharedEvent(
    isOpen && sharedEventId ? sharedEventId : ""
  );

  const createdAt = formatDateTime(sharedEvent?.createdAt);
  const closesAt = formatDateTime(sharedEvent?.closesAt);
  const participants = sharedEvent?.participants ?? [];

  useEffect(() => {
    if (!isOpen) return;

    const previousOverflow = document.body.style.overflow;
    document.body.style.overflow = "hidden";

    return () => {
      document.body.style.overflow = previousOverflow;
    };
  }, [isOpen]);

  function goToProfile(userId: string) {
    onClose();
    navigate(`/group/${groupId}/profile/${userId}`);
  }

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 overflow-y-auto">
      <div className="flex min-h-full items-start justify-center p-4 sm:items-center">
        <div className="fixed inset-0 bg-black/40" onClick={onClose} />
        <div className="relative my-4 w-full max-w-2xl rounded-2xl bg-surface-container-lowest text-text-primary shadow-xl">
          <div className="flex items-center justify-between border-b border-surface-container px-5 py-4">
          <div>
            <h2 className="text-lg font-semibold">Visualizar evento em grupo</h2>
            <p className="text-sm text-text-secondary">Detalhes e participantes</p>
          </div>
          <button
            type="button"
            onClick={onClose}
            className="flex h-9 w-9 items-center justify-center rounded-full text-text-secondary transition-colors hover:bg-surface-container-low"
          >
            <X size={18} />
          </button>
        </div>

        {isLoading ? (
          <div className="flex items-center justify-center px-5 py-14">
            <AppSpinner size="md" />
          </div>
        ) : !sharedEvent ? (
          <div className="px-5 py-10 text-center text-sm text-text-secondary">
            Não foi possível carregar este evento.
          </div>
        ) : (
          <div className="max-h-[78vh] overflow-y-auto px-5 py-5 sm:max-h-[70vh]">
            {sharedEvent.imageUrl ? (
              <div className="mb-5 overflow-hidden rounded-2xl border border-surface-container">
                <img
                  src={sharedEvent.imageUrl}
                  alt={sharedEvent.title}
                  className="h-44 w-full object-cover md:h-48"
                />
              </div>
            ) : null}

            <div className="flex flex-wrap items-center gap-2">
              <span className="inline-flex items-center gap-1 rounded-full bg-primary-container/10 px-3 py-1 text-sm font-semibold text-primary">
                <ArrowUp size={14} />
                +{sharedEvent.points} pts
              </span>
              <span
                className={`inline-flex items-center gap-1 rounded-full px-3 py-1 text-sm font-medium ${
                  sharedEvent.isClosed
                    ? "bg-surface-container text-text-secondary"
                    : "bg-green-500/10 text-green-700"
                }`}
              >
                <CheckCircle2 size={14} />
                {sharedEvent.isClosed ? "Encerrado" : "Ativo"}
              </span>
            </div>

            <div className="mt-4">
              <h3 className="text-xl font-bold text-text-primary md:text-2xl">{sharedEvent.title}</h3>
              {sharedEvent.description ? (
                <p className="mt-2 text-sm leading-6 text-text-secondary">
                  {sharedEvent.description}
                </p>
              ) : (
                <p className="mt-2 text-sm text-text-secondary">Sem descrição informada.</p>
              )}
            </div>

            <div className="mt-5 grid gap-3 sm:grid-cols-2">
              <div className="rounded-2xl border border-surface-container bg-surface-container-low p-4">
                <p className="text-xs font-medium uppercase tracking-wide text-text-secondary">
                  Organizador
                </p>
                <button
                  type="button"
                  onClick={() => goToProfile(sharedEvent.createdByUserId)}
                  className="mt-2 text-left text-sm font-semibold text-text-primary transition-colors hover:text-primary"
                >
                  {sharedEvent.createdByUserName || "Usuário"}
                </button>
              </div>

              <div className="rounded-2xl border border-surface-container bg-surface-container-low p-4">
                <p className="text-xs font-medium uppercase tracking-wide text-text-secondary">
                  Participantes
                </p>
                <div className="mt-2 flex items-center gap-2 text-sm font-semibold text-text-primary">
                  <Users size={16} className="text-text-secondary" />
                  {sharedEvent.participantCount}
                </div>
              </div>

              <div className="rounded-2xl border border-surface-container bg-surface-container-low p-4">
                <p className="text-xs font-medium uppercase tracking-wide text-text-secondary">
                  Criado em
                </p>
                <p className="mt-2 text-sm font-semibold text-text-primary">
                  {createdAt || "Data indisponível"}
                </p>
              </div>

              <div className="rounded-2xl border border-surface-container bg-surface-container-low p-4">
                <p className="text-xs font-medium uppercase tracking-wide text-text-secondary">
                  Fechamento
                </p>
                <div className="mt-2 flex items-center gap-2 text-sm font-semibold text-text-primary">
                  <Clock size={16} className="text-text-secondary" />
                  {closesAt || "Sem data de fechamento"}
                </div>
              </div>
            </div>

            <div className="mt-6">
              <div className="mb-3 flex items-center gap-2">
                <Users size={18} className="text-text-secondary" />
                <h4 className="text-base font-semibold text-text-primary">
                  Quem participou
                </h4>
              </div>

              {participants.length === 0 ? (
                <div className="rounded-2xl border border-surface-container bg-surface-container-low p-4 text-sm text-text-secondary">
                  Ainda não há participantes confirmados neste evento.
                </div>
              ) : (
                <div className="space-y-2">
                  {participants.map((participant) => {
                    const initial = (participant.userName || participant.user?.name || "U")
                      .charAt(0)
                      .toUpperCase();

                    return (
                      <button
                        key={participant.id}
                        type="button"
                        onClick={() => goToProfile(participant.userId)}
                        className="flex w-full items-center justify-between rounded-2xl border border-surface-container bg-surface-container-low px-4 py-3 text-left transition-colors hover:border-primary/40 hover:bg-primary-container/5"
                      >
                        <div className="flex min-w-0 items-center gap-3">
                          <div className="flex h-10 w-10 items-center justify-center rounded-full bg-primary-container/20 text-sm font-bold text-primary">
                            {initial}
                          </div>
                          <div className="min-w-0">
                            <p className="truncate text-sm font-semibold text-text-primary">
                              {participant.userName || participant.user?.name || "Usuário"}
                            </p>
                            <p className="text-xs text-text-secondary">
                              Entrou em {formatDateTime(participant.joinedAt) || "data indisponível"}
                            </p>
                          </div>
                        </div>
                        <span className="text-xs font-medium text-primary">Ver perfil</span>
                      </button>
                    );
                  })}
                </div>
              )}
            </div>
          </div>
        )}
        </div>
      </div>
    </div>
  );
}
