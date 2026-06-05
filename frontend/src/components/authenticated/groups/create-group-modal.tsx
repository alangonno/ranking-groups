import { useState } from "react";
import { AppButton } from "../../ui/app-button";
import { AppInput } from "../../ui/app-input";
import { AppSpinner } from "../../ui/app-spinner";
import { X } from "lucide-react";
import { useCreateGroup } from "../../../hooks/use-groups";
import { ApiError } from "../../../lib/api";

interface CreateGroupModalProps {
  isOpen: boolean;
  onClose: () => void;
}

export function CreateGroupModal({ isOpen, onClose }: CreateGroupModalProps) {
  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const [error, setError] = useState<string | null>(null);
  const createGroup = useCreateGroup();

  if (!isOpen) return null;

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!name.trim()) return;
    setError(null);
    createGroup.mutate(
      { name: name.trim(), description: description.trim() || undefined },
      {
        onSuccess: () => {
          setName("");
          setDescription("");
          onClose();
        },
        onError: (err) => {
          setError(err instanceof ApiError ? err.message : "Erro ao criar grupo");
        },
      }
    );
  }

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
      <div className="absolute inset-0 bg-black/40" onClick={onClose} />
      <div className="relative bg-surface-container-lowest dark:bg-surface rounded-2xl shadow-xl w-full max-w-md p-6">
        <div className="flex items-center justify-between mb-6">
          <h2 className="text-lg font-semibold text-text-primary">
            Criar Novo Grupo
          </h2>
          <button
            type="button"
            onClick={onClose}
            className="w-8 h-8 rounded-full flex items-center justify-center text-text-secondary hover:bg-surface-container-low transition-colors"
          >
            <X size={18} />
          </button>
        </div>

        <form onSubmit={handleSubmit} className="space-y-6">
          <div>
            <label htmlFor="group-name" className="block text-sm font-medium text-text-secondary mb-1.5">
              Nome do Grupo
            </label>
            <AppInput
              id="group-name"
              placeholder="Ex: Weekend Warriors"
              value={name}
              onChange={(e) => setName(e.target.value)}
              required
              disabled={createGroup.isPending}
              sizing="md"
              className="w-full"
            />
          </div>

          <div>
            <label htmlFor="group-description" className="block text-sm font-medium text-text-secondary mb-1.5">
              Descrição
            </label>
            <AppInput
              id="group-description"
              placeholder="Sobre o que é este grupo?"
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              disabled={createGroup.isPending}
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
            disabled={createGroup.isPending || !name.trim()}
          >
            {createGroup.isPending ? (
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
