import { Button, type ButtonProps } from "flowbite-react";

export type AppButtonProps = ButtonProps;

export function AppButton({ className, color, ...props }: AppButtonProps) {
  const themeOverride =
    color === "red" || !color
      ? "!bg-primary !text-on-primary hover:!bg-primary-hover focus:!ring-primary/30"
      : color === "gray"
      ? "!bg-surface-container-high !text-on-surface hover:!bg-surface-container-highest focus:!ring-primary/30"
      : color === "light"
      ? "!bg-surface-container-lowest !text-text-primary !border-border hover:!bg-surface-container-low focus:!ring-primary/30"
      : "";

  return (
    <Button
      color={themeOverride ? undefined : color}
      className={`py-2.5 ${themeOverride} ${className ?? ""}`}
      {...props}
    />
  );
}
