import { Card, type CardProps } from "flowbite-react";

export type AppCardProps = CardProps;

export function AppCard({ className, ...props }: AppCardProps) {
  return (
    <Card
      className={`!bg-surface-container-lowest !border-border dark:!bg-surface dark:!border-surface-container-high shadow-[0_1px_3px_rgba(0,0,0,0.05)] ${className ?? ""}`}
      {...props}
    />
  );
}
