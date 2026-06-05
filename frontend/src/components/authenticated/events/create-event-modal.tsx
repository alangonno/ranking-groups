import { useState, useRef } from "react";
import { AppButton } from "../../ui/app-button";
import { AppInput } from "../../ui/app-input";
import { AppTextarea } from "../../ui/app-textarea";
import { AppSelect } from "../../ui/app-select";
import { AppSpinner } from "../../ui/app-spinner";
import { X, ImagePlus } from "lucide-react";
import { useCreateEvent } from "../../../hooks/use-events";
import { useMembers } from "../../../hooks/use-members";
import { useImageUpload } from "../../../hooks/use-image-upload";
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
  const [eventImageUrl, setEventImageUrl] = useState<string | undefined>(undefined);
  const [eventImagePublicUrl, setEventImagePublicUrl] = useState<string | undefined>(undefined);
  const [error, setError] = useState<string | null>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const createEvent = useCreateEvent();
  const { uploadImage, isUploading: isUploadingImage } = useImageUpload();
  const { data: membersData } = useMembers(groupId);
  const members = membersData?.flattened ?? [];

  if (!isOpen) return null;

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault();

    const pointsNum = Number(points);
    if (!title.trim() || pointsNum <= 0 || !affectedUserId) {
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
        imageUrl: eventImageUrl,
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
    Number(points) > 0 &&
    affectedUserId;

  async function handleImageSelect(e: React.ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0];
    if (!file) return;
    try {
      const result = await uploadImage(file, "event-images");
      setEventImageUrl(result.path);
      setEventImagePublicUrl(result.publicUrl);
    } catch {
      // error handled by hook
    }
  }

  return (
    <div className="fixed inset-0 z-50 overflow-y-auto">
      <div className="flex min-h-full items-start sm:items-center justify-center p-4">
        <div className="fixed inset-0 bg-black/40" onClick={onClose} />
        <div className="relative bg-white rounded-2xl shadow-xl w-full max-w-md p-6 my-4">
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
          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-5">
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
                  placeholder="Descreva o que aconteceu... (opcional)"
                  value={description}
                  onChange={(e) => setDescription(e.target.value)}
                  disabled={createEvent.isPending}
                  rows={3}
                  className="w-full"
                />
              </div>
            </div>

            <div className="space-y-5">
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
            </div>
          </div>

          <div>
            <label className="block text-sm font-medium text-text-secondary mb-1.5">
              Imagem do evento <span className="text-text-muted">(opcional)</span>
            </label>
            <input
              ref={fileInputRef}
              type="file"
              accept="image/jpeg,image/png,image/webp"
              onChange={handleImageSelect}
              className="hidden"
            />
            {eventImagePublicUrl ? (
              <div className="relative w-full h-32 rounded-lg overflow-hidden">
                <img
                  src={eventImagePublicUrl}
                  alt="Preview"
                  className="w-full h-full object-cover"
                />
                <button
                  type="button"
                  onClick={() => {
                    setEventImageUrl(undefined);
                    setEventImagePublicUrl(undefined);
                  }}
                  className="absolute top-2 right-2 bg-black/50 text-white rounded-full p-1 hover:bg-black/70 transition-colors"
                >
                  <X size={14} />
                </button>
              </div>
            ) : (
              <button
                type="button"
                onClick={() => fileInputRef.current?.click()}
                disabled={isUploadingImage}
                className="w-full h-32 rounded-lg border-2 border-dashed border-surface-container flex flex-col items-center justify-center text-secondary hover:border-primary hover:text-primary transition-colors disabled:opacity-50"
              >
                {isUploadingImage ? (
                  <AppSpinner size="sm" />
                ) : (
                  <>
                    <ImagePlus size={24} />
                    <span className="text-sm mt-1">Adicionar imagem</span>
                  </>
                )}
              </button>
            )}
          </div>

          {error && (
            <div className="text-sm text-danger bg-red-50 rounded-lg px-4 py-2.5">
              {error}
            </div>
          )}

          <AppButton
            type="submit"
            className="w-full bg-primary hover:bg-primary-hover text-white focus:ring-primary-light focus:ring-2 disabled:opacity-50"
            disabled={createEvent.isPending || isUploadingImage || !isValid}
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
  </div>
  );
}
