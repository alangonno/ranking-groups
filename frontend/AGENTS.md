# Frontend Architecture

# Stack

- React 19
- Vite 6
- TypeScript 6
- React Router 7
- TanStack Query 5
- Flowbite React 0.12
- TailwindCSS 4 (via @tailwindcss/vite)
- lucide-react (ícones)
- @formkit/auto-animate (animações de lista)

---

# Estrutura do Projeto

```text
src/
 ├── app/
 │
 ├── components/
 │    ├── ui/                          ← Wrappers do Flowbite React (genéricos)
 │    │    ├── app-button.tsx
 │    │    ├── app-card.tsx
 │    │    ├── app-badge.tsx
 │    │    ├── app-modal.tsx
 │    │    ├── app-input.tsx
 │    │    ├── app-spinner.tsx
 │    │    ├── app-tabs.tsx
 │    │    └── ...
 │    │
 │    ├── authenticated/               ← Componentes de área autenticada
 │    │    ├── auth/
 │    │    ├── events/                 ← event-card, voting-card, quick-action-card, shared-event-card, shared-events-carousel
 │    │    ├── groups/                 ← group-card, create-group-modal, join-group-modal
 │    │    ├── ranking/                ← podium-card, ranking-list-item, ranking-filter, search-input, top-members-widget, stats-widget
 │    │    ├── dashboard/              ← hero-score-card, pending-votes-section, feed-tabs
 │    │    ├── members/                ← member-card
 │    │    ├── sidebar/                ← app-sidebar.tsx
 │    │    └── navigation/             ← bottom-nav.tsx
 │    │
 │    └── public/
 │         └── auth/                   ← login-form.tsx, register-form.tsx
 │
 ├── pages/
 │    ├── authenticated/
 │    │    ├── dashboard-page.tsx      ← /group/:groupId
 │    │    ├── groups-page.tsx         ← /groups
 │    │    ├── ranking-page.tsx        ← /group/:groupId/ranking
 │    │    ├── events-page.tsx         ← /group/:groupId/events
 │    │    ├── members-page.tsx        ← /group/:groupId/members
 │    │    └── profile-page.tsx        ← /group/:groupId/profile/:userId
 │    │
 │    └── public/
 │         ├── login-page.tsx
 │         └── register-page.tsx
 │
 ├── hooks/
 │    ├── use-auth.ts                  ← useLogin, useRegister, useLogout, useCurrentUser
 │    ├── use-events.ts                ← useGroupEvents, useCreateEvent, useUpdateEvent, useDeleteEvent, useVoteEvent
 │    ├── use-groups.ts                ← useGroups, useGroup, useCreateGroup, useJoinGroup, useLeaveGroup
 │    ├── use-members.ts              ← useMembers
 │    ├── use-user-profile.ts         ← useUserProfile
 │    ├── use-shared-events.ts        ← useGroupSharedEvents, useCreateSharedEvent, useJoinSharedEvent, etc.
 │    ├── use-comments.ts             ← useEventComments, useSharedEventComments, useCreateEventComment, useCreateSharedEventComment
 │    └── use-ranking.ts              ← useRanking, useFeed
 │
 ├── lib/
 │    ├── api.ts                       ← Cliente axios + wrappers HTTP (consolidado)
 │    ├── query-client.ts             ← Configuração do TanStack Query
 │    ├── auth-token.ts               ← funções puras: decodeTokenPayload, getUserIdFromToken, getUserFromToken, isTokenExpiringSoon
 │    ├── group-storage.ts            ← get/set/remove lastGroupId (localStorage)
 │    ├── use-group-context.ts        ← Hook: extrai groupId da URL em tempo real
 │    ├── mock-data.ts                ← Dados mockados para desenvolvimento
 │    ├── constants/
 │    ├── utils/
 │    └── validations/
 │
 ├── providers/
 │    ├── auth-provider.tsx            ← AuthProvider + useAuthContext (refresh silencioso no mount)
 │    └── query-provider.tsx           ← QueryClientProvider + Devtools
 │
 ├── layouts/
 │    ├── authenticated-layout.tsx    ← Sidebar + BottomNav + Main Content
 │    ├── public-layout.tsx           ← Layout limpo para login/register
 │    └── group-layout.tsx            ← Wrapper para rotas /group/:groupId/*
 │
 ├── routes/
 │    ├── public-routes.tsx
 │    ├── authenticated-routes.tsx
 │    └── router.tsx                  ← createBrowserRouter com rotas contextuais
 │
 ├── store/
 │    ├── auth-store.ts
 │    └── app-store.ts
 │
 ├── types/
 │    ├── auth/
 │    ├── event/
 │    ├── group/
 │    ├── shared-event/
 │    ├── ranking/
 │    └── common/
 │
 ├── styles/
 │
 ├── assets/
 │
 ├── main.tsx
 │
 └── vite-env.d.ts
```

---

# Organização dos Componentes

## components/ui

Contém apenas wrappers genéricos do Flowbite React.

Regra: Nunca usar Flowbite diretamente em pages ou componentes de feature. Sempre importar de `components/ui`.

Exemplos:

```text
app-button.tsx      ← wrapper de <Button>
app-card.tsx        ← wrapper de <Card>
app-badge.tsx       ← wrapper de <Badge>
app-modal.tsx       ← wrapper de <Modal>
app-input.tsx       ← wrapper de <TextInput>
app-spinner.tsx     ← wrapper de <Spinner>
app-tabs.tsx        ← wrapper de <Tabs>
```

Não conter:

- lógica de negócio
- chamadas HTTP
- dependência de feature
- estilização customizada pesada

---

# components/authenticated

Componentes usados apenas em áreas autenticadas.

Os componentes devem obrigatoriamente ser separados por feature.

Estrutura:

```text
components/authenticated/
 ├── auth/
 ├── events/
 ├── groups/
 ├── ranking/
 ├── dashboard/
 ├── sidebar/          ← app-sidebar.tsx
 └── navigation/       ← bottom-nav.tsx
```

Exemplos:

```text
components/authenticated/events/
 ├── event-card.tsx
 ├── voting-card.tsx
 ├── quick-action-card.tsx
 ├── shared-event-card.tsx
 ├── shared-events-carousel.tsx
 └── comments-section.tsx
```

---

# components/public

Componentes usados apenas em áreas públicas.

Também devem ser separados por feature.

Estrutura:

```text
components/public/
 └── auth/
      ├── login-form.tsx
      └── register-form.tsx
```

---

# Regras das Pages

As pages podem ficar diretamente dentro do contexto.

Exemplo:

```text
pages/authenticated/
 ├── dashboard-page.tsx      ← /group/:groupId
 ├── groups-page.tsx         ← /groups
 ├── ranking-page.tsx        ← /group/:groupId/ranking
 ├── events-page.tsx         ← /group/:groupId/events
 └── profile-page.tsx        ← /group/:groupId/profile
```

---

# Regras das Pages

Pages devem:

- compor componentes
- orquestrar layout
- conectar hooks
- evitar lógica pesada
- receber groupId via useParams() quando contextual ao grupo

Pages NÃO devem:

- conter chamadas HTTP diretas
- conter lógica de negócio
- conter componentes gigantes
- assumir grupo sem validar groupId

---

# Filosofia da Estrutura

A estrutura deve priorizar:

- previsibilidade
- descoberta rápida
- baixo acoplamento
- organização por feature
- separação clara de contexto (público vs autenticado)
- escalabilidade saudável
- navegação contextual ao grupo

---

# Regras de Implementação

## Obrigatório

- separar componentes por contexto (public vs authenticated)
- separar componentes por feature (events, groups, ranking, etc.)
- evitar componentes globais desnecessários
- manter componentes pequenos e focados
- evitar duplicação
- seguir estrutura definida
- validar groupId em todas as páginas contextuais

---

# Proibições

Não implementar:

- pasta components genérica sem contexto
- componentes gigantes
- lógica de negócio em componentes
- hooks dentro de pages sem reutilização
- abstrações prematuras
- dependência de localStorage para contexto de navegação (usar URL)

---

# Fluxo de Grupo (Arquitetura Central)

O sistema é centrado no conceito de **grupo**. Todo o fluxo de navegação segue este padrão:

```
Login/Register → /groups (lista de grupos)
                      │
                      ├── Criar Grupo → Modal → POST /api/groups
                      ├── Entrar via Código → Modal → POST /api/groups/join
                      └── Selecionar Grupo → /group/:groupId
                                                  │
                                                   ├── Dashboard (/group/:groupId)
                                                   ├── Ranking (/group/:groupId/ranking)
                                                   ├── Events (/group/:groupId/events)
                                                   ├── Members (/group/:groupId/members)
                                                   └── Profile (/group/:groupId/profile/:userId)
                                                  │
                                                  └── Voltar → /groups (troca de grupo)
```

Regras:

1. **Pós-login:** Sempre redirecionar para `/groups`
2. **Sem grupo selecionado:** Sidebar mostra Dashboard/Ranking/Perfil como disabled (cinza)
3. **Com grupo selecionado:** Todos os links da sidebar apontam para `/group/:groupId/*`
4. **Persistência:** Último grupo visitado salvo no localStorage (apenas para conveniência do link Dashboard)
5. **Fonte da verdade:** O `groupId` real vem sempre da URL (`useParams`), nunca do localStorage

---

# Navegação Contextual ao Grupo

## Sidebar (Desktop)

Comportamento dinâmico baseado na URL atual:

| Item | Fora de grupo (`/groups`) | Dentro de grupo (`/group/:groupId/*`) |
|---|---|---|
| Dashboard | `text-muted`, `cursor-not-allowed` | `/group/:groupId` |
| Ranking | `text-muted`, `cursor-not-allowed` | `/group/:groupId/ranking` |
| Eventos | `text-muted`, `cursor-not-allowed` | `/group/:groupId/events` |
| Membros | `text-muted`, `cursor-not-allowed` | `/group/:groupId/members` |
| Perfil | `text-muted`, `cursor-not-allowed` | `/group/:groupId/profile` |
| Meus Grupos | Ativo | `/groups` |

## BottomNav (Mobile)

Fora de grupo (`/groups`):
```
┌─────────────────────────────┐
│         [Grupos ●]          │  ← Apenas 1 item visível
└─────────────────────────────┘
```

Dentro de grupo (`/group/:groupId/*`):
```
┌─────────────────────────────┐
│ [Home][Ranking][FAB][Grupos][Membros] │
└─────────────────────────────┘
```

Links dinâmicos:
- Home → `/group/:groupId`
- Ranking → `/group/:groupId/ranking`
- Grupos → `/groups`
- Membros → `/group/:groupId/members`

---

# Regras de Hooks

Hooks devem ser organizados por feature.

Estrutura:

```text
hooks/
 ├── use-auth.ts
 ├── use-events.ts
 ├── use-groups.ts
 ├── use-shared-events.ts
 └── use-ranking.ts
```

---

# Regras dos Hooks

Hooks devem:

- encapsular chamadas HTTP
- encapsular estados
- encapsular queries/mutations
- reutilizar lógica de feature
- não conter lógica de roteamento

Hooks NÃO devem:

- renderizar UI
- conter JSX
- conter regras globais desnecessárias

---

# lib/api.ts

## Objetivo

Centralizar toda comunicação HTTP em um único arquivo.

## Estrutura

```text
lib/
 └── api.ts          ← api-client (axios), ApiError, getJson, postJson, patchJson, putJson, deleteJson
```

## Regras

Funções HTTP devem:

- retornar JSON tipado
- lançar erros padronizados (ApiError)
- centralizar headers
- centralizar autenticação (Bearer token via `authStore` em memória)
- centralizar tratamento de token
- usar `withCredentials: true` para enviar cookies cross-origin

---

# Autenticação e Tokens

## Arquitetura

- **Access Token**: JWT de 15 min guardado em `authStore` (memória JS, não localStorage). Morre no F5.
- **Refresh Token**: JWT de 7 dias enviado pelo backend como cookie `HttpOnly`, `Secure`, `SameSite`.
- **Persistência de sessão**: Apenas o cookie do refresh token persiste. O access token é restaurado via silent refresh no mount do `AuthProvider`.

## Fluxo

1. **Login/Register**: Backend seta cookie `refresh_token` + retorna `accessToken` no body. Frontend salva access token na `authStore`.
2. **F5 / Reload**: `AuthProvider` monta → verifica `authStore` vazio → faz `POST /api/auth/refresh-token` (cookie enviado auto) → restaura access token.
3. **Access Token Expirado (401)**: Axios interceptor detecta 401 → pausa fila de requests → chama `/api/auth/refresh-token` → atualiza access token → retry requests.
4. **Refresh Token Expirado/Inválido (403)**: Interceptor detecta 403 → limpa auth → redireciona `/login`.
5. **Logout**: Chama `POST /api/auth/logout` (limpa cookie no backend) + limpa access token + redireciona `/login`.

## Regras

- Nunca salvar access token no localStorage
- Nunca salvar refresh token no frontend (cookie HttpOnly é inacessível ao JS)
- O axios deve ter `withCredentials: true`

# Exemplo de Uso

```ts
await postJson<CreateEventResponse>(
  "/api/events",
  payload
)
```

---

# Regras de Types

Organizar por domínio.

Estrutura:

```text
types/
 ├── auth/
 │    └── user.ts          ← User, LoginRequest, LoginResponse, RegisterRequest, RegisterResponse, etc.
 ├── comment/
 │    └── comment.ts       ← Comment, CreateCommentRequest, EventCommentsResponse, SharedEventCommentsResponse
 ├── event/
 │    └── event.ts         ← Event, EventStatus, EventType, EventVoteType, EventApproval, CreateEventRequest, etc.
 ├── group/
 │    └── group.ts         ← Group, GroupMember, GroupRole, CreateGroupRequest, JoinGroupRequest, etc.
 ├── shared-event/
 │    └── shared-event.ts  ← SharedEvent, SharedEventParticipant, CreateSharedEventRequest, etc.
 ├── ranking/
 │    └── ranking.ts       ← RankingEntry, RankingQueryParams
 └── common/
      └── base-entity.ts   ← BaseEntity
```

Types devem conter:

- requests
- responses
- entities
- enums
- estados

Não duplicar tipos. Centralizar tudo no arquivo de domínio.

---

# Regras de Estado

Priorizar:

- TanStack Query para server state
- estado local para UI state

Evitar:

- stores globais desnecessárias
- Redux sem necessidade
- contexto excessivo

---

# Store Global

Usar apenas para:

- autenticação
- tema
- sessão

---

# Estrutura Recomendada

```text
store/
 ├── auth-store.ts
 └── app-store.ts
```

---

# Regras de Rotas

Separar:

```text
routes/
 ├── public-routes.tsx
 ├── authenticated-routes.tsx
 └── router.tsx          ← createBrowserRouter
```

---

# Padrão de Rotas

```text
Públicas:
  /login
  /register

Autenticadas - Fora de Grupo:
  /groups

Autenticadas - Dentro de Grupo (contextual):
  /group/:groupId
  /group/:groupId/ranking
  /group/:groupId/events
  /group/:groupId/members
  /group/:groupId/profile/:userId
```

---

# Layouts

Separar layouts por contexto.

```text
layouts/
 ├── authenticated-layout.tsx    ← Sidebar + BottomNav + Main Content
 ├── public-layout.tsx           ← Layout limpo para login/register
 └── group-layout.tsx            ← Wrapper para rotas /group/:groupId/*
```

---

# Regras de Segurança

Frontend nunca deve:

- confiar em permissões locais
- confiar em validações locais
- assumir autorização sem backend

Toda regra crítica deve existir no backend.

---

# Regras de Componentização

## Obrigatório

- componentes pequenos
- responsabilidade única
- composição ao invés de componentes gigantes
- evitar prop drilling excessivo

---

# Regras de Estilo

## Obrigatório

- usar Tailwind v4
- usar wrappers do Flowbite React via `components/ui`
- evitar CSS isolado sem necessidade
- evitar styled-components
- manter consistência visual
- usar sombra sutil: `shadow-[0_1px_3px_rgba(0,0,0,0.05)]`
- cards brancos puros (`#FFFFFF`) com fundo off-white (`#F9FAFB`)
- usar bordas cinza claras (`#E5E7EB`) quando necessário
- remover bordas pretas sólidas e tracejadas

---

# Regras de Implementação

## Obrigatório

- evitar over-engineering
- evitar abstrações prematuras
- evitar genericidade artificial
- implementar incrementalmente
- seguir estrutura definida
- seguir fluxo: types → hooks → component → page → integração

---

# Fluxo Obrigatório de Desenvolvimento

Para cada feature:

1. types
2. hook
3. service/api
4. component
5. page
6. integração final

---

# Regras de Contexto

O agente deve:

- trabalhar apenas na feature atual
- evitar alterar estruturas não relacionadas
- evitar refactors globais desnecessários
- respeitar contexto arquitetural definido
- validar groupId em páginas contextuais

---

# Regras de Output

O agente deve:

- responder incrementalmente
- indicar arquivos criados/modificados
- evitar reescrever arquivos inteiros sem necessidade
- justificar mudanças arquiteturais importantes
- seguir o fluxo de grupo: login → grupos → grupo → páginas

---

# Proibições

Não implementar:

- arquitetura enterprise desnecessária
- abstrações genéricas excessivas
- hooks gigantes
- componentes gigantes
- lógica de negócio em UI
- chamadas HTTP diretas em páginas
- lógica duplicada
- navegação baseada apenas em localStorage (sempre usar URL)

---

# Estratégia Geral

Prioridades do frontend:

1. simplicidade
2. clareza
3. experiência de desenvolvimento
4. modularidade
5. reutilização saudável
6. performance saudável
7. manutenção
8. navegação contextual ao grupo

---
