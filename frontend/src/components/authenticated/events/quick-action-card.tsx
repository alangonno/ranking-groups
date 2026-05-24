import { ThumbsUp, ThumbsDown, Users } from "lucide-react";

interface QuickActionCardsProps {
  onCreatePositive?: () => void;
  onCreateNegative?: () => void;
  onCreateShared?: () => void;
}

export function QuickActionCards({
  onCreatePositive,
  onCreateNegative,
  onCreateShared,
}: QuickActionCardsProps) {
  return (
    <div className="grid grid-cols-3 gap-4">
      <button
        type="button"
        onClick={onCreatePositive}
        className="bg-white rounded-2xl p-6 shadow-[0_1px_3px_rgba(0,0,0,0.05)] hover:shadow-md transition-shadow text-center group"
      >
        <div className="w-14 h-14 rounded-full bg-blue-50 flex items-center justify-center mx-auto mb-3 group-hover:scale-110 transition-transform">
          <ThumbsUp size={28} className="text-blue-500" />
        </div>
        <span className="text-sm font-semibold text-text-primary">
          Evento Positivo
        </span>
        <p className="text-xs text-text-secondary mt-1">
          Registrar boa ação
        </p>
      </button>

      <button
        type="button"
        onClick={onCreateNegative}
        className="bg-white rounded-2xl p-6 shadow-[0_1px_3px_rgba(0,0,0,0.05)] hover:shadow-md transition-shadow text-center group"
      >
        <div className="w-14 h-14 rounded-full bg-red-50 flex items-center justify-center mx-auto mb-3 group-hover:scale-110 transition-transform">
          <ThumbsDown size={28} className="text-primary" />
        </div>
        <span className="text-sm font-semibold text-text-primary">
          Evento Negativo
        </span>
        <p className="text-xs text-text-secondary mt-1">
          Registrar infração
        </p>
      </button>

      <button
        type="button"
        onClick={onCreateShared}
        className="bg-white rounded-2xl p-6 shadow-[0_1px_3px_rgba(0,0,0,0.05)] hover:shadow-md transition-shadow text-center group"
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
