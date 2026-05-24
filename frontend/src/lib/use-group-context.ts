import { useLocation } from "react-router-dom";

export function useCurrentGroupId(): string | null {
  const { pathname } = useLocation();
  const match = pathname.match(/^\/group\/([^\/]+)/);
  return match ? match[1] : null;
}
