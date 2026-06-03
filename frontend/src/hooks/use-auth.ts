import { useMutation, useQuery } from "@tanstack/react-query";
import { postJson } from "../lib/api";
import { getUserIdFromToken } from "../lib/auth-token";
import { authStore } from "../store/auth-store";
import { queryClient } from "../lib/query-client";
import { useAuthContext } from "../providers/auth-provider";
import type { LoginRequest, LoginResponse } from "../types/auth/user";
import type { RegisterRequest, RegisterResponse } from "../types/auth/user";
import type { User } from "../types/auth/user";

const AUTH_KEY = ["auth", "me"];

function mapToUser(data: { userId: string; name: string; username: string; email: string; avatarUrl?: string }): User {
  return {
    id: data.userId,
    name: data.name,
    username: data.username,
    email: data.email,
    avatarUrl: data.avatarUrl,
    createdAt: "",
    updatedAt: "",
  };
}

export function useLogin() {
  const { setUser } = useAuthContext();

  return useMutation({
    mutationFn: (payload: LoginRequest) =>
      postJson<LoginResponse>("/api/auth/login", payload),
    onSuccess: (data) => {
      authStore.setAccessToken(data.accessToken);
      queryClient.setQueryData(AUTH_KEY, mapToUser(data));
      setUser({
        id: data.userId,
        name: data.name,
        email: data.email,
        username: data.username,
      });
    },
  });
}

export function useRegister() {
  const { setUser } = useAuthContext();

  return useMutation({
    mutationFn: (payload: RegisterRequest) =>
      postJson<RegisterResponse>("/api/auth/register", payload),
    onSuccess: (data) => {
      authStore.setAccessToken(data.accessToken);
      queryClient.setQueryData(AUTH_KEY, mapToUser(data));
      setUser({
        id: data.userId,
        name: data.name,
        email: data.email,
        username: data.username,
      });
    },
  });
}

export function useLogout() {
  const { clearUser } = useAuthContext();

  return useMutation({
    mutationFn: async () => {
      await postJson<unknown>("/api/auth/logout");
      authStore.clearAccessToken();
      queryClient.clear();
    },
    onSuccess: () => {
      clearUser();
    },
  });
}

export function useCurrentUser() {
  const { user } = useAuthContext();

  return useQuery<User | null>({
    queryKey: AUTH_KEY,
    queryFn: async () => {
      if (!user) return null;
      return user as User;
    },
    enabled: !!user,
    staleTime: 5 * 60 * 1000,
  });
}

export function getCurrentUserId(): string | null {
  const token = authStore.getAccessToken();
  if (!token) return null;
  return getUserIdFromToken(token);
}
