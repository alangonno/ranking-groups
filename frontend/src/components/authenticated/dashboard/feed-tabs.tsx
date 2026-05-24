type FeedTab = "all" | "pending";

interface FeedTabsProps {
  activeTab: FeedTab;
  onTabChange: (tab: FeedTab) => void;
  pendingCount?: number;
}

export function FeedTabs({ activeTab, onTabChange, pendingCount = 0 }: FeedTabsProps) {
  return (
    <div className="flex gap-2">
      <button
        type="button"
        onClick={() => onTabChange("all")}
        className={`px-4 py-2 rounded-full text-sm font-medium transition-all ${
          activeTab === "all"
            ? "bg-white text-text-primary shadow-sm border border-border"
            : "bg-gray-100 text-text-secondary hover:bg-gray-200"
        }`}
      >
        Todos
      </button>
      <button
        type="button"
        onClick={() => onTabChange("pending")}
        className={`px-4 py-2 rounded-full text-sm font-medium transition-all flex items-center gap-1.5 ${
          activeTab === "pending"
            ? "bg-white text-text-primary shadow-sm border border-border"
            : "bg-gray-100 text-text-secondary hover:bg-gray-200"
        }`}
      >
        Pendentes
        {pendingCount > 0 && (
          <span className="bg-primary text-white text-[10px] font-bold w-4 h-4 rounded-full flex items-center justify-center">
            {pendingCount}
          </span>
        )}
      </button>
    </div>
  );
}
