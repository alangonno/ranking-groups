import { type InputHTMLAttributes } from "react";

type AppInputProps = Omit<InputHTMLAttributes<HTMLInputElement>, "size"> & {
  sizing?: "sm" | "md" | "lg";
};

const SIZING_CLASSES = {
  sm: "px-3 py-2 text-xs",
  md: "px-4 py-2.5 text-sm",
  lg: "px-4 py-3 text-base",
} as const;

export function AppInput({ sizing = "md", className = "", ...props }: AppInputProps) {
  return (
    <input
      className={`w-full bg-surface-container-low text-text-primary ${SIZING_CLASSES[sizing]} rounded-lg border-0 focus:ring-2 focus:ring-primary/30 placeholder:text-text-muted dark:bg-surface-container ${className}`}
      {...props}
    />
  );
}
