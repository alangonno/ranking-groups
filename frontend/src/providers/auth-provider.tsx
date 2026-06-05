import { createContext, useContext, useState, useEffect, type ReactNode } from "react";
import { getUserFromToken } from "../lib/auth-token";
import { authStore } from "../store/auth-store";
import { postJson } from "../lib/api";
import type { RefreshTokenResponse } from "../types/auth/user";

export interface AuthUser {
  id: string;
  name: string;
  email: string;
  username: string;
  avatarUrl?: string;
}

interface AuthContextType {
  user: AuthUser | null;
  setUser: (user: AuthUser) => void;
  clearUser: () => void;
  isAuthenticated: boolean;
  isLoading: boolean;
}

const AuthContext = createContext<AuthContextType | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUserState] = useState<AuthUser | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    async function trySilentRefresh() {
      const token = authStore.getAccessToken();
      if (token) {
        const existingUser = getUserFromToken(token);
        if (existingUser) {
          setUserState(existingUser);
        }
        setIsLoading(false);
        return;
      }

      try {
        const data = await postJson<RefreshTokenResponse>("/api/auth/refresh-token");
        authStore.setAccessToken(data.accessToken);
        const refreshedUser = getUserFromToken(data.accessToken);
        if (refreshedUser) {
          setUserState(refreshedUser);
        }
      } catch {
        authStore.clearAccessToken();
      } finally {
        setIsLoading(false);
      }
    }

    trySilentRefresh();
  }, []);

  const setUser = (newUser: AuthUser) => {
    setUserState(newUser);
  };

  const clearUser = () => {
    setUserState(null);
    authStore.clearAccessToken();
  };

  return (
    <AuthContext.Provider
      value={{ user, setUser, clearUser, isAuthenticated: !!user, isLoading }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export function useAuthContext(): AuthContextType {
  const ctx = useContext(AuthContext);
  if (!ctx) {
    throw new Error("useAuthContext must be used within AuthProvider");
  }
  return ctx;
}
