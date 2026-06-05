import { useState } from "react";
import { Bell, BellDot, Check, Trash2 } from "lucide-react";
import {
  useNotifications,
  useMarkNotificationAsRead,
  useMarkAllNotificationsAsRead,
} from "../../../hooks/use-notifications";
import { useCurrentGroupId } from "../../../lib/use-group-context";
import { useNavigate } from "react-router-dom";

export function NotificationDropdown() {
  const [isOpen, setIsOpen] = useState(false);
  const groupId = useCurrentGroupId();
  const navigate = useNavigate();
  const { data: notificationsData, isLoading } = useNotifications(groupId);
  const markAsRead = useMarkNotificationAsRead();
  const markAllAsRead = useMarkAllNotificationsAsRead();

  const notifications = notificationsData?.flattened ?? [];
  const notificationCount = notifications?.length ?? 0;
  const hasNotifications = notificationCount > 0;

  function handleNotificationClick(notification: {
    notificationId: string;
    eventId?: string;
    sharedEventId?: string;
  }) {
    if (notification.eventId) {
      navigate(`/group/${groupId}/events`);
    } else if (notification.sharedEventId) {
      navigate(`/group/${groupId}/events`);
    }
    markAsRead.mutate(notification.notificationId);
    setIsOpen(false);
  }

  function handleMarkAllAsRead() {
    markAllAsRead.mutate(groupId);
  }

  return (
    <div className="relative">
      <button
        type="button"
        onClick={() => setIsOpen(!isOpen)}
        className="relative p-2 rounded-lg hover:bg-surface-container transition-colors"
        aria-label="Notificações"
      >
        {hasNotifications ? (
          <BellDot size={20} className="text-primary" />
        ) : (
          <Bell size={20} className="text-secondary" />
        )}
        {hasNotifications && (
          <span className="absolute -top-1 -right-1 w-5 h-5 bg-error text-white text-xs rounded-full flex items-center justify-center font-bold">
            {notificationCount > 9 ? "9+" : notificationCount}
          </span>
        )}
      </button>

      {isOpen && (
        <>
          <div
            className="fixed inset-0 z-40"
            onClick={() => setIsOpen(false)}
          />
          <div className="absolute right-0 mt-2 w-80 bg-surface-container-lowest dark:bg-surface rounded-xl shadow-lg border border-surface-container z-50 max-h-96 overflow-hidden lg:left-1/2 lg:-translate-x-1/2 lg:right-auto">
            <div className="flex items-center justify-between px-4 py-3 border-b border-surface-container">
              <h3 className="text-label-bold font-label-bold text-on-surface">
                Notificações
              </h3>
              {hasNotifications && (
                <button
                  type="button"
                  onClick={handleMarkAllAsRead}
                  disabled={markAllAsRead.isPending}
                  className="text-xs text-primary hover:text-primary-dark font-medium flex items-center gap-1 disabled:opacity-50"
                >
                  <Check size={14} />
                  Marcar todas como vistas
                </button>
              )}
            </div>

            <div className="overflow-y-auto max-h-80">
              {isLoading ? (
                <div className="px-4 py-8 text-center text-secondary text-sm">
                  Carregando...
                </div>
              ) : !hasNotifications ? (
                <div className="px-4 py-8 text-center text-secondary text-sm">
                  <Bell size={24} className="mx-auto mb-2 text-text-muted" />
                  <p>Nenhuma notificação</p>
                </div>
              ) : (
                <div className="divide-y divide-surface-container">
                  {notifications?.map((notification) => (
                    <div
                      key={notification.notificationId}
                      className="px-4 py-3 hover:bg-surface-container/50 cursor-pointer group"
                      onClick={() => handleNotificationClick(notification)}
                    >
                      <div className="flex items-start justify-between gap-2">
                        <div className="flex-1 min-w-0">
                          <p className="text-sm font-semibold text-on-surface truncate">
                            {notification.title}
                          </p>
                          <p className="text-xs text-secondary mt-0.5 line-clamp-2">
                            {notification.description}
                          </p>
                          <p className="text-xs text-text-muted mt-1">
                            {new Date(notification.createdAt).toLocaleDateString(
                              "pt-BR",
                              {
                                day: "2-digit",
                                month: "2-digit",
                                hour: "2-digit",
                                minute: "2-digit",
                              }
                            )}
                          </p>
                        </div>
                        <button
                          type="button"
                          onClick={(e) => {
                            e.stopPropagation();
                            markAsRead.mutate(notification.notificationId);
                          }}
                          disabled={markAsRead.isPending}
                          className="opacity-0 group-hover:opacity-100 p-1 rounded hover:bg-surface-container transition-opacity disabled:opacity-50"
                          title="Marcar como vista"
                        >
                          <Trash2 size={14} className="text-secondary" />
                        </button>
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </div>
          </div>
        </>
      )}
    </div>
  );
}
