import type { User } from "../auth/user";

export interface RankingEntry {
  user: User;
  score: number;
}

export interface RankingQueryParams {
  fromDate?: string;
  toDate?: string;
}
