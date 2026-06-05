# Objetivo

Você é responsável pela implementação e manutenção do backend da aplicação.

Você deve priorizar:

- simplicidade
- previsibilidade
- modularidade
- baixo acoplamento
- clareza
- evolução incremental
- segurança de regras de negócio
- consistência arquitetural

O agente NÃO deve introduzir complexidade desnecessária.

---

# Stack Tecnológica

## Backend

- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL
- JWT Authentication

## Infraestrutura

- Docker
- Docker Compose

## Testes

- xUnit
- FluentAssertions
- NSubstitute

---

# Filosofia Arquitetural

A arquitetura do projeto é modular e pragmática.

O projeto NÃO utiliza:

- Clean Architecture completa
- CQRS complexo
- MediatR
- Event Sourcing
- Microservices
- DDD excessivo
- abstrações genéricas desnecessárias

A prioridade é:

- organização
- produtividade
- testabilidade
- manutenção
- simplicidade operacional

---

# Estrutura do Projeto

```text
src/
  ├── Controllers/
  ├── Handlers/
  │    ├── Auth/
  │    ├── Comments/
  │    ├── Events/
  │    ├── Groups/
  │    ├── Notifications/
  │    └── Rankings/
 ├── Repositories/
 ├── Entities/
 │    ├── Base/
 │    └── Enums/
 ├── Data/
 │    ├── Configurations/
 │    ├── Migrations/
 │    └── Seed/
 ├── Services/
 ├── Middleware/
 ├── Extensions/
 │    └── ConnectionStringBuilder.cs
 ├── Common/
 │    ├── Exceptions/
 │    ├── Models/
 │    └── Rules/
 └── Program.cs
```

---

# Estrutura de Testes

```text
tests/
 ├── Rules/                    # Prioridade máxima
 ├── Handlers/
 ├── Services/
 ├── Fixtures/
 ├── Builders/
 └── Mocks/
```

---

# Organização das Features

Handlers devem ser organizados por feature.

Exemplo:

```text
Handlers/
  ├── Auth/
  ├── Comments/
  ├── Events/
  ├── Groups/
  ├── Notifications/
  └── Rankings/
```

Cada arquivo de handler deve conter:

- Request
- Response
- Interface
- Implementação
- Validações relacionadas

Não criar DTOs separados.

---

# Regras Arquiteturais

## Controllers

Controllers devem:

- receber requests HTTP
- validar autenticação/autorização
- chamar handlers
- retornar responses HTTP

Controllers NÃO devem:

- conter regra de negócio
- acessar DbContext
- acessar EF diretamente
- executar queries
- conter validações complexas

Controllers devem ser finos.

---

## Handlers

Handlers são responsáveis por:

- regras de negócio
- validações
- orquestração
- persistência via repositories
- controle transacional

Handlers devem:

- trabalhar apenas no contexto da feature
- validar entradas
- manter lógica coesa
- chamar SaveChangesAsync()

Handlers NÃO devem:

- conter SQL
- conter lógica HTTP
- acessar infraestrutura externa diretamente sem necessidade

---

## Repositories

Repositories são responsáveis por:

- acesso ao banco
- queries
- includes
- paginação
- persistência EF Core

Repositories NÃO devem:

- conter regra de negócio
- conter validação de domínio
- executar SaveChangesAsync()

---

## Services

Services devem ser usados apenas para:

- regras compartilhadas
- JWT
- hashing
- autenticação
- regras puras reutilizáveis

Evitar services artificiais.

---

# Comunicação Entre Camadas

Fluxo obrigatório:

```text
Controller
   ↓
Handler
   ↓
Repository
   ↓
DbContext
```

Controllers nunca acessam repositories diretamente.

Repositories nunca chamam handlers.

---

# Entidades

Todas entidades devem herdar de:

```csharp
Entity
```

Estrutura obrigatória:

```csharp
Id
CreatedAt
UpdatedAt
```

---

# Regras do Banco

## Obrigatório

- PostgreSQL
- EF Core
- migrations
- snake_case
- timestamps automáticos
- configurações separadas por entidade

---

# AppDbContext

Local obrigatório:

```text
Data/AppDbContext.cs
```

Configurações devem ficar em:

```text
Data/Configurations/
```

Não concentrar configurações no AppDbContext.

---

# BaseRepository

O BaseRepository deve conter apenas:

- GetById
- Add
- Update
- Remove

Não implementar Generic Repository complexo.

Repositories específicos devem possuir queries específicas.

---

# SaveChanges

Repositories NÃO devem executar:

```csharp
SaveChangesAsync()
```

O controle transacional pertence ao handler.

---

# Estratégia de Testes

## Regra Obrigatória

Toda regra de negócio deve possuir testes antes da implementação funcional.

Os testes são a especificação comportamental do sistema.

---

# Prioridade dos Testes

Prioridade máxima:

- regras de negócio
- regras de aprovação
- regras de ranking
- permissões
- restrições
- segurança comportamental

Baixa prioridade:

- controllers
- EF Core
- código trivial

---

# Regras Obrigatórias de Teste

As seguintes regras devem possuir testes desde o início:

- usuário afetado não pode mexer no evento relacionado a ele
- criador não aprova sozinho evento de votação sozinho
- evento negativo inicia pendente
- score não altera antes da aprovação
- usuário fora do grupo não pode interagir
- não permitir múltiplos votos
- não permitir score zero
- impedir manipulação de ranking
- nome do grupo não pode ser vazio
- invite code deve ser único e gerado automaticamente
- usuário já membro não pode entrar novamente
- owner pode sair com transferência de ownership
- deleção em cascata do grupo (eventos, membros, aprovações)

---

# Estratégia de Implementação

Implementação deve ocorrer em pequenas etapas.

Fluxo obrigatório:

1. regra
2. teste
3. handler
4. repository
5. endpoint
6. integração

Não implementar múltiplos domínios simultaneamente.

---

# Regras de Implementação

## Obrigatório

- evitar over-engineering
- seguir arquitetura definida
- modularidade
- responsabilidade única
- baixo acoplamento
- separação correta de camadas
- métodos pequenos
- código explícito
- nomes claros
- evitar abstrações prematuras

---

# Proibições

Não implementar:

- abstrações genéricas excessivas
- factories desnecessárias
- patterns sem necessidade real
- services artificiais
- interfaces inúteis
- arquitetura enterprise desnecessária
- complexidade sem benefício real

---

# Regras de Contexto

O agente deve:

- trabalhar apenas no escopo da feature atual
- evitar modificar domínios não relacionados
- respeitar contexto hierárquico do projeto
- evitar refactors desnecessários
- evitar alterar contratos existentes sem necessidade

---

# Autenticação

## Tokens

- **Access Token**: JWT assinado com `JWT_SECRET`, expira em 15 min, claims completos (`sub`, `name`, `email`, `username`).
- **Refresh Token**: JWT assinado com `JWT_REFRESH_SECRET`, expira em 7 dias, claims mínimos (`sub` apenas).
- **Persistência**: Nenhuma. Refresh tokens são **stateless** — não há tabela no banco.

## Cookies

- Login/Register retornam access token no body + setam cookie `refresh_token` (`HttpOnly`, `Secure`, `SameSite=None` em prod, `SameSite=Lax` em dev).
- Refresh endpoint (`POST /api/auth/refresh-token`) lê o cookie automaticamente.
- Logout (`POST /api/auth/logout`) limpa o cookie.

## Erros de Refresh

- Refresh token inválido/expirado → `403 Forbidden` (não 400).
- Access token expirado → `401 Unauthorized` (padrão do middleware JWT).

# Regras de Segurança

Obrigatório:

- JWT Authentication
- validação de permissões
- validação de pertencimento ao grupo
- proteção contra múltiplos votos
- proteção contra auto aprovação
- proteção contra manipulação de ranking

Nunca confiar em validações do frontend.

---

# Regras de Código

## Obrigatório

- código legível
- previsibilidade
- simplicidade
- baixo acoplamento
- responsabilidade única

---

# Padrões de Código

## Namespaces
- Enums ficam em `src/Entities/Enums/`
- namespace: `backend.src.Entities.Enums`

## Interfaces
- Interfaces são declaradas no mesmo arquivo da implementação
- Exemplo: `IPasswordHasher` dentro de `BcryptPasswordHasher.cs`
- Não criar arquivos separados para interfaces

## Dependency Injection
- Registrado diretamente em `Program.cs`
- Não usar `ServiceCollectionExtensions.cs`
- Única exceção: utilitários como `ConnectionStringBuilder`

## Regras de Negócio
- Classes estáticas puras em `src/Common/Rules/`
- Lançam `BusinessRuleException` com código + mensagem
- Testadas unitariamente antes dos handlers

## Handlers
- Arquivo único contendo: Request, Response, Interface, Implementação, Validator
- Não criar DTOs separados
- Validator chamado dentro do HandleAsync
- SaveChangesAsync chamado no handler (nunca no repository)

---

# Regras de Output do Agente

O agente deve:

- responder incrementalmente
- indicar claramente arquivos alterados
- justificar decisões técnicas importantes
- evitar reescrever arquivos inteiros sem necessidade
- evitar alterações fora do escopo

---

# Regras de Comunicação

As respostas devem:

- ser objetivas
- focar implementação
- evitar explicações excessivas
- evitar redundância
- evitar conteúdo educacional desnecessário

---

# Limites do Agente

O agente NÃO deve:

- alterar arquitetura sem justificativa
- introduzir tecnologias não aprovadas
- modificar padrões definidos
- implementar over-engineering
- fugir do escopo atual
- adicionar complexidade desnecessária

---

# Estratégia Geral do Projeto

A prioridade do projeto é:

1. regras corretas
2. segurança comportamental
3. simplicidade
4. manutenção
5. evolução incremental
6. performance saudável
7. previsibilidade