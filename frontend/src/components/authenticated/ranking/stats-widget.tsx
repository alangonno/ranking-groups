import { AppCard } from "../../ui/app-card";
import { Trophy, Users } from "lucide-react";

interface StatsWidgetProps {
  weeklyEvents: number;
  activeMembers: number;
}

export function StatsWidget({ weeklyEvents, activeMembers }: StatsWidgetProps) {
  return (
    <div className="grid grid-cols-2 gap-4">
      <AppCard className="shadow-[0_1px_3px_rgba(0,0,0,0.05)] bg-red-50 p-5 flex flex-col items-center justify-center text-center">
        <div className="flex items-center gap-2 mb-3">
          <Trophy size={18} className="text-primary" />
          <span className="text-xs font-medium text-text-secondary">
            Eventos Semana
          </span>
        </div>
        <p className="text-3xl font-bold text-text-primary">{weeklyEvents}</p>
      </AppCard>

      <AppCard className="shadow-[0_1px_3px_rgba(0,0,0,0.05)] bg-gray-100 p-5 flex flex-col items-center justify-center text-center">
        <div className="flex items-center gap-2 mb-3">
          <Users size={18} className="text-text-secondary" />
          <span className="text-xs font-medium text-text-secondary">
            Membros Ativos
          </span>
        </div>
        <p className="text-3xl font-bold text-text-primary">{activeMembers}</p>
      </AppCard>
    </div>
  );
}
