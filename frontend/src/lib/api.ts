import axios from "axios";
import type { RefreshTokenResponse } from "../types/auth/user";
import { authStore } from "../store/auth-store";
import { getRefreshToken, setRefreshToken, removeRefreshToken } from "../lib/refresh-token-storage";

export class ApiError extends Error {
  statusCode: number;
  data: unknown;
  type?: string;
  rule?: string;

  constructor(
    message: string,
    statusCode: number,
    data: unknown,
    type?: string,
    rule?: string
  ) {
    super(message);
    this.name = "ApiError";
    this.statusCode = statusCode;
    this.data = data;
    this.type = type;
    this.rule = rule;
  }
}

const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || "",
  headers: {
    "Content-Type": "application/json",
  },
  withCredentials: true,
});

let isRefreshing = false;
let failedQueue: Array<{
  resolve: (value: string) => void;
  reject: (reason?: unknown) => void;
}> = [];

function processQueue(error: unknown | null, token: string | null = null) {
  failedQueue.forEach((prom) => {
    if (error || !token) {
      prom.reject(error);
    } else {
      prom.resolve(token);
    }
  });
  failedQueue = [];
}

apiClient.interceptors.request.use((config) => {
  const token = authStore.getAccessToken();
  if (token) {
    config.headers.set("Authorization", `Bearer ${token}`);
  }
  return config;
});

apiClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;

    if (
      axios.isAxiosError(error) &&
      error.response?.status === 401 &&
      originalRequest &&
      !originalRequest._retry
    ) {
      if (isRefreshing) {
        return new Promise<string>((resolve, reject) => {
          failedQueue.push({ resolve, reject });
        })
          .then((token) => {
            originalRequest.headers.set("Authorization", `Bearer ${token}`);
            return apiClient(originalRequest);
          })
          .catch((err) => Promise.reject(err));
      }

      originalRequest._retry = true;
      isRefreshing = true;

      try {
        const refreshToken = getRefreshToken();
        if (!refreshToken) {
          throw new Error("No refresh token available");
        }

        const response = await apiClient.post<RefreshTokenResponse>(
          "/api/auth/refresh-token",
          { refreshToken }
        );
        const { accessToken, refreshToken: newRefreshToken } = response.data;
        authStore.setAccessToken(accessToken);
        setRefreshToken(newRefreshToken);
        processQueue(null, accessToken);
        originalRequest.headers.set("Authorization", `Bearer ${accessToken}`);
        return apiClient(originalRequest);
      } catch (refreshError) {
        processQueue(refreshError, null);
        authStore.clearAccessToken();
        removeRefreshToken();
        window.location.href = "/login";
        return Promise.reject(refreshError);
      } finally {
        isRefreshing = false;
      }
    }

    if (axios.isAxiosError(error)) {
      const data = error.response?.data as Record<string, unknown> | undefined;
      const message =
        (data?.Message as string) ||
        error.message ||
        "Erro na requisição";
      const statusCode = error.response?.status || 500;
      const type = data?.Type as string | undefined;
      const rule = data?.Rule as string | undefined;
      return Promise.reject(new ApiError(message, statusCode, data, type, rule));
    }

    return Promise.reject(error);
  }
);

export default apiClient;

export async function getJson<TResponse>(
  url: string,
  params?: Record<string, unknown>
): Promise<TResponse> {
  try {
    const response = await apiClient.get<TResponse>(url, { params });
    return response.data;
  } catch (error) {
    if (error instanceof ApiError) {
      throw error;
    }
    throw new ApiError("Erro ao realizar GET", 500, error);
  }
}

export async function postJson<TResponse>(
  url: string,
  body?: unknown
): Promise<TResponse> {
  try {
    const response = await apiClient.post<TResponse>(url, body);
    return response.data;
  } catch (error) {
    if (error instanceof ApiError) {
      throw error;
    }
    throw new ApiError("Erro ao realizar POST", 500, error);
  }
}

export async function patchJson<TResponse>(
  url: string,
  body?: unknown
): Promise<TResponse> {
  try {
    const response = await apiClient.patch<TResponse>(url, body);
    return response.data;
  } catch (error) {
    if (error instanceof ApiError) {
      throw error;
    }
    throw new ApiError("Erro ao realizar PATCH", 500, error);
  }
}

export async function putJson<TResponse>(
  url: string,
  body?: unknown
): Promise<TResponse> {
  try {
    const response = await apiClient.put<TResponse>(url, body);
    return response.data;
  } catch (error) {
    if (error instanceof ApiError) {
      throw error;
    }
    throw new ApiError("Erro ao realizar PUT", 500, error);
  }
}

export async function deleteJson<TResponse>(
  url: string,
  params?: Record<string, unknown>
): Promise<TResponse> {
  try {
    const response = await apiClient.delete<TResponse>(url, { params });
    return response.data;
  } catch (error) {
    if (error instanceof ApiError) {
      throw error;
    }
    throw new ApiError("Erro ao realizar DELETE", 500, error);
  }
}
