import { Link, useNavigate } from "react-router-dom";
import { useState } from "react";
import { AppCard } from "../../components/ui/app-card";
import { LoginForm } from "../../components/public/auth/login-form";
import { useLogin } from "../../hooks/use-auth";

export function LoginPage() {
  const login = useLogin();
  const navigate = useNavigate();
  const [error, setError] = useState<string | null>(null);

  function handleSubmit(data: { email: string; password: string }) {
    setError(null);
    login.mutate(data, {
      onSuccess: () => {
        navigate("/groups");
      },
      onError: (err) => {
        setError(err instanceof Error ? err.message : "Erro ao fazer login");
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
          Entre para acessar seus grupos e ranking
        </p>
      </div>

      <AppCard className="p-8 shadow-sm border-0">
        <h2 className="text-lg font-semibold text-text-primary text-center pt-2 mb-6">
          Bem-vindo de volta
        </h2>
        <LoginForm
          onSubmit={handleSubmit}
          isPending={login.isPending}
          error={error}
        />
      </AppCard>

      <p className="text-center mt-6 text-sm text-text-secondary">
        Ainda não tem conta?{" "}
        <Link
          to="/register"
          className="text-primary font-medium hover:underline"
        >
          Criar conta
        </Link>
      </p>
    </div>
  );
}
