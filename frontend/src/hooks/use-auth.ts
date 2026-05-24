import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { postJson } from "../lib/api";
import {
  getAccessToken,
  getUserIdFromToken,
  removeAccessToken,
  setAccessToken,
} from "../lib/auth-token";
import { queryClient } from "../lib/query-client";
import { LoginRequest, LoginResponse } from "../types/auth/user";
import { RegisterRequest, RegisterResponse } from "../types/auth/user";
import { User } from "../types/auth/user";

const AUTH_KEY = ["auth", "me"];

export function useLogin() {
  return useMutation({
    mutationFn: (payload: LoginRequest) =>
      postJson<LoginResponse>("/api/auth/login", payload),
    onSuccess: (data) => {
      setAccessToken(data.accessToken);
      queryClient.setQueryData(AUTH_KEY, data.user);
    },
  });
}

export function useRegister() {
  return useMutation({
    mutationFn: (payload: RegisterRequest) =>
      postJson<RegisterResponse>("/api/auth/register", payload),
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
