import { useState } from "react";
import { AppButton } from "../../ui/app-button";
import { AppInput } from "../../ui/app-input";
import { AppSpinner } from "../../ui/app-spinner";

interface LoginFormProps {
  onSubmit: (data: { email: string; password: string }) => void;
  isPending: boolean;
  error?: string | null;
}

export function LoginForm({ onSubmit, isPending, error }: LoginFormProps) {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    onSubmit({ email, password });
  }

  return (
    <form onSubmit={handleSubmit} className="space-y-6">
      <div>
        <label htmlFor="email" className="block text-sm font-medium text-text-secondary mb-1.5">
          Email
        </label>
        <AppInput
          id="email"
          type="email"
          placeholder="seu@email.com"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          required
          disabled={isPending}
          sizing="lg"
          className="w-full"
        />
      </div>

      <div>
        <label htmlFor="password" className="block text-sm font-medium text-text-secondary mb-1.5">
          Senha
        </label>
        <AppInput
          id="password"
          type="password"
          placeholder="••••••••"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          required
          disabled={isPending}
          sizing="lg"
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
        disabled={isPending}
      >
        {isPending ? (
          <span className="flex items-center justify-center gap-2">
            <AppSpinner size="sm" />
            Entrando...
          </span>
        ) : (
          "Entrar"
        )}
      </AppButton>
    </form>
  );
}
