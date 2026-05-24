import { Link, useNavigate } from "react-router-dom";
import { useState } from "react";
import { AppCard } from "../../components/ui/app-card";
import { RegisterForm } from "../../components/public/auth/register-form";
import { useRegister } from "../../hooks/use-auth";
import type { RegisterRequest } from "../../types/auth/user";

export function RegisterPage() {
  const register = useRegister();
  const navigate = useNavigate();
  const [error, setError] = useState<string | null>(null);

  function handleSubmit(data: { name: string; username: string; email: string; password: string }) {
    setError(null);
    const payload: RegisterRequest = data;

    register.mutate(payload, {
      onSuccess: () => {
        navigate("/groups");
      },
      onError: (err) => {
        setError(err instanceof Error ? err.message : "Erro ao criar conta");
      },
    });
  }

  return (
    <div className="py-12">
      <div className="text-center mb-8">
        <h1 className="text-3xl font-bold mb-2">
          <span className="text-primary">4</span>
          <span className="text-text-primary"> Quase </span>
          <span className="text-primary">5</span>
        </h1>
        <p className="text-text-secondary text-sm">
          Crie sua conta e comece a competir
        </p>
      </div>

      <AppCard className="p-8 shadow-sm border-0">
        <h2 className="text-lg font-semibold text-text-primary text-center pt-2 mb-6">
          Criar conta
        </h2>
        <RegisterForm
          onSubmit={handleSubmit}
          isPending={register.isPending}
          error={error}
        />
      </AppCard>

      <p className="text-center mt-6 text-sm text-text-secondary">
        Já tem uma conta?{" "}
        <Link
          to="/login"
          className="text-primary font-medium hover:underline"
        >
          Entrar
        </Link>
      </p>
    </div>
  );
}
