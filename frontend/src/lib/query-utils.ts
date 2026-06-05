import type { InfiniteData, QueryClient } from "@tanstack/react-query";
import type { CursorPage } from "../types/common/cursor-page";

export function invalidateFirstPage(queryClient: QueryClient, queryKey: unknown[]) {
  queryClient.setQueryData<InfiniteData<CursorPage<unknown>>>(queryKey, (old) => {
    if (!old) return old;
    return {
      ...old,
      pages: old.pages.slice(0, 1),
      pageParams: old.pageParams.slice(0, 1),
    };
  });
  queryClient.invalidateQueries({ queryKey });
}
