import { ArrowUpRight } from "lucide-react";

interface HeroScoreCardProps {
  score: number;
  delta: number;
}

export function HeroScoreCard({ score, delta }: HeroScoreCardProps) {
  const formattedScore = score >= 1000 ? `${(score / 1000).toFixed(1)}k` : String(score);

  return (
    <div className="bg-primary rounded-2xl p-5 text-white">
      <div className="flex flex-col">
        <span className="text-[10px] font-bold uppercase tracking-wider text-white/70 mb-1">
          SEU SCORE
        </span>
        <span className="text-4xl font-bold">{formattedScore}</span>

        <div className="mt-3 flex items-center">
          <span className="inline-flex items-center gap-1 bg-white/20 text-white text-xs font-medium px-2.5 py-1 rounded-full">
            <ArrowUpRight size={14} />
            +{delta} hj
          </span>
        </div>
      </div>
    </div>
  );
}
