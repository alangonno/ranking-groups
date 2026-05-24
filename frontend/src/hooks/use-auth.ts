import { useMutation, useQuery } from "@tanstack/react-query";
import { postJson } from "../lib/api";
import {
  getAccessToken,
  getUserIdFromToken,
  removeAccessToken,
  setAccessToken,
} from "../lib/auth-token";
import { queryClient } from "../lib/query-client";
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
  return useMutation({
    mutationFn: (payload: LoginRequest) =>
      postJson<LoginResponse>("/api/auth/login", payload),
    onSuccess: (data) => {
      setAccessToken(data.accessToken);
      queryClient.setQueryData(AUTH_KEY, mapToUser(data));
    },
  });
}

export function useRegister() {
  return useMutation({
    mutationFn: (payload: RegisterRequest) =>
      postJson<RegisterResponse>("/api/auth/register", payload),
    onSuccess: (data) => {
      setAccessToken(data.accessToken);
      queryClient.setQueryData(AUTH_KEY, mapToUser(data));
    },
  });
}

export function useLogout() {
  return useMutation({
    mutationFn: async () => {
      removeAccessToken();
      queryClient.clear();
    },
  });
}

export function useCurrentUser() {
  const userId = getUserIdFromToken();

  return useQuery<User | null>({
    queryKey: AUTH_KEY,
    queryFn: async () => {
      const token = getAccessToken();
      if (!token) return null;

      const cached = queryClient.getQueryData<User>(AUTH_KEY);
      if (cached) return cached;

      return null;
    },
    enabled: !!userId,
    staleTime: 5 * 60 * 1000,
  });
}

export function getCurrentUserId(): string | null {
  return getUserIdFromToken();
}
