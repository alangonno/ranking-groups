import { useState, useRef } from "react";
import { AppButton } from "../../ui/app-button";
import { AppInput } from "../../ui/app-input";
import { AppTextarea } from "../../ui/app-textarea";
import { AppSpinner } from "../../ui/app-spinner";
import { X, ImagePlus } from "lucide-react";
import { useCreateSharedEvent } from "../../../hooks/use-shared-events";
import { useImageUpload } from "../../../hooks/use-image-upload";
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
  const [closesAt, setClosesAt] = useState("");
  const [eventImageUrl, setEventImageUrl] = useState<string | undefined>(undefined);
  const [error, setError] = useState<string | null>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const createSharedEvent = useCreateSharedEvent();
  const { uploadImage, isUploading: isUploadingImage } = useImageUpload();

  if (!isOpen) return null;

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault();

    const pointsNum = Number(points);
    if (!title.trim() || pointsNum <= 0) {
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
        closesAt: closesAt ? new Date(closesAt).toISOString() : undefined,
        imageUrl: eventImageUrl,
      },
      {
        onSuccess: () => {
          setTitle("");
          setDescription("");
          setPoints("");
          setClosesAt("");
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
    Number(points) > 0;

  async function handleImageSelect(e: React.ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0];
    if (!file) return;
    try {
      const result = await uploadImage(file, "event-images");
      setEventImageUrl(result.path);
    } catch {
      // error handled by hook
    }
  }

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
              placeholder="Descreva a atividade... (opcional)"
              value={description}
              onChange={(e) => setDescription(e.target.value)}
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
            {eventImageUrl ? (
              <div className="relative w-full h-32 rounded-lg overflow-hidden">
                <img
                  src={`${import.meta.env.VITE_SUPABASE_URL}/storage/v1/object/public/${eventImageUrl}`}
                  alt="Preview"
                  className="w-full h-full object-cover"
                />
                <button
                  type="button"
                  onClick={() => setEventImageUrl(undefined)}
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

          <div>
            <label htmlFor="shared-closes-at" className="block text-sm font-medium text-text-secondary mb-1.5">
              Data de fechamento <span className="text-text-muted">(opcional)</span>
            </label>
            <AppInput
              id="shared-closes-at"
              type="datetime-local"
              value={closesAt}
              onChange={(e) => setClosesAt(e.target.value)}
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
            disabled={createSharedEvent.isPending || isUploadingImage || !isValid}
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
