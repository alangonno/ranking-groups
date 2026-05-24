import { useState } from "react";
import { AppButton } from "../../ui/app-button";
import { AppInput } from "../../ui/app-input";
import { AppTextarea } from "../../ui/app-textarea";
import { AppSelect } from "../../ui/app-select";
import { AppSpinner } from "../../ui/app-spinner";
import { X } from "lucide-react";
import { useCreateEvent } from "../../../hooks/use-events";
import { useMembers } from "../../../hooks/use-members";
import { EventType } from "../../../types/event/event";
import { ApiError } from "../../../lib/api";

interface CreateEventModalProps {
  isOpen: boolean;
  onClose: () => void;
  groupId: string;
}

export function CreateEventModal({ isOpen, onClose, groupId }: CreateEventModalProps) {
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [points, setPoints] = useState("");
  const [type, setType] = useState<string>("" + EventType.Positive);
  const [affectedUserId, setAffectedUserId] = useState("");
  const [error, setError] = useState<string | null>(null);

  const createEvent = useCreateEvent();
  const { data: members = [] } = useMembers(groupId);

  if (!isOpen) return null;

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault();

    const pointsNum = Number(points);
    if (!title.trim() || !description.trim() || pointsNum <= 0 || !affectedUserId) {
      setError("Preencha todos os campos obrigatórios.");
      return;
    }

    setError(null);

    createEvent.mutate(
      {
        groupId,
        title: title.trim(),
        description: description.trim(),
        points: pointsNum,
        type: Number(type) as EventType,
        affectedUserId,
      },
      {
        onSuccess: () => {
          setTitle("");
          setDescription("");
          setPoints("");
          setType("" + EventType.Positive);
          setAffectedUserId("");
          onClose();
        },
        onError: (err) => {
          setError(err instanceof ApiError ? err.message : "Erro ao criar evento");
        },
      }
    );
  }

  const isValid =
    title.trim() &&
    description.trim() &&
    Number(points) > 0 &&
    affectedUserId;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
      <div className="absolute inset-0 bg-black/40" onClick={onClose} />
      <div className="relative bg-white rounded-2xl shadow-xl w-full max-w-md p-6">
        <div className="flex items-center justify-between mb-6">
          <h2 className="text-lg font-semibold text-text-primary">
            Novo Evento
          </h2>
          <button
            type="button"
            onClick={onClose}
            className="w-8 h-8 rounded-full flex items-center justify-center text-text-secondary hover:bg-gray-100 transition-colors"
          >
            <X size={18} />
          </button>
        </div>

        <form onSubmit={handleSubmit} className="space-y-5">
          <div>
            <label htmlFor="event-title" className="block text-sm font-medium text-text-secondary mb-1.5">
              Título
            </label>
            <AppInput
              id="event-title"
              placeholder="Ex: Organizou o churrasco"
              value={title}
              onChange={(e) => setTitle(e.target.value)}
              required
              disabled={createEvent.isPending}
              sizing="md"
              className="w-full"
            />
          </div>

          <div>
            <label htmlFor="event-description" className="block text-sm font-medium text-text-secondary mb-1.5">
              Descrição
            </label>
            <AppTextarea
              id="event-description"
              placeholder="Descreva o que aconteceu..."
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              required
              disabled={createEvent.isPending}
              rows={3}
              className="w-full"
            />
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div>
              <label htmlFor="event-points" className="block text-sm font-medium text-text-secondary mb-1.5">
                Pontos
              </label>
              <AppInput
                id="event-points"
                type="number"
                min={1}
                placeholder="20"
                value={points}
                onChange={(e) => setPoints(e.target.value)}
                required
                disabled={createEvent.isPending}
                sizing="md"
                className="w-full"
              />
            </div>

            <div>
              <label htmlFor="event-type" className="block text-sm font-medium text-text-secondary mb-1.5">
                Tipo
              </label>
              <AppSelect
                id="event-type"
                value={type}
                onChange={(e) => setType(e.target.value)}
                disabled={createEvent.isPending}
                className="w-full"
              >
                <option value={EventType.Positive}>Positivo</option>
                <option value={EventType.Negative}>Negativo</option>
              </AppSelect>
            </div>
          </div>

          <div>
            <label htmlFor="event-affected-user" className="block text-sm font-medium text-text-secondary mb-1.5">
              Usuário afetado
            </label>
            <AppSelect
              id="event-affected-user"
              value={affectedUserId}
              onChange={(e) => setAffectedUserId(e.target.value)}
              disabled={createEvent.isPending}
              className="w-full"
            >
              <option value="">Selecione um membro</option>
              {members.map((m) => (
                <option key={m.userId} value={m.userId}>
                  {m.name}
                </option>
              ))}
            </AppSelect>
          </div>

          {error && (
            <div className="text-sm text-danger bg-red-50 rounded-lg px-4 py-2.5">
              {error}
            </div>
          )}

          <AppButton
            type="submit"
            className="w-full bg-primary hover:bg-primary-hover text-white focus:ring-primary-light focus:ring-2 disabled:opacity-50"
            disabled={createEvent.isPending || !isValid}
          >
            {createEvent.isPending ? (
              <span className="flex items-center justify-center gap-2">
                <AppSpinner size="sm" />
                Criando...
              </span>
            ) : (
              "Criar Evento"
            )}
          </AppButton>
        </form>
      </div>
    </div>
  );
}
