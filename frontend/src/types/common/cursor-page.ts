export interface CursorPage<T> {
  items: T[];
  hasMore: boolean;
  nextCursor: string | null;
}
