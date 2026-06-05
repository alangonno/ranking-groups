import { Badge, type BadgeProps } from "flowbite-react";

export type AppBadgeProps = Omit<BadgeProps, "color"> & {
  color?: "gray" | "red" | "blue" | "green" | "yellow";
};

export function AppBadge({ color, className, ...props }: AppBadgeProps) {
  const themeColors: Record<string, string> = {
    gray: "!bg-surface-container-high !text-on-surface dark:!bg-surface-container dark:!text-on-surface",
    red: "!bg-primary/10 !text-primary dark:!bg-primary/20 dark:!text-primary",
    blue: "!bg-tertiary/10 !text-tertiary dark:!bg-tertiary/20 dark:!text-tertiary",
    green: "!bg-success/10 !text-success dark:!bg-success/20 dark:!text-success",
    yellow: "!bg-warning/10 !text-warning dark:!bg-warning/20 dark:!text-warning",
  };

  const override = color && themeColors[color] ? themeColors[color] : "";

  return <Badge className={`${override} ${className ?? ""}`} {...props} />;
}
