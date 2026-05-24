import type { BaseEntity } from "../common/base-entity";

export interface User extends BaseEntity {
  name: string;
  username: string;
  email: string;
  avatarUrl?: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  userId: string;
  accessToken: string;
  refreshToken: string;
  name: string;
  username: string;
  email: string;
  avatarUrl?: string;
}

export interface RegisterRequest {
  name: string;
  username: string;
  email: string;
  password: string;
}

export interface RegisterResponse {
  userId: string;
  accessToken: string;
  refreshToken: string;
  name: string;
  username: string;
  email: string;
  avatarUrl?: string;
}

export interface RefreshTokenRequest {
  refreshToken: string;
}

export interface RefreshTokenResponse {
  accessToken: string;
  refreshToken: string;
}
