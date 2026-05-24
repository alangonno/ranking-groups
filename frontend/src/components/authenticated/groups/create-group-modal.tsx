import { useState } from "react";
import { AppButton } from "../../ui/app-button";
import { AppInput } from "../../ui/app-input";
import { AppSpinner } from "../../ui/app-spinner";
import { X } from "lucide-react";

interface CreateGroupModalProps {
  isOpen: boolean;
  onClose: () => void;
}

export function CreateGroupModal({ isOpen, onClose }: CreateGroupModalProps) {
  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);

  if (!isOpen) return null;

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!name.trim()) return;
    setIsSubmitting(true);
    // TODO: Integrar com useCreateGroup() quando backend estiver pronto
    setTimeout(() => {
      setIsSubmitting(false);
      setName("");
      setDescription("");
      onClose();
    }, 1000);
  }

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
      <div className="absolute inset-0 bg-black/40" onClick={onClose} />
      <div className="relative bg-white rounded-2xl shadow-xl w-full max-w-md p-6">
        <div className="flex items-center justify-between mb-6">
          <h2 className="text-lg font-semibold text-text-primary">
            Criar Novo Grupo
          </h2>
          <button
            type="button"
            onClick={onClose}
            className="w-8 h-8 rounded-full flex items-center justify-center text-text-secondary hover:bg-gray-100 transition-colors"
          >
            <X size={18} />
          </button>
        </div>

        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-text-secondary mb-1.5">
              Nome do Grupo
            </label>
            <AppInput
              placeholder="Ex: Weekend Warriors"
              value={name}
              onChange={(e) => setName(e.target.value)}
              required
              disabled={isSubmitting}
              className="w-full"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-text-secondary mb-1.5">
              Descrição
            </label>
            <AppInput
              placeholder="Sobre o que é este grupo?"
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              disabled={isSubmitting}
              className="w-full"
            />
          </div>

          <AppButton
            type="submit"
            color="red"
            className="w-full"
            disabled={isSubmitting || !name.trim()}
          >
            {isSubmitting ? (
              <span className="flex items-center justify-center gap-2">
                <AppSpinner size="sm" />
                Criando...
              </span>
            ) : (
              "Criar Grupo"
            )}
          </AppButton>
        </form>
      </div>
    </div>
  );
}
