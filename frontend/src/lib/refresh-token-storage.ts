const STORAGE_KEY = "refresh_token";

export function getRefreshToken(): string | null {
  return localStorage.getItem(STORAGE_KEY);
}

export function setRefreshToken(token: string): void {
  localStorage.setItem(STORAGE_KEY, token);
}

export function removeRefreshToken(): void {
  localStorage.removeItem(STORAGE_KEY);
}

// #Para melhor experiencia em SPA e nao hospedagem paga do servico
// salvando temporariamente em local storage mesmo nao sendo o melhor
// padrao - em breve usar o fluxo correto (cookie HttpOnly com mesma origem)
//
// Fluxo via cookie (nao utilizado atualmente - mantido para referencia):
// O cookie HttpOnly e enviado automaticamente pelo navegador em requests
// com withCredentials: true, sem necessidade de manipulacao via JS.
// Quando migrar para infraestrutura com mesma origem (nginx),
// basta remover o uso do localStorage e depender apenas do cookie.
