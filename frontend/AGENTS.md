# Frontend Architecture

# Stack

- React
- Vite
- TypeScript
- React Router
- TanStack Query
- Flowbite React
- TailwindCSS

---

# Estrutura do Projeto

```text
src/
 ├── app/
 │
 ├── components/
 │    ├── ui/
 │    │
 │    ├── authenticated/
 │    │    ├── auth/
 │    │    ├── events/
 │    │    ├── groups/
 │    │    ├── ranking/
 │    │    └── profile/
 │    │
 │    └── public/
 │         ├── auth/
 │         └── landing/
 │
 ├── pages/
 │    ├── authenticated/
 │    │    ├── dashboard-page.tsx
 │    │    ├── group-page.tsx
 │    │    ├── ranking-page.tsx
 │    │    └── profile-page.tsx
 │    │
 │    └── public/
 │         ├── login-page.tsx
 │         ├── register-page.tsx
 │         └── landing-page.tsx
 │
 ├── hooks/
 │    ├── use-auth/
 │    ├── use-events/
 │    ├── use-groups/
 │    └── use-ranking/
 │
 ├── lib/
 │    ├── api/
 │    │    ├── api-client.ts
 │    │    ├── api-error.ts
 │    │    ├── get-json.ts
 │    │    ├── post-json.ts
 │    │    ├── patch-json.ts
 │    │    ├── put-json.ts
 │    │    └── delete-json.ts
 │    │
 │    ├── constants/
 │    ├── utils/
 │    └── validations/
 │
 ├── services/
 │
 ├── providers/
 │
 ├── layouts/
 │    ├── authenticated-layout.tsx
 │    └── public-layout.tsx
 │
 ├── routes/
 │    ├── authenticated-routes.tsx
 │    ├── public-routes.tsx
 │    └── router.tsx
 │
 ├── store/
 │    ├── auth-store.ts
 │    └── app-store.ts
 │
 ├── types/
 │    ├── auth/
 │    ├── event/
 │    ├── group/
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

Contém apenas componentes reutilizáveis e genéricos.

Exemplos:

```text
button.tsx
dialog.tsx
input.tsx
card.tsx
badge.tsx
table.tsx
```

Não conter:

- lógica de negócio
- chamadas HTTP
- dependência de feature

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
 └── profile/
```

Exemplos:

```text
components/authenticated/events/
 ├── create-event-modal.tsx
 ├── event-feed.tsx
 ├── event-card.tsx
 └── approve-event-button.tsx
```

---

# components/public

Componentes usados apenas em áreas públicas.

Também devem ser separados por feature.

Estrutura:

```text
components/public/
 ├── auth/
 └── landing/
```

Exemplos:

```text
components/public/auth/
 ├── login-form.tsx
 └── register-form.tsx
```

---

# Regras das Pages

As pages podem ficar diretamente dentro do contexto.

Exemplo:

```text
pages/authenticated/
 ├── dashboard-page.tsx
 ├── group-page.tsx
 └── ranking-page.tsx
```

---

# Regras das Pages

Pages devem:

- compor componentes
- orquestrar layout
- conectar hooks
- evitar lógica pesada

Pages NÃO devem:

- conter chamadas HTTP diretas
- conter lógica de negócio
- conter componentes gigantes

---

# Filosofia da Estrutura

A estrutura deve priorizar:

- previsibilidade
- descoberta rápida
- baixo acoplamento
- organização por feature
- separação clara de contexto
- escalabilidade saudável

---

# Regras de Implementação

## Obrigatório

- separar componentes por contexto
- separar componentes por feature
- evitar componentes globais desnecessários
- manter componentes pequenos
- evitar duplicação
- seguir estrutura definida

---

# Proibições

Não implementar:

- pasta components genérica sem contexto
- componentes gigantes
- lógica de negócio em componentes
- hooks dentro de pages sem reutilização
- abstrações prematuras

---

# Regras de Hooks

Hooks devem ser organizados por feature.

Estrutura:

```text
hooks/
 ├── use-auth/
 ├── use-events/
 ├── use-groups/
 └── use-ranking/
```

---

# Regras dos Hooks

Hooks devem:

- encapsular chamadas HTTP
- encapsular estados
- encapsular queries/mutations
- reutilizar lógica de feature

Hooks NÃO devem:

- renderizar UI
- conter JSX
- conter regras globais desnecessárias

---

# Estrutura Recomendada dos Hooks

Exemplo:

```text
hooks/use-events/
 ├── use-create-event.ts
 ├── use-approve-event.ts
 ├── use-group-events.ts
 └── use-event-permissions.ts
```

---

# Regras da Pasta lib/api

## Objetivo

Centralizar toda comunicação HTTP.

---

# Estrutura

```text
lib/api/
 ├── api-client.ts
 ├── api-error.ts
 ├── get-json.ts
 ├── post-json.ts
 ├── patch-json.ts
 ├── put-json.ts
 └── delete-json.ts
```

---

# Regras

Funções HTTP devem:

- retornar JSON tipado
- lançar erros padronizados
- centralizar headers
- centralizar autenticação
- centralizar tratamento de token

---

# Exemplo de Uso

```ts
await postJson<CreateEventResponse>(
  "/events",
  payload
)
```

---

# api-client.ts

Responsável por:

- baseURL
- Authorization
- interceptors
- tratamento global de erro

---

# Regras de Types

Organizar por domínio.

Estrutura:

```text
types/
 ├── auth/
 ├── event/
 ├── group/
 ├── ranking/
 └── common/
```

---

# Regras de Types

Types devem conter:

- requests
- responses
- entities
- enums
- estados

Não duplicar tipos.

---

# Exemplo

```text
types/event/
 ├── create-event-request.ts
 ├── create-event-response.ts
 ├── event.ts
 └── event-status.ts
```

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
 ├── authenticated-routes.tsx
 ├── public-routes.tsx
 └── router.tsx
```

---

# Layouts

Separar layouts por contexto.

```text
layouts/
 ├── authenticated-layout.tsx
 └── public-layout.tsx
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

- usar Tailwind
- usar shadcn/ui
- evitar CSS isolado sem necessidade
- evitar styled-components
- manter consistência visual

---

# Regras de Implementação

## Obrigatório

- evitar over-engineering
- evitar abstrações prematuras
- evitar genericidade artificial
- implementar incrementalmente
- seguir estrutura definida

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

---

# Regras de Output

O agente deve:

- responder incrementalmente
- indicar arquivos criados/modificados
- evitar reescrever arquivos inteiros sem necessidade
- justificar mudanças arquiteturais importantes

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


---