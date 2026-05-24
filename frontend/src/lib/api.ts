import axios from "axios";

export class ApiError extends Error {
  statusCode: number;
  data: unknown;

  constructor(
    message: string,
    statusCode: number,
    data: unknown
  ) {
    super(message);
    this.name = "ApiError";
    this.statusCode = statusCode;
    this.data = data;
  }
}

const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || "",
  headers: {
    "Content-Type": "application/json",
  },
});

apiClient.interceptors.request.use((config) => {
  const token = localStorage.getItem("access_token");
  if (token) {
    config.headers.set("Authorization", `Bearer ${token}`);
  }
  return config;
});

apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (axios.isAxiosError(error)) {
      const message =
        error.response?.data?.message ||
        error.message ||
        "Erro na requisição";
      const statusCode = error.response?.status || 500;
      const data = error.response?.data || null;
      return Promise.reject(new ApiError(message, statusCode, data));
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

export async function deleteJson<TResponse>(url: string): Promise<TResponse> {
  try {
    const response = await apiClient.delete<TResponse>(url);
    return response.data;
  } catch (error) {
    if (error instanceof ApiError) {
      throw error;
    }
    throw new ApiError("Erro ao realizar DELETE", 500, error);
  }
}
