const LAST_GROUP_KEY = "last_group_id";

export function setLastGroupId(groupId: string): void {
  localStorage.setItem(LAST_GROUP_KEY, groupId);
}

export function getLastGroupId(): string | null {
  return localStorage.getItem(LAST_GROUP_KEY);
}

export function removeLastGroupId(): void {
  localStorage.removeItem(LAST_GROUP_KEY);
}
