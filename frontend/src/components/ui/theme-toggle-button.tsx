import { Sun, Moon } from "lucide-react";
import { useTheme } from "../../providers/theme-provider";

export function ThemeToggleButton() {
  const { theme, toggleTheme } = useTheme();

  return (
    <button
      onClick={toggleTheme}
      aria-label={
        theme === "light" ? "Ativar modo escuro" : "Ativar modo claro"
      }
      className="fixed z-50 flex items-center justify-center w-10 h-10 rounded-full shadow-[0_1px_3px_rgba(0,0,0,0.1)] border transition-colors focus:outline-none focus:ring-2 focus:ring-primary focus:ring-offset-2 focus:ring-offset-background
        bottom-20 right-4 lg:top-4 lg:bottom-auto
        bg-white text-text-primary border-border hover:bg-gray-100
        dark:bg-surface-container-high dark:text-on-surface dark:border-outline-variant dark:hover:bg-surface-container-highest"
    >
      {theme === "light" ? (
        <Moon className="w-5 h-5" />
      ) : (
        <Sun className="w-5 h-5" />
      )}
    </button>
  );
}
