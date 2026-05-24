const ACCESS_TOKEN_KEY = "access_token";

export function getAccessToken(): string | null {
  return localStorage.getItem(ACCESS_TOKEN_KEY);
}

export function setAccessToken(token: string): void {
  localStorage.setItem(ACCESS_TOKEN_KEY, token);
}

export function removeAccessToken(): void {
  localStorage.removeItem(ACCESS_TOKEN_KEY);
}

export function getUserIdFromToken(): string | null {
  const token = getAccessToken();
  if (!token) return null;

  try {
    const payload = token.split(".")[1];
    if (!payload) return null;

    const decoded = JSON.parse(atob(payload));
    return decoded.sub || decoded.userId || decoded.id || null;
  } catch {
    return null;
  }
}

export function getUserFromToken(): {
  id: string;
  name: string;
  email: string;
  username: string;
} | null {
  const token = getAccessToken();
  if (!token) return null;

  try {
    const payload = token.split(".")[1];
    if (!payload) return null;

    const decoded = JSON.parse(atob(payload));
    return {
      id: decoded.sub || decoded.userId || decoded.id || "",
      name: decoded.name || "",
      email: decoded.email || "",
      username: decoded.username || "",
    };
  } catch {
    return null;
  }
}
