import { ChevronDown } from "lucide-react";

interface RankingFilterProps {
  value: string;
  onChange: (value: string) => void;
}

const options = [
  { value: "month", label: "Este Mês" },
  { value: "last-month", label: "Último Mês" },
  { value: "all", label: "Todo Período" },
];

export function RankingFilter({ value, onChange }: RankingFilterProps) {
  return (
    <div className="relative">
      <select
        value={value}
        onChange={(e) => onChange(e.target.value)}
        className="appearance-none bg-gray-100 text-text-primary text-sm font-medium pl-4 pr-10 py-2 rounded-full border-0 focus:ring-2 focus:ring-primary/30 cursor-pointer"
      >
        {options.map((opt) => (
          <option key={opt.value} value={opt.value}>
            {opt.label}
          </option>
        ))}
      </select>
      <ChevronDown
        size={16}
        className="absolute right-3 top-1/2 -translate-y-1/2 text-text-secondary pointer-events-none"
      />
    </div>
  );
}
