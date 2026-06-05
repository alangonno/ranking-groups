import type { CursorPage } from "../types/common/cursor-page";

export function flattenPages<T>(pages: CursorPage<T>[]): T[] {
  return pages.flatMap((page) => page.items);
}

export function getNextPageParam(lastPage: CursorPage<unknown>): string | undefined {
  return lastPage.nextCursor ?? undefined;
}
