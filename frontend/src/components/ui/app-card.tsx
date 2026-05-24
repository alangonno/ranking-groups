import { Card, type CardProps } from "flowbite-react";

export type AppCardProps = CardProps;

export function AppCard(props: AppCardProps) {
  return <Card {...props} />;
}
