import { useState } from "react";
import { AppButton } from "../../ui/app-button";
import { AppInput } from "../../ui/app-input";
import { AppSpinner } from "../../ui/app-spinner";
import { X } from "lucide-react";
import { useJoinGroup } from "../../../hooks/use-groups";
import { ApiError } from "../../../lib/api";

interface JoinGroupModalProps {
  isOpen: boolean;
  onClose: () => void;
}

export function JoinGroupModal({ isOpen, onClose }: JoinGroupModalProps) {
  const [code, setCode] = useState("");
  const [error, setError] = useState<string | null>(null);
  const joinGroup = useJoinGroup();

  if (!isOpen) return null;

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (code.length < 8) return;
    setError(null);
    joinGroup.mutate(
      { inviteCode: code },
      {
        onSuccess: () => {
          setCode("");
          onClose();
        },
        onError: (err) => {
          setError(err instanceof ApiError ? err.message : "Erro ao entrar no grupo");
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
            Entrar via Código
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
            <label htmlFor="group-code" className="block text-sm font-medium text-text-secondary mb-1.5">
              Código do Grupo
            </label>
            <AppInput
              id="group-code"
              placeholder="ABC12345"
              value={code}
              onChange={(e) => setCode(e.target.value.toUpperCase())}
              required
              disabled={joinGroup.isPending}
              sizing="md"
              className="w-full text-center text-lg font-mono tracking-wider"
              maxLength={8}
            />
            <p className="text-xs text-text-muted mt-1.5">
              Insira o código de 8 caracteres fornecido pelo administrador
            </p>
          </div>

          {error && (
            <div className="text-sm text-danger bg-red-50 rounded-lg px-4 py-2.5">
              {error}
            </div>
          )}

          <AppButton
            type="submit"
            className="w-full bg-primary hover:bg-primary-hover text-white focus:ring-primary-light focus:ring-2 disabled:opacity-50"
            disabled={joinGroup.isPending || code.length < 8}
          >
            {joinGroup.isPending ? (
              <span className="flex items-center justify-center gap-2">
                <AppSpinner size="sm" />
                Entrando...
              </span>
            ) : (
              "Entrar no Grupo"
            )}
          </AppButton>
        </form>
      </div>
    </div>
  );
}
