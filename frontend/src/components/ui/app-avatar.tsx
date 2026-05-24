import { Avatar, type AvatarProps } from "flowbite-react";

export type AppAvatarProps = AvatarProps;

export function AppAvatar(props: AppAvatarProps) {
  return <Avatar {...props} />;
}
