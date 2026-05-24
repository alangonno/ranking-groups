import { useState } from "react";
import { AppButton } from "../../ui/app-button";
import { AppInput } from "../../ui/app-input";
import { AppSpinner } from "../../ui/app-spinner";

interface RegisterFormProps {
  onSubmit: (data: { name: string; username: string; email: string; password: string }) => void;
  isPending: boolean;
  error?: string | null;
}

export function RegisterForm({ onSubmit, isPending, error }: RegisterFormProps) {
  const [name, setName] = useState("");
  const [username, setUsername] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    onSubmit({ name, username, email, password });
  }

  return (
    <form onSubmit={handleSubmit} className="space-y-6">
      <div>
        <label htmlFor="name" className="block text-sm font-medium text-text-secondary mb-1.5">
          Nome
        </label>
        <AppInput
          id="name"
          type="text"
          placeholder="Seu nome"
          value={name}
          onChange={(e) => setName(e.target.value)}
          required
          disabled={isPending}
          sizing="lg"
          className="w-full"
        />
      </div>

      <div>
        <label htmlFor="username" className="block text-sm font-medium text-text-secondary mb-1.5">
          Nome de usuário
        </label>
        <AppInput
          id="username"
          type="text"
          placeholder="@seuusuario"
          value={username}
          onChange={(e) => setUsername(e.target.value)}
          required
          disabled={isPending}
          sizing="lg"
          className="w-full"
        />
      </div>

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
            Criando conta...
          </span>
        ) : (
          "Criar conta"
        )}
      </AppButton>
    </form>
  );
}
