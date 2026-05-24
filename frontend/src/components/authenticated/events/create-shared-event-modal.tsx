import { useState } from "react";
import { AppButton } from "../../ui/app-button";
import { AppInput } from "../../ui/app-input";
import { AppTextarea } from "../../ui/app-textarea";
import { AppSpinner } from "../../ui/app-spinner";
import { X } from "lucide-react";
import { useCreateSharedEvent } from "../../../hooks/use-shared-events";
import { ApiError } from "../../../lib/api";

interface CreateSharedEventModalProps {
  isOpen: boolean;
  onClose: () => void;
  groupId: string;
}

export function CreateSharedEventModal({ isOpen, onClose, groupId }: CreateSharedEventModalProps) {
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [points, setPoints] = useState("");
  const [error, setError] = useState<string | null>(null);

  const createSharedEvent = useCreateSharedEvent();

  if (!isOpen) return null;

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault();

    const pointsNum = Number(points);
    if (!title.trim() || !description.trim() || pointsNum <= 0) {
      setError("Preencha todos os campos obrigatórios.");
      return;
    }

    setError(null);

    createSharedEvent.mutate(
      {
        groupId,
        title: title.trim(),
        description: description.trim(),
        points: pointsNum,
      },
      {
        onSuccess: () => {
          setTitle("");
          setDescription("");
          setPoints("");
          onClose();
        },
        onError: (err) => {
          setError(err instanceof ApiError ? err.message : "Erro ao criar evento compartilhado");
        },
      }
    );
  }

  const isValid =
    title.trim() &&
    description.trim() &&
    Number(points) > 0;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
      <div className="absolute inset-0 bg-black/40" onClick={onClose} />
      <div className="relative bg-white rounded-2xl shadow-xl w-full max-w-md p-6">
        <div className="flex items-center justify-between mb-6">
          <h2 className="text-lg font-semibold text-text-primary">
            Novo Evento Compartilhado
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
            <label htmlFor="shared-title" className="block text-sm font-medium text-text-secondary mb-1.5">
              Título
            </label>
            <AppInput
              id="shared-title"
              placeholder="Ex: Churrasco do Grupo"
              value={title}
              onChange={(e) => setTitle(e.target.value)}
              required
              disabled={createSharedEvent.isPending}
              sizing="md"
              className="w-full"
            />
          </div>

          <div>
            <label htmlFor="shared-description" className="block text-sm font-medium text-text-secondary mb-1.5">
              Descrição
            </label>
            <AppTextarea
              id="shared-description"
              placeholder="Descreva a atividade..."
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              required
              disabled={createSharedEvent.isPending}
              rows={3}
              className="w-full"
            />
          </div>

          <div>
            <label htmlFor="shared-points" className="block text-sm font-medium text-text-secondary mb-1.5">
              Pontos por participante
            </label>
            <AppInput
              id="shared-points"
              type="number"
              min={1}
              placeholder="15"
              value={points}
              onChange={(e) => setPoints(e.target.value)}
              required
              disabled={createSharedEvent.isPending}
              sizing="md"
              className="w-full"
            />
          </div>

          {error && (
            <div className="text-sm text-danger bg-red-50 rounded-lg px-4 py-2.5">
              {error}
            </div>
          )}

          <AppButton
            type="submit"
            className="w-full bg-primary hover:bg-primary-hover text-white focus:ring-primary-light focus:ring-2 disabled:opacity-50"
            disabled={createSharedEvent.isPending || !isValid}
          >
            {createSharedEvent.isPending ? (
              <span className="flex items-center justify-center gap-2">
                <AppSpinner size="sm" />
                Criando...
              </span>
            ) : (
              "Criar Evento Compartilhado"
            )}
          </AppButton>
        </form>
      </div>
    </div>
  );
}
