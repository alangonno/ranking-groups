export function formatRelativeTime(dateString: string): string {
  const date = new Date(dateString);
  const now = new Date();
  const diffMs = now.getTime() - date.getTime();
  const diffMin = Math.floor(diffMs / 60000);
  const diffHours = Math.floor(diffMin / 60);
  const diffDays = Math.floor(diffHours / 24);
  const diffWeeks = Math.floor(diffDays / 7);
  const diffMonths = Math.floor(diffDays / 30);
  const diffYears = Math.floor(diffDays / 365);

  if (diffMin < 1) return "Agora mesmo";

  let relative: string;
  if (diffMin < 60) {
    relative = `${diffMin} min atrás`;
  } else if (diffHours < 24) {
    relative = `${diffHours} hora${diffHours > 1 ? "s" : ""} atrás`;
  } else if (diffDays < 7) {
    relative = `${diffDays} dia${diffDays > 1 ? "s" : ""} atrás`;
  } else if (diffWeeks < 4) {
    relative = `${diffWeeks} semana${diffWeeks > 1 ? "s" : ""} atrás`;
  } else if (diffMonths < 12) {
    relative = `${diffMonths} mês${diffMonths > 1 ? "es" : ""} atrás`;
  } else {
    relative = `${diffYears} ano${diffYears > 1 ? "s" : ""} atrás`;
  }

  const time = date.toLocaleTimeString("pt-BR", { hour: "2-digit", minute: "2-digit" });
  const dayMonth = date.toLocaleDateString("pt-BR", { day: "2-digit", month: "2-digit" });
  const year = date.getFullYear();

  return `${relative} (${dayMonth}/${year} ${time})`;
}
