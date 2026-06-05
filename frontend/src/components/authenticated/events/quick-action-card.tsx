import { Plus, Users } from "lucide-react";

interface QuickActionCardsProps {
  onCreateEvent?: () => void;
  onCreateShared?: () => void;
}

export function QuickActionCards({
  onCreateEvent,
  onCreateShared,
}: QuickActionCardsProps) {
  return (
    <div className="grid grid-cols-2 gap-4">
      <button
        type="button"
        onClick={onCreateEvent}
        className="bg-surface-container-lowest dark:bg-surface rounded-2xl p-6 shadow-[0_1px_3px_rgba(0,0,0,0.05)] hover:shadow-md transition-shadow text-center group"
      >
        <div className="w-14 h-14 rounded-full bg-blue-50 flex items-center justify-center mx-auto mb-3 group-hover:scale-110 transition-transform">
          <Plus size={28} className="text-blue-500" />
        </div>
        <span className="text-sm font-semibold text-text-primary">
          Novo Evento
        </span>
        <p className="text-xs text-text-secondary mt-1">
          Registrar pontuação
        </p>
      </button>

      <button
        type="button"
        onClick={onCreateShared}
        className="bg-surface-container-lowest dark:bg-surface rounded-2xl p-6 shadow-[0_1px_3px_rgba(0,0,0,0.05)] hover:shadow-md transition-shadow text-center group"
      >
        <div className="w-14 h-14 rounded-full bg-purple-50 flex items-center justify-center mx-auto mb-3 group-hover:scale-110 transition-transform">
          <Users size={28} className="text-purple-500" />
        </div>
        <span className="text-sm font-semibold text-text-primary">
          Evento Compartilhado
        </span>
        <p className="text-xs text-text-secondary mt-1">
          Atividade em grupo
        </p>
      </button>
    </div>
  );
}
