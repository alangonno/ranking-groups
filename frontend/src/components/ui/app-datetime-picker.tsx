import DatePicker from "react-datepicker";
import "react-datepicker/dist/react-datepicker.css";
import { CalendarDays, ChevronLeft, ChevronRight } from "lucide-react";

interface AppDateTimePickerProps {
  value: string;
  onChange: (value: string) => void;
  disabled?: boolean;
}

function toEndOfDayISO(d: Date | null): string {
  if (!d) return "";
  const end = new Date(d);
  end.setHours(23, 59, 59, 999);
  return end.toISOString();
}

function toDate(value: string): Date | null {
  if (!value) return null;
  const d = new Date(value);
  return isNaN(d.getTime()) ? null : d;
}

export function AppDateTimePicker({ value, onChange, disabled }: AppDateTimePickerProps) {
  const selected = toDate(value);

  return (
    <div className="relative">
      <div className="flex items-center bg-surface-container-low dark:bg-surface-container rounded-lg cursor-pointer focus-within:ring-2 focus-within:ring-primary/30">
        <DatePicker
          selected={selected}
          onChange={(d: Date | null) => onChange(toEndOfDayISO(d))}
          disabled={disabled}
          dateFormat="dd/MM/yyyy"
          placeholderText="Selecione a data"
          className="w-full bg-transparent text-text-primary px-4 py-2.5 text-sm outline-none cursor-pointer"
          popperClassName="z-50"
          calendarClassName="!bg-surface-container-lowest !border !border-border !rounded-xl !shadow-lg !font-sans"
          dayClassName={(d) => {
            const today = new Date();
            const isToday =
              d.getDate() === today.getDate() &&
              d.getMonth() === today.getMonth() &&
              d.getFullYear() === today.getFullYear();
            return isToday ? "!bg-primary !text-on-primary !rounded-full" : "!text-text-primary hover:!bg-surface-container !rounded-full";
          }}
          renderCustomHeader={({
            date,
            decreaseMonth,
            increaseMonth,
          }) => (
            <div className="flex items-center justify-between px-2 py-2">
              <button
                type="button"
                onClick={(e) => { e.stopPropagation(); decreaseMonth(); }}
                className="p-1 rounded-full hover:bg-surface-container text-text-secondary"
              >
                <ChevronLeft size={16} />
              </button>
              <span className="text-sm font-semibold text-text-primary">
                {date.toLocaleDateString("pt-BR", { month: "long", year: "numeric" })}
              </span>
              <button
                type="button"
                onClick={(e) => { e.stopPropagation(); increaseMonth(); }}
                className="p-1 rounded-full hover:bg-surface-container text-text-secondary"
              >
                <ChevronRight size={16} />
              </button>
            </div>
          )}
        />
        <CalendarDays size={18} className="mr-3 text-text-secondary flex-shrink-0 pointer-events-none" />
      </div>
      <style>{`
        .react-datepicker__header {
          background: transparent !important;
          border-bottom: 1px solid var(--border, #e5e7eb) !important;
          padding-top: 8px !important;
        }
        .react-datepicker__day-name {
          color: var(--text-secondary, #6b7280) !important;
          font-size: 12px !important;
          width: 36px !important;
          line-height: 36px !important;
        }
        .react-datepicker__day {
          width: 36px !important;
          line-height: 36px !important;
          font-size: 13px !important;
          margin: 1px !important;
        }
        .react-datepicker__day--selected {
          background: var(--color-primary, #6366f1) !important;
          color: white !important;
          border-radius: 9999px !important;
        }
        .react-datepicker__day--keyboard-selected {
          background: var(--color-primary, #6366f1) !important;
          color: white !important;
          border-radius: 9999px !important;
        }
      `}</style>
    </div>
  );
}
