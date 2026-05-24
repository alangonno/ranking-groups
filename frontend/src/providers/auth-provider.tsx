import { createContext, useContext, useState, type ReactNode } from "react";
import { getUserFromToken } from "../lib/auth-token";

export interface AuthUser {
  id: string;
  name: string;
  email: string;
  username: string;
}

interface AuthContextType {
  user: AuthUser | null;
  setUser: (user: AuthUser) => void;
  clearUser: () => void;
  isAuthenticated: boolean;
}

const AuthContext = createContext<AuthContextType | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUserState] = useState<AuthUser | null>(() =>
    getUserFromToken()
  );

  const setUser = (newUser: AuthUser) => {
    setUserState(newUser);
  };

  const clearUser = () => {
    setUserState(null);
  };

  return (
    <AuthContext.Provider
      value={{ user, setUser, clearUser, isAuthenticated: !!user }}
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
