import { useState } from "react";
import { AppButton } from "../../ui/app-button";
import { AppInput } from "../../ui/app-input";
import { AppSpinner } from "../../ui/app-spinner";
import { X } from "lucide-react";

interface JoinGroupModalProps {
  isOpen: boolean;
  onClose: () => void;
}

export function JoinGroupModal({ isOpen, onClose }: JoinGroupModalProps) {
  const [code, setCode] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);

  if (!isOpen) return null;

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!code.trim()) return;
    setIsSubmitting(true);
    // TODO: Integrar com useJoinGroup() quando backend estiver pronto
    setTimeout(() => {
      setIsSubmitting(false);
      setCode("");
      onClose();
    }, 1000);
  }

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
      <div className="absolute inset-0 bg-black/40" onClick={onClose} />
      <div className="relative bg-white rounded-2xl shadow-xl w-full max-w-md p-6">
        <div className="flex items-center justify-between mb-6">
          <h2 className="text-lg font-semibold text-text-primary">
            Entrar via Código
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
              Código do Grupo
            </label>
            <AppInput
              placeholder="ABC12345"
              value={code}
              onChange={(e) => setCode(e.target.value.toUpperCase())}
              required
              disabled={isSubmitting}
              className="w-full text-center text-lg font-mono tracking-wider"
              maxLength={8}
            />
            <p className="text-xs text-text-muted mt-1.5">
              Insira o código de 8 caracteres fornecido pelo administrador
            </p>
          </div>

          <AppButton
            type="submit"
            color="red"
            className="w-full"
            disabled={isSubmitting || code.length < 8}
          >
            {isSubmitting ? (
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
