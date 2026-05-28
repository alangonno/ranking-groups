import { type SelectHTMLAttributes } from "react";
import { ChevronDown } from "lucide-react";

type AppSelectProps = SelectHTMLAttributes<HTMLSelectElement> & {
  sizing?: "sm" | "md" | "lg";
};

const SIZING_CLASSES = {
  sm: "px-3 py-2 text-xs",
  md: "px-4 py-2.5 text-sm",
  lg: "px-4 py-3 text-base",
} as const;

export function AppSelect({ sizing = "md", className = "", children, ...props }: AppSelectProps) {
  return (
    <div className={`relative ${className}`}>
      <select
        className={`appearance-none w-full bg-gray-100 text-text-primary ${SIZING_CLASSES[sizing]} rounded-full border-0 focus:ring-2 focus:ring-primary/30 cursor-pointer pr-10`}
        {...props}
      >
        {children}
      </select>
      <ChevronDown
        size={16}
        className="absolute right-3 top-1/2 -translate-y-1/2 text-text-secondary pointer-events-none"
      />
    </div>
  );
}
