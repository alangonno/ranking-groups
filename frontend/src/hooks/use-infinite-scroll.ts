import { useEffect, useRef, useCallback } from "react";

interface UseInfiniteScrollOptions {
  onIntersect: () => void;
  hasMore: boolean;
  isLoading: boolean;
}

export function useInfiniteScroll({ onIntersect, hasMore, isLoading }: UseInfiniteScrollOptions) {
  const observerRef = useRef<IntersectionObserver | null>(null);
  const sentinelRef = useRef<HTMLDivElement | null>(null);

  const setSentinelRef = useCallback(
    (node: HTMLDivElement | null) => {
      sentinelRef.current = node;

      if (observerRef.current) {
        observerRef.current.disconnect();
      }

      if (!node || !hasMore || isLoading) return;

      observerRef.current = new IntersectionObserver(
        (entries) => {
          if (entries[0].isIntersecting) {
            onIntersect();
          }
        },
        { threshold: 0.1, rootMargin: "100px" }
      );

      observerRef.current.observe(node);
    },
    [onIntersect, hasMore, isLoading]
  );

  useEffect(() => {
    return () => {
      observerRef.current?.disconnect();
    };
  }, []);

  return { sentinelRef: setSentinelRef };
}
