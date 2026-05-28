type FeedTab = "all" | "pending";

interface FeedTabsProps {
  activeTab: FeedTab;
  onTabChange: (tab: FeedTab) => void;
  pendingCount?: number;
}

export function FeedTabs({ activeTab, onTabChange, pendingCount = 0 }: FeedTabsProps) {
  return (
    <div className="flex bg-surface-container-low p-1 rounded-full border border-surface-container-highest w-full md:w-auto shadow-sm">
      <button
        type="button"
        onClick={() => onTabChange("all")}
        className={`flex-1 md:flex-none px-5 py-2 rounded-full text-label-bold font-label-bold transition-all duration-200 ${
          activeTab === "all"
            ? "bg-surface-container-lowest shadow-sm text-on-surface"
            : "text-secondary hover:text-on-surface"
        }`}
      >
        Todos
      </button>
      <button
        type="button"
        onClick={() => onTabChange("pending")}
        className={`flex-1 md:flex-none px-5 py-2 rounded-full text-label-bold font-label-bold transition-all duration-200 relative ${
          activeTab === "pending"
            ? "bg-surface-container-lowest shadow-sm text-on-surface"
            : "text-secondary hover:text-on-surface"
        }`}
      >
        Pendentes
        {pendingCount > 0 && (
          <span className="ml-1.5 inline-flex items-center text-white justify-center w-5 h-5 bg-primary text-on-primary rounded-full text-[10px] font-bold">
            {pendingCount}
          </span>
        )}
      </button>
    </div>
  );
}
