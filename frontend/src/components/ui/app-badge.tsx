import { Badge, type BadgeProps } from "flowbite-react";

export type AppBadgeProps = BadgeProps;

export function AppBadge(props: AppBadgeProps) {
  return <Badge {...props} />;
}
