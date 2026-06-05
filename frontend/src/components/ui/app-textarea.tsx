import { type TextareaHTMLAttributes } from "react";

type AppTextareaProps = TextareaHTMLAttributes<HTMLTextAreaElement> & {
  sizing?: "sm" | "md" | "lg";
};

const SIZING_CLASSES = {
  sm: "px-3 py-2 text-xs",
  md: "px-4 py-2.5 text-sm",
  lg: "px-4 py-3 text-base",
} as const;

export function AppTextarea({ sizing = "md", className = "", ...props }: AppTextareaProps) {
  return (
    <textarea
      className={`w-full bg-surface-container-low text-text-primary ${SIZING_CLASSES[sizing]} rounded-2xl border-0 focus:ring-2 focus:ring-primary/30 placeholder:text-text-muted resize-none dark:bg-surface-container ${className}`}
      {...props}
    />
  );
}
