import { Search } from "lucide-react";

interface SearchInputProps {
  value: string;
  onChange: (value: string) => void;
  placeholder?: string;
}

export function SearchInput({ value, onChange, placeholder = "Buscar..." }: SearchInputProps) {
  return (
    <div className="relative flex-1 max-w-md">
      <Search
        size={18}
        className="absolute left-3 top-1/2 -translate-y-1/2 text-text-muted"
      />
      <input
        type="text"
        value={value}
        onChange={(e) => onChange(e.target.value)}
        placeholder={placeholder}
        className="w-full pl-10 pr-4 py-2.5 bg-gray-100 text-text-primary text-sm rounded-full border-0 focus:ring-2 focus:ring-primary/30 placeholder:text-text-muted"
      />
    </div>
  );
}
