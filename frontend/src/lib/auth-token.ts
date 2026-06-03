export function decodeTokenPayload(token: string): Record<string, unknown> | null {
  try {
    const payload = token.split(".")[1];
    if (!payload) return null;
    return JSON.parse(atob(payload));
  } catch {
    return null;
  }
}

export function getUserIdFromToken(token: string): string | null {
  const decoded = decodeTokenPayload(token);
  if (!decoded) return null;
  return (decoded.sub as string) || (decoded.userId as string) || (decoded.id as string) || null;
}

export function getUserFromToken(token: string): {
  id: string;
  name: string;
  email: string;
  username: string;
} | null {
  const decoded = decodeTokenPayload(token);
  if (!decoded) return null;

  return {
    id: (decoded.sub as string) || (decoded.userId as string) || (decoded.id as string) || "",
    name: (decoded.name as string) || "",
    email: (decoded.email as string) || "",
    username: (decoded.username as string) || "",
  };
}

export function isTokenExpiringSoon(token: string, seconds: number = 60): boolean {
  const decoded = decodeTokenPayload(token);
  if (!decoded) return true;

  const exp = decoded.exp as number | undefined;
  if (!exp) return true;

  const expiresAt = exp * 1000;
  const now = Date.now();
  return expiresAt - now < seconds * 1000;
}
