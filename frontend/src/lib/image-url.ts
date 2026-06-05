const SUPABASE_URL = import.meta.env.VITE_SUPABASE_URL || "";

export function getImageUrl(path?: string): string | undefined {
  if (!path) return undefined;
  if (path.startsWith("http://") || path.startsWith("https://")) return path;
  const cleanPath = path.startsWith("/") ? path : `/${path}`;
  return `${SUPABASE_URL}/storage/v1/object/public${cleanPath}`;
}

export function getAvatarUrl(avatarUrl?: string): string | undefined {
  if (!avatarUrl) return undefined;
  return getImageUrl(avatarUrl);
}
