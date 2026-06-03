interface AuthStoreState {
  accessToken: string | null;
}

const globalStore = (typeof window !== "undefined"
  ? (window as unknown as Record<string, unknown>).__AUTH_STORE__
  : null) as AuthStoreState | undefined;

const state: AuthStoreState = globalStore ?? { accessToken: null };

if (typeof window !== "undefined") {
  (window as unknown as Record<string, unknown>).__AUTH_STORE__ = state;
}

export const authStore = {
  getAccessToken(): string | null {
    return state.accessToken;
  },

  setAccessToken(token: string): void {
    state.accessToken = token;
  },

  clearAccessToken(): void {
    state.accessToken = null;
  },
};
